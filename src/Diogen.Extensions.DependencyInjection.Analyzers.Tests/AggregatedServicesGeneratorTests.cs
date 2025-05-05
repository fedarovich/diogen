using System;
using System.Collections.Generic;
using Diogen.Analyzers.Common;
using Diogen.Generators;
using VerifyCS = Diogen.Extensions.DependencyInjection.Analyzers.Tests.Verifiers.CSharpSourceGeneratorVerifier<
    Diogen.Extensions.DependencyInjection.Analyzers.Generators.AggregatedServices.AggregatedServicesGenerator>;
using static Diogen.Generators.GeneratedTypeVisibility;

namespace Diogen.Extensions.DependencyInjection.Analyzers.Tests;

public class AggregatedServicesGeneratorTests
{
    [Test]
    public async Task TopLevelInterfaceWithNullable_GeneratesTopLevelRecord(
        [Values(Default, Public, Internal, GeneratedTypeVisibility.File)] GeneratedTypeVisibility visibility,
        [Values(false, true)] bool isSealed)
    {
        var sourceCode =
            $$"""
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface IRequiredService { }
            
            public interface IOptionalService { }
            
            public interface IKeyedService { }
            
            public enum KeyEnum
            {
                Key1
            }
            
            [AggregatedServices(
                Visibility = GeneratedTypeVisibility.{{visibility}},
                IsSealed = {{isSealed.ToString().ToLowerInvariant()}}
            )]
            public interface IDependencies
            {
                IRequiredService Required { get; }
                
                IOptionalService? Optional { get; }
                
                [Keyed("string-key")] IKeyedService KeyedWithStringKey { get; }
                
                [Keyed(42)] IKeyedService KeyedWithIntKey { get; }
                
                [Keyed(KeyEnum.Key1)] IKeyedService KeyedWithEnumKey { get; }
                
                [Keyed(typeof(int))] IKeyedService KeyedWithBuiltInTypeKey { get; }
                
                [Keyed(typeof(DayOfWeek))] IKeyedService KeyedWithTypeKey { get; }
            }
            """;

        var generatedCode =
            $$"""
            #nullable enable
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            {{visibility.ToCSharpString()}}{{(isSealed ? "sealed " : "")}}partial record Dependencies(
                global::Test.Diogen.Generators.AggregatedServices.IRequiredService Required,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("string-key")] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithStringKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(42)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithIntKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(global::Test.Diogen.Generators.AggregatedServices.KeyEnum.Key1)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithEnumKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(int))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithBuiltInTypeKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(global::System.DayOfWeek))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithTypeKey,
                global::Test.Diogen.Generators.AggregatedServices.IOptionalService? Optional = null
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies;
            
            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Dependencies.cs");
    }

    [Test]
    public async Task TopLevelInterfaceWithoutNullable_GeneratesTopLevelRecord(
        [Values(Default, Public, Internal, GeneratedTypeVisibility.File)] GeneratedTypeVisibility visibility,
        [Values(false, true)] bool isSealed)
    {
        var sourceCode =
            $$"""
            #nullable disable
            
            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface IRequiredService { }
            
            public interface IOptionalService { }
            
            public interface IKeyedService { }
            
            public enum KeyEnum
            {
                Key1
            }
            
            [AggregatedServices(
                Visibility = GeneratedTypeVisibility.{{visibility}},
                IsSealed = {{isSealed.ToString().ToLowerInvariant()}}
            )]
            public interface IDependencies
            {
                IRequiredService Required { get; }
                
                [Optional]
                IOptionalService Optional { get; }
                
                [Keyed("string-key")] IKeyedService KeyedWithStringKey { get; }
                
                [Keyed(42)] IKeyedService KeyedWithIntKey { get; }
                
                [Keyed(KeyEnum.Key1)] IKeyedService KeyedWithEnumKey { get; }
                
                [Keyed(typeof(int))] IKeyedService KeyedWithBuiltInTypeKey { get; }
                
                [Keyed(typeof(DayOfWeek))] IKeyedService KeyedWithTypeKey { get; }
            }
            """;

        var generatedCode =
            $$"""
            #nullable enable
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            {{visibility.ToCSharpString()}}{{(isSealed ? "sealed " : "")}}partial record Dependencies(
                global::Test.Diogen.Generators.AggregatedServices.IRequiredService Required,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("string-key")] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithStringKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(42)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithIntKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(global::Test.Diogen.Generators.AggregatedServices.KeyEnum.Key1)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithEnumKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(int))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithBuiltInTypeKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(global::System.DayOfWeek))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithTypeKey,
                global::Test.Diogen.Generators.AggregatedServices.IOptionalService? Optional = null
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies;
            
            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Dependencies.cs");
    }

    [Test]
    public async Task TopLevelInterfaceWithNullable_GeneratesTopLevelClass(
        [Values(Default, Public, Internal, GeneratedTypeVisibility.File)] GeneratedTypeVisibility visibility,
        [Values(false, true)] bool isSealed)
    {
        var sourceCode =
            $$"""
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface IRequiredService { }
            
            public interface IOptionalService { }
            
            public interface IKeyedService { }
            
            public enum KeyEnum
            {
                Key1
            }
            
            [AggregatedServices(
                Visibility = GeneratedTypeVisibility.{{visibility}},
                IsSealed = {{isSealed.ToString().ToLowerInvariant()}},
                Kind = GeneratedTypeKind.Class
            )]
            public interface IDependencies
            {
                IRequiredService Required { get; }
                
                IOptionalService? Optional { get; }
                
                [Keyed("string-key")] IKeyedService KeyedWithStringKey { get; }
                
                [Keyed(42)] IKeyedService KeyedWithIntKey { get; }
                
                [Keyed(KeyEnum.Key1)] IKeyedService KeyedWithEnumKey { get; }
                
                [Keyed(typeof(int))] IKeyedService KeyedWithBuiltInTypeKey { get; }
                
                [Keyed(typeof(DayOfWeek))] IKeyedService KeyedWithTypeKey { get; }
            }
            """;

        var generatedCode =
            $$"""
            #nullable enable
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            {{visibility.ToCSharpString()}}{{(isSealed ? "sealed " : "")}}partial class Dependencies(
                global::Test.Diogen.Generators.AggregatedServices.IRequiredService required,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute("string-key")] global::Test.Diogen.Generators.AggregatedServices.IKeyedService keyedWithStringKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(42)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService keyedWithIntKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(global::Test.Diogen.Generators.AggregatedServices.KeyEnum.Key1)] global::Test.Diogen.Generators.AggregatedServices.IKeyedService keyedWithEnumKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(int))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService keyedWithBuiltInTypeKey,
                [global::Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute(typeof(global::System.DayOfWeek))] global::Test.Diogen.Generators.AggregatedServices.IKeyedService keyedWithTypeKey,
                global::Test.Diogen.Generators.AggregatedServices.IOptionalService? optional = null
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies
            {
                public global::Test.Diogen.Generators.AggregatedServices.IRequiredService Required { get; } = required;
                public global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithStringKey { get; } = keyedWithStringKey;
                public global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithIntKey { get; } = keyedWithIntKey;
                public global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithEnumKey { get; } = keyedWithEnumKey;
                public global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithBuiltInTypeKey { get; } = keyedWithBuiltInTypeKey;
                public global::Test.Diogen.Generators.AggregatedServices.IKeyedService KeyedWithTypeKey { get; } = keyedWithTypeKey;
                public global::Test.Diogen.Generators.AggregatedServices.IOptionalService? Optional { get; } = optional;
            }
            
            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Dependencies.cs");
    }
}
