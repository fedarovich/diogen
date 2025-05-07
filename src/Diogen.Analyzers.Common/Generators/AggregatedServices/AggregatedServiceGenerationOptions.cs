using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public record struct AggregatedServiceGenerationOptions(
    string? CustomName,
    GeneratedTypeAccessibility Accessibility,
    bool IsSealed,
    GeneratedTypeKind Kind,
    GeneratedTypeLocation Location
);
