using Diogen.Analyzers.Common.Generators;
using Diogen.Analyzers.Common.Generators.AggregatedServices;
using Microsoft.CodeAnalysis;

namespace Diogen.Extensions.DependencyInjection.Analyzers.Generators.AggregatedServices;

[Generator]
public class AggregatedServicesGenerator : AggregatedServicesGeneratorBase
{
    protected override string AggregatedServicesAttributeType =>
        "Diogen.Extensions.DependencyInjection.Generators.AggregatedServicesAttribute";

    protected override void AppendKeyAttribute(IndentedStringBuilder builder, string keyExpression)
    {
        builder.Append($"[global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute({keyExpression})] ");
    }
}
