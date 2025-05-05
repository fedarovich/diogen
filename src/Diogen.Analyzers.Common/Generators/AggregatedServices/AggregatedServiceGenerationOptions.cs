using System;
using System.Collections.Generic;
using System.Text;
using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public record struct AggregatedServiceGenerationOptions(
    GeneratedTypeVisibility Visibility,
    bool IsSealed,
    GeneratedTypeKind Kind
);
