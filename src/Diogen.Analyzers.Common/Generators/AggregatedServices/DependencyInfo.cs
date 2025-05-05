namespace Diogen.Analyzers.Common.Generators.AggregatedServices;

public readonly record struct DependencyInfo(string Type, string Name, string? Key, bool Optional);
