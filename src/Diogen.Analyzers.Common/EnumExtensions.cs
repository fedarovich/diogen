using Diogen.Generators;

namespace Diogen.Analyzers.Common;

public static class EnumExtensions
{
    public static string ToCSharpString(this GeneratedTypeAccessibility accessibility) =>
        accessibility switch
        {
            GeneratedTypeAccessibility.File => "file ",
            GeneratedTypeAccessibility.Default => string.Empty,
            GeneratedTypeAccessibility.Private => "private ",
            GeneratedTypeAccessibility.PrivateProtected => "private protected ",
            GeneratedTypeAccessibility.Protected => "protected ",
            GeneratedTypeAccessibility.Internal => "internal ",
            GeneratedTypeAccessibility.ProtectedInternal => "protected internal ",
            GeneratedTypeAccessibility.Public => "public ",
            _ => throw new ArgumentOutOfRangeException(nameof(accessibility), accessibility, null)
        };
}
