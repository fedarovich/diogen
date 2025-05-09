using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators;

public interface IGenerationOptions
{
    GeneratedTypeAccessibility Accessibility { get; set; }
    bool IsSealed { get; set; }
    GeneratedTypeKind Kind { get; set; }
    GeneratedTypeLocation Location { get; set; }
}