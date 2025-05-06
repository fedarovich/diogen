using System.Collections.Immutable;
using System.Linq;
using Diogen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public abstract class AggregatedServicesGeneratorBase : IIncrementalGenerator
{
    protected abstract string AggregatedServicesAttributeType { get; }

    protected abstract void AppendKeyAttribute(IndentedStringBuilder builder, string keyExpression);

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
                    attribute.GetVisibility() ?? GeneratedTypeVisibility.Public,
                    attribute.IsSealed() ?? false,
                    attribute.GetKind() ?? GeneratedTypeKind.Record);

                var properties = @interface
                    .GetMembers()
                    .OfType<IPropertySymbol>()
                    .Where(p => p is { IsReadOnly: true, IsAbstract: true, IsStatic: false, DeclaredAccessibility: Accessibility.Public })
                    .Select(GetDependencyInfo)
                    .OrderBy(p => p.Optional);

                var typeParameters = @interface
                    .TypeParameters
                    .Select(tp => new TypeParameter(tp.Name, GetTypeConstraints(tp)));

                return new AggregatedServiceInfo(
                    @interface.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat),
                    @interface.Name,
                    new EquatableImmutableArray<TypeParameter>(typeParameters),
                    new EquatableImmutableArray<DependencyInfo>(properties),
                    options);
            });

        context.RegisterSourceOutput(aggregatedServicesInterfaces.Where(info => info.HasValue).Select((info, _) => info!.Value),
            (ctx, info) =>
            {
                var builder = new IndentedStringBuilder();
                builder.AppendLine("#nullable enable");
                builder.AppendLine();

                bool hasNamespace = !string.IsNullOrEmpty(info.Namespace);

                if (hasNamespace)
                {
                    builder.AppendLine($"namespace {info.Namespace.AsSpan(8)};"); // remove the global:: prefix
                    builder.AppendLine();
                }

                var options = info.Options;

                bool isClass = options.Kind == GeneratedTypeKind.Class;

                var className = info.InterfaceName[1..];
                builder.Append($"{options.Visibility.ToCSharpString()}{(options.IsSealed ? "sealed " : "")}partial {(isClass ? "class" : "record")} {className}");

                AppendTypeParameters(info, builder);

                builder.AppendLine("(");

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

                builder.Append(") : ");
                if (!string.IsNullOrEmpty(info.Namespace))
                {
                    builder.Append(info.Namespace);
                    builder.Append('.');
                }
                builder.Append(info.InterfaceName);
                AppendTypeParameters(info, builder);

                foreach (var tp in info.TypeParameters)
                {
                    if (tp.Constraints is not [])
                    {
                        builder.AppendLine();
                        using (builder.Indent())
                        {
                            builder.Append($"where {tp.Name} : ");
                            builder.AppendJoin(", ", tp.Constraints);
                        }
                    }
                }

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

                ctx.AddSource("Dependencies.cs", builder.ToString());
            });
    }

    private static void AppendTypeParameters(AggregatedServiceInfo info, IndentedStringBuilder builder)
    {
        if (info.TypeParameters is not [])
        {
            builder.Append("<");
            builder.AppendJoin(", ", info.TypeParameters.Select(tp => tp.Name));
            builder.Append(">");
        }
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
}
