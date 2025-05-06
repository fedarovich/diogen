namespace Diogen.Analyzers.Common.Generators;

public record ContainingType(ContainingTypeKind Kind, bool IsRecord, string Name, EquatableImmutableArray<TypeParameter> TypeParameters);