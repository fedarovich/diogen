using Diogen.Generators;

namespace Diogen.Analyzers.Common;

public static class EnumExtensions
{
    public static string ToCSharpString(this GeneratedTypeVisibility visibility) =>
        visibility switch
        {
            GeneratedTypeVisibility.File => "file ",
            GeneratedTypeVisibility.Default => string.Empty,
            GeneratedTypeVisibility.Private => "private ",
            GeneratedTypeVisibility.PrivateProtected => "private protected ",
            GeneratedTypeVisibility.Protected => "protected ",
            GeneratedTypeVisibility.Internal => "internal ",
            GeneratedTypeVisibility.ProtectedInternal => "protected internal ",
            GeneratedTypeVisibility.Public => "public ",
            _ => throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null)
        };
}
