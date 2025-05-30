﻿using System.Collections.Immutable;
using System.Text;
using Diogen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public abstract class AggregatedServicesGeneratorBase : IIncrementalGenerator
{
    private static readonly Version FallbackVersion = new(1, 0, 0, 0);

    private Version? _version;

    protected abstract string AggregatedServicesAttributeType { get; }

    protected abstract void AppendKeyAttribute(IndentedStringBuilder builder, string keyExpression);

    protected virtual Version Version => LazyInitializer.EnsureInitialized(ref _version, () => GetType().Assembly.GetName().Version) ?? FallbackVersion;

    public virtual ImmutableArray<string> KeyAttributes => ImmutableArray.Create("global::Diogen.Generators.KeyedAttribute");

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var aggregatedServicesInterfaces = context.SyntaxProvider.ForAttributeWithMetadataName(
            AggregatedServicesAttributeType,
            (node, _) => node is InterfaceDeclarationSyntax,
            (attributeContext, _) =>
            {
                var @interface = attributeContext.SemanticModel.GetDeclaredSymbol(attributeContext.TargetNode) as INamedTypeSymbol;
                if (@interface is null)
                    return default(AggregatedServiceInfo?);

                var attribute = attributeContext.Attributes[0];
                var options = new AggregatedServiceGenerationOptions(
                    attribute.GetName(),
                    attribute.GetAccessibility() ?? GeneratedTypeAccessibility.Public,
                    attribute.IsSealed() ?? false,
                    attribute.GetKind() ?? GeneratedTypeKind.Record,
                    attribute.GetLocation() ?? GeneratedTypeLocation.Sibling);

                return new AggregatedServiceInfo(
                    @interface.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    @interface.Name,
                    GetContainingTypes(@interface),
                    GetTypeParameters(@interface),
                    GetDependencies(@interface),
                    options);
            });

        context.RegisterSourceOutput(aggregatedServicesInterfaces.Where(info => info.HasValue).Select((info, _) => info!.Value),
            (ctx, info) =>
            {
                var builder = new IndentedStringBuilder();

                builder.AppendCommonHeader();

                builder.AppendNamespace(info.Namespace);

                if (info.Options.Location != GeneratedTypeLocation.TopLevel)
                {
                    foreach (var containingType in info.ContainingTypes)
                    {
                        builder.AppendContainingTypeHeader(containingType);
                    }
                }

                if (info.Options.Location == GeneratedTypeLocation.Nested)
                {
                    builder.AppendContainingInterfaceHeader(info.InterfaceName, info.TypeParameters);
                }

                var options = info.Options;
                bool isClass = options.Kind == GeneratedTypeKind.Class;
                var className = string.IsNullOrWhiteSpace(options.CustomName) ? info.InterfaceName[1..] : options.CustomName!.Trim();

                builder
                    .AppendGeneratedCodeAttribute(GetType().FullName!, Version)
                    .AppendGeneratedClassHeader(className, info.TypeParameters, options)
                    .AppendLine("(");

                using (builder.Indent())
                {
                    int dependencyIndex = 0;
                    foreach (var dependency in info.Dependencies)
                    {
                        AppendParameter(builder, dependency, isClass);
                        if (++dependencyIndex < info.Dependencies.Count)
                        {
                            builder.Append(',');
                        }

                        builder.AppendLine();
                    }
                }

                builder
                    .Append(") : ")
                    .AppendImplementedInterfaceNameAndConstraints(info.Namespace, info.InterfaceName, info.TypeParameters, info.ContainingTypes);

                if (isClass)
                {
                    builder.AppendLine();

                    builder.AppendLine("{");

                    using (builder.Indent())
                    {
                        foreach (var dependency in info.Dependencies)
                        {
                            AppendProperty(builder, dependency);
                        }
                    }

                    builder.AppendLine("}");
                }
                else
                {
                    builder.AppendLine(";");
                }

                builder.CloseAllScopes();

                var sourceName = GetSourceName(info, className);
                ctx.AddSource(sourceName, builder.ToString());
            });
    }

    

    protected EquatableImmutableArray<TypeParameter> GetTypeParameters(INamedTypeSymbol @interface)
    {
        return @interface
            .TypeParameters
            .Select(tp => new TypeParameter(tp.Name, GetTypeConstraints(tp)))
            .ToImmutableArray();
    }

    protected EquatableImmutableArray<DependencyInfo> GetDependencies(INamedTypeSymbol @interface)
    {
        var properties = @interface
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p is { IsReadOnly: true, IsAbstract: true, IsStatic: false, DeclaredAccessibility: Accessibility.Public })
            .Select(GetDependencyInfo)
            .OrderBy(p => p.Optional);
        return properties.ToImmutableArray();
    }

    protected EquatableImmutableArray<ContainingType> GetContainingTypes(INamedTypeSymbol @interface)
    {
        if (@interface.ContainingType is null)
            return default;

        var containingTypes = new List<ContainingType>(4);
        var containingType = @interface.ContainingType;
        do
        {
            var (kind, isRecord) = containingType.TypeKind switch
            {
                TypeKind.Class => (ContainingTypeKind.Class, containingType.IsRecord),
                TypeKind.Struct => (ContainingTypeKind.Struct, containingType.IsRecord),
                TypeKind.Interface => (ContainingTypeKind.Interface, false),
                _ => (ContainingTypeKind.Unsupported, false)
            };

            containingTypes.Add(new ContainingType(kind, isRecord, containingType.Name, GetTypeParameters(containingType)));
            containingType = containingType.ContainingType;
        } while (containingType is not null);

        containingTypes.Reverse();
        return containingTypes.ToImmutableArray();
    }
    
    protected virtual EquatableImmutableArray<string> GetTypeConstraints(ITypeParameterSymbol tp)
    {
        bool hasTypeKindConstraint = tp.HasReferenceTypeConstraint | tp.HasValueTypeConstraint |
                                     tp.HasUnmanagedTypeConstraint | tp.HasNotNullConstraint;
        var count = (hasTypeKindConstraint ? 1 : 0) + tp.ConstraintTypes.Length + (tp.HasConstructorConstraint ? 1 : 0);
        if (count == 0)
            return default;

        var constraints = new List<string>(count);
        if (hasTypeKindConstraint)
        {
            if (tp.HasReferenceTypeConstraint)
            {
                constraints.Add("class");
            }
            else if (tp.HasValueTypeConstraint)
            {
                constraints.Add("struct");
            }
            else if (tp.HasNotNullConstraint)
            {
                constraints.Add("notnull");
            }
            else if (tp.HasUnmanagedTypeConstraint)
            {
                constraints.Add("unmanaged");
            }
        }

        constraints.AddRange(tp.ConstraintTypes.Select(type => type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

        if (tp.HasConstructorConstraint)
        {
            constraints.Add("new()");
        }

        return constraints.ToImmutableArray();
    }

    protected virtual DependencyInfo GetDependencyInfo(IPropertySymbol p)
    {
        var typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var name = p.Name;

        var attributes = p.GetAttributes();
        var keyAttribute = attributes.FirstOrDefault(a => KeyAttributes.Contains(a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)!));
        var key = keyAttribute != null ? GetKeyValue(keyAttribute) : null;

        return new DependencyInfo(
            typeName,
            name,
            key,
            IsOptional(p));
    }

    private string? GetKeyValue(AttributeData attributeData)
    {
        var keyValue = GetKeyValueCore(attributeData);

        if (keyValue is null)
            return null;

        switch (keyValue.Value.Kind)
        {
            case TypedConstantKind.Enum:
                return "global::" + keyValue.Value.ToCSharpString();
            case TypedConstantKind.Primitive:
            case TypedConstantKind.Array:
                return keyValue.Value.ToCSharpString();
            case TypedConstantKind.Type:
                return $"typeof({((INamedTypeSymbol) keyValue.Value.Value!).ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})";
            default:
                return null;
        }
    }

    protected virtual TypedConstant? GetKeyValueCore(AttributeData attributeData)
    {
        return attributeData.ConstructorArguments is [var keyValue] ? keyValue : null;
    }

    protected virtual void AppendParameter(IndentedStringBuilder builder, DependencyInfo dependency, bool camelCase = false)
    {
        if (dependency.Key != null)
        {
            AppendKeyAttribute(builder, dependency.Key);
        }

        builder.Append(dependency.Type);
        if (dependency.Optional)
        {
            builder.Append('?');
        }

        builder.Append(' ');
        var name = camelCase
            ? dependency.Name.ToCamelCase()
            : dependency.Name;
        builder.Append(name);

        if (dependency.Optional)
        {
            builder.Append(" = null");
        }
    }

    protected virtual void AppendProperty(IndentedStringBuilder builder, DependencyInfo dependency)
    {
        builder.AppendLine(
            $$"""public {{dependency.Type}}{{(dependency.Optional ? "?" : "")}} {{dependency.Name}} { get; } = {{dependency.Name.ToCamelCase()}};""");
    }

    protected virtual bool IsOptional(IPropertySymbol property) =>
        property.NullableAnnotation switch
        {
            NullableAnnotation.NotAnnotated => false,
            NullableAnnotation.Annotated => true,
            _ => property.GetAttributes().Any(a =>
                a.AttributeClass?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::Diogen.Generators.OptionalAttribute")
        };

    protected virtual string GetSourceName(AggregatedServiceInfo info, string generatedTypeName)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrEmpty(info.Namespace))
        {
            builder.Append(info.Namespace.Substring(8)); // Remove global:: prefix
            builder.Append('.');
        }

        if (info.Options.Location != GeneratedTypeLocation.TopLevel)
        {
            foreach (var containingType in info.ContainingTypes)
            {
                builder.Append(containingType.Name);
                if (containingType.TypeParameters.Length > 0)
                {
                    builder.Append("`");
                    builder.Append(containingType.TypeParameters.Length);
                }

                builder.Append(".");
            }
        }

        if (info.Options.Location == GeneratedTypeLocation.Nested)
        {
            builder.Append(info.InterfaceName);
            if (info.TypeParameters.Length > 0)
            {
                builder.Append("`");
                builder.Append(info.TypeParameters.Length);
            }

            builder.Append(".");
        }

        builder.Append(generatedTypeName);
        if (info.Options.Location != GeneratedTypeLocation.Nested && info.TypeParameters.Length > 0)
        {
            builder.Append("`");
            builder.Append(info.TypeParameters.Length);
        }
        builder.Append(".cs");
        return builder.ToString();
    }
}
