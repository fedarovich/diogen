using Microsoft.CodeAnalysis.Testing;

namespace Diogen.Extensions.DependencyInjection.Analyzers.Tests.Verifiers;

public static partial class CSharpSourceGeneratorVerifier<TSourceGenerator>
    where TSourceGenerator : new()
{
    public static async Task VerifySourcesGeneratorAsync(string sourceCode, string generatedCode, string filename)
    {
        var test = new Test
        {
            TestState =
            {
                Sources = { sourceCode },
                GeneratedSources =
                {
                    (typeof(TSourceGenerator), filename, generatedCode)
                },
                ReferenceAssemblies = ReferenceAssemblies.Net.Net80
            }
            
        };

        await test.RunAsync(CancellationToken.None);
    }
}