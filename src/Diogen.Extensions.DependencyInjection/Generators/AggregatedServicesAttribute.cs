using Diogen.Generators;

namespace Diogen.Extensions.DependencyInjection.Generators;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class AggregatedServicesAttribute : Attribute
{
    public GeneratedTypeLocation Location { get; set; } = GeneratedTypeLocation.SameLevel;

    public GeneratedTypeAccessibility Accessibility { get; set; } = GeneratedTypeAccessibility.Public;

    public GeneratedTypeKind Kind { get; set; } = GeneratedTypeKind.Record;

    public bool IsSealed { get; set; }
}
