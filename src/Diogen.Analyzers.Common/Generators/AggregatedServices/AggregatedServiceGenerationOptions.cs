using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public record struct AggregatedServiceGenerationOptions(
    GeneratedTypeAccessibility Accessibility,
    bool IsSealed,
    GeneratedTypeKind Kind,
    GeneratedTypeLocation Location
);
