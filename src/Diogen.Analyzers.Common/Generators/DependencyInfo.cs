namespace Diogen.Analyzers.Common.Generators;

public readonly record struct DependencyInfo(string Type, string Name, string? Key, bool Optional);
