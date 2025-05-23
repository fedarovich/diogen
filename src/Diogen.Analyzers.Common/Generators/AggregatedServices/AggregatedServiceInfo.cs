﻿using Diogen.Generators;

namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public readonly record struct AggregatedServiceInfo(
    string Namespace, 
    string InterfaceName,
    EquatableImmutableArray<ContainingType> ContainingTypes,
    EquatableImmutableArray<TypeParameter> TypeParameters,
    EquatableImmutableArray<DependencyInfo> Dependencies,
    AggregatedServiceGenerationOptions Options)
{
}
