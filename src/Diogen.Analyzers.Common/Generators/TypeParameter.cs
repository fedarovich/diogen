namespace Diogen.Analyzers.Common.Generators;

public readonly record struct TypeParameter(string Name, EquatableImmutableArray<string> Constraints);
