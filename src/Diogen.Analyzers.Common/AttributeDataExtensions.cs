using Diogen.Generators;
using Microsoft.CodeAnalysis;

namespace Diogen.Analyzers.Common;

internal static class AttributeDataExtensions
{
    public static GeneratedTypeVisibility? GetVisibility(this AttributeData attributeData)
    {
        var kv = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "Visibility");
        if (kv.Key is null)
            return null;

        var value = kv.Value.Value as int?;
        if (value is null)
            return null;

        return (GeneratedTypeVisibility)value.Value;
    }

    public static GeneratedTypeKind? GetKind(this AttributeData attributeData)
    {
        var kv = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "Kind");
        if (kv.Key is null)
            return null;

        var value = kv.Value.Value as int?;
        if (value is null)
            return null;

        return (GeneratedTypeKind)value.Value;
    }

    public static bool? IsSealed(this AttributeData attributeData)
    {
        var kv = attributeData.NamedArguments.FirstOrDefault(a => a.Key == "IsSealed");
        if (kv.Key is null)
            return null;

        return kv.Value.Value is bool boolValue ? boolValue : null;
    }
}
