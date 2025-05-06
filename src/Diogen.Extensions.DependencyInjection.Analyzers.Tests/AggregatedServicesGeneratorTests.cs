using System.Diagnostics.CodeAnalysis;
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

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies.cs");
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

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies.cs");
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

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies.cs");
    }

    [Test]
    public async Task TopLevelGenericInterfaceWithNullable_GeneratesTopLevelRecord()
    {
        var sourceCode =
            """
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface INonGenericService { }
            
            public interface ISimpleGenericService<out T> { }
            
            public interface IConstrainedGenericService<out T> where T : class, IDisposable, new() { }
            
            public interface IConstrainedGenericService2<T> where T : struct { }
            
            [AggregatedServices]
            public interface IDependencies<T1, T2, T3> where T2 : class, IDisposable, new() where T3 : struct
            {
                INonGenericService NonGeneric { get; }
                
                ISimpleGenericService<T1> SimpleGeneric { get; }
                
                IConstrainedGenericService<T2> ConstrainedGeneric { get; }
                
                IConstrainedGenericService2<T3> ConstrainedGeneric2 { get; }
            }
            """;

        var generatedCode =
            """
            #nullable enable

            namespace Test.Diogen.Generators.AggregatedServices;

            public partial record Dependencies<T1, T2, T3>(
                global::Test.Diogen.Generators.AggregatedServices.INonGenericService NonGeneric,
                global::Test.Diogen.Generators.AggregatedServices.ISimpleGenericService<T1> SimpleGeneric,
                global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService<T2> ConstrainedGeneric,
                global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService2<T3> ConstrainedGeneric2
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies<T1, T2, T3>
                where T2 : class, global::System.IDisposable, new()
                where T3 : struct;

            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies`3.cs");
    }

    [Test]
    public async Task TopLevelGenericInterfaceWithNullable_GeneratesTopLevelClass()
    {
        var sourceCode =
            """
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface INonGenericService { }
            
            public interface ISimpleGenericService<out T> { }
            
            public interface IConstrainedGenericService<out T> where T : class, IDisposable, new() { }
            
            public interface IConstrainedGenericService2<T> where T : struct { }
            
            [AggregatedServices(Kind = GeneratedTypeKind.Class)]
            public interface IDependencies<T1, T2, T3> where T2 : class, IDisposable, new() where T3 : struct
            {
                INonGenericService NonGeneric { get; }
                
                ISimpleGenericService<T1> SimpleGeneric { get; }
                
                IConstrainedGenericService<T2> ConstrainedGeneric { get; }
                
                IConstrainedGenericService2<T3> ConstrainedGeneric2 { get; }
            }
            """;

        var generatedCode =
            """
            #nullable enable

            namespace Test.Diogen.Generators.AggregatedServices;

            public partial class Dependencies<T1, T2, T3>(
                global::Test.Diogen.Generators.AggregatedServices.INonGenericService nonGeneric,
                global::Test.Diogen.Generators.AggregatedServices.ISimpleGenericService<T1> simpleGeneric,
                global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService<T2> constrainedGeneric,
                global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService2<T3> constrainedGeneric2
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies<T1, T2, T3>
                where T2 : class, global::System.IDisposable, new()
                where T3 : struct
            {
                public global::Test.Diogen.Generators.AggregatedServices.INonGenericService NonGeneric { get; } = nonGeneric;
                public global::Test.Diogen.Generators.AggregatedServices.ISimpleGenericService<T1> SimpleGeneric { get; } = simpleGeneric;
                public global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService<T2> ConstrainedGeneric { get; } = constrainedGeneric;
                public global::Test.Diogen.Generators.AggregatedServices.IConstrainedGenericService2<T3> ConstrainedGeneric2 { get; } = constrainedGeneric2;
            }

            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies`3.cs");
    }

    [Test]
    public async Task DefaultInterfaceProperties_AreIgnored()
    {
        var sourceCode =
            """
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface IService1 { }
            public interface IService2 { }
            public interface IService3 { }
            
            [AggregatedServices]
            public interface IDependencies
            {
                IService1 Service1 { get; }
                public IService2 Service2 { get; }
                IService3 Service3 => throw new NotImplementedException();
            }
            """;

        var generatedCode =
            """
            #nullable enable

            namespace Test.Diogen.Generators.AggregatedServices;

            public partial record Dependencies(
                global::Test.Diogen.Generators.AggregatedServices.IService1 Service1,
                global::Test.Diogen.Generators.AggregatedServices.IService2 Service2
            ) : global::Test.Diogen.Generators.AggregatedServices.IDependencies;

            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Dependencies.cs");
    }

    [Test]
    public async Task Nested_GeneratesNestedRecord()
    {
        var sourceCode =
            """
            #nullable enable

            using System;
            using Diogen.Generators;            
            using Diogen.Extensions.DependencyInjection.Generators;
            
            namespace Test.Diogen.Generators.AggregatedServices;
            
            public interface IService { }
            
            public partial class Class
            {
                public partial record Record
                {
                    public partial struct Struct
                    {
                        public partial record struct RecordStruct<T>
                        {
                            public partial interface Interface
                            {
                                [AggregatedServices]
                                public interface IDependencies
                                {
                                    IService Service { get; }
                                }
                            }
                        }
                    }
                }
            }
            """;

        var generatedCode =
            """
            #nullable enable

            namespace Test.Diogen.Generators.AggregatedServices;

            partial class Class
            {
                partial record class Record
                {
                    partial struct Struct
                    {
                        partial record struct RecordStruct<T>
                        {
                            partial interface Interface
                            {
                                public partial record Dependencies(
                                    global::Test.Diogen.Generators.AggregatedServices.IService Service
                                ) : global::Test.Diogen.Generators.AggregatedServices.Class.Record.Struct.RecordStruct<T>.Interface.IDependencies;
                            }
                        }
                    }
                }
            }
            
            """;

        await VerifyCS.VerifySourcesGeneratorAsync(sourceCode, generatedCode, "Test.Diogen.Generators.AggregatedServices.Class.Record.Struct.RecordStruct`1.Interface.Dependencies.cs");
    }
}
