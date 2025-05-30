﻿using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators;

public static class IndentedStringBuilderExtensions
{
    public static IndentedStringBuilder AppendCommonHeader(this IndentedStringBuilder builder) =>
        builder
            .AppendLine("// <auto-generated/>")
            .AppendLine()
            .AppendLine("#nullable enable annotations")
            .AppendLine("#nullable disable warnings")
            .AppendLine()
            .AppendLine("// Suppress warnings about [Obsolete] member usage in generated code.")
            .AppendLine("#pragma warning disable CS0612, CS0618")
            .AppendLine();

    public static IndentedStringBuilder AppendNamespace(this IndentedStringBuilder builder, string? @namespace)
    {
        if (!string.IsNullOrWhiteSpace(@namespace))
        {
            if (@namespace!.StartsWith("global::", StringComparison.Ordinal))
            {
                // remove global:: prefix
                builder.AppendLine($"namespace {@namespace.AsSpan(8)};");
            }
            else
            {
                builder.AppendLine($"namespace {@namespace};");
            }
            builder.AppendLine();
        }

        return builder;
    }

    public static IndentedStringBuilder AppendTypeParameters(this IndentedStringBuilder builder, EquatableImmutableArray<TypeParameter> typeParameters)
    {
        if (typeParameters is not [])
        {
            builder
                .Append("<")
                .AppendJoin(", ", typeParameters.Select(tp => tp.Name))
                .Append(">");
        }

        return builder;
    }

    public static IndentedStringBuilder AppendContainingTypeHeader(this IndentedStringBuilder builder, ContainingType containingType) =>
        builder
            .Append($"partial {(containingType.IsRecord ? "record " : "")}{containingType.Kind.ToString().ToLowerInvariant()} {containingType.Name}")
            .AppendTypeParameters(containingType.TypeParameters)
            .AppendLine()
            .AppendLine("{")
            .IncrementIndent();

    public static IndentedStringBuilder AppendContainingInterfaceHeader(
        this IndentedStringBuilder builder,
        string interfaceName,
        EquatableImmutableArray<TypeParameter> typeParameters
    ) =>
        builder
            .Append($"partial interface {interfaceName}")
            .AppendTypeParameters(typeParameters)
            .AppendLine()
            .AppendLine("{")
            .IncrementIndent();

    public static IndentedStringBuilder AppendGeneratedCodeAttribute(this IndentedStringBuilder builder, string toolName, Version toolVersion) =>
        builder.AppendLine(
            $"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("{toolName}", "{toolVersion}")]""");


    public static IndentedStringBuilder AppendGeneratedClassHeader<TOptions>(
        this IndentedStringBuilder builder,
        string name,
        EquatableImmutableArray<TypeParameter> typeParameters,
        TOptions options) where TOptions : IGenerationOptions
    {
        bool isClass = options.Kind == GeneratedTypeKind.Class;

        builder.Append($"{options.Accessibility.ToCSharpString()}{(options.IsSealed ? "sealed " : "")}partial {(isClass ? "class" : "record")} {name}");

        if (options.Location != GeneratedTypeLocation.Nested)
        {
            builder.AppendTypeParameters(typeParameters);
        }

        return builder;
    }

    public static IndentedStringBuilder AppendImplementedInterfaceNameAndConstraints(
        this IndentedStringBuilder builder, 
        string? @namespace, 
        string interfaceName, 
        EquatableImmutableArray<TypeParameter> typeParameters, 
        EquatableImmutableArray<ContainingType> containingTypes)
    {
        if (!string.IsNullOrWhiteSpace(@namespace))
        {
            builder.Append(@namespace!).Append('.');
        }

        foreach (var containingType in containingTypes)
        {
            builder.Append(containingType.Name);
            builder.AppendTypeParameters(containingType.TypeParameters);
            builder.Append('.');
        }
        builder.Append(interfaceName);
        builder.AppendTypeParameters(typeParameters);

        foreach (var tp in typeParameters)
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

        return builder;
    }

    public static IndentedStringBuilder CloseAllScopes(this IndentedStringBuilder builder)
    {
        while (builder.IndentCount > 0)
        {
            builder.DecrementIndent();
            builder.AppendLine("}");
        }

        return builder;
    }
}
