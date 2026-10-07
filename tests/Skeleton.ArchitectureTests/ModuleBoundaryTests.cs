using System.Reflection;
using NetArchTest.Rules;
using Skeleton.SharedKernel;

namespace Skeleton.ArchitectureTests;

public class ModuleBoundaryTests
{
    private const string ModulesPrefix = "Skeleton.Modules.";

    [Fact]
    public void SharedKernel_DoesNotDependOn_ModulesOrHost()
    {
        var result = Types.InAssembly(typeof(IModule).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Skeleton.Modules", "Skeleton.Host")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    [Fact]
    public void Modules_DoNotDependOn_OtherModulesInternals()
    {
        var modules = LoadModuleAssemblies();

        foreach (var module in modules)
        {
            foreach (var other in modules.Where(m => m != module))
            {
                // Every type in another module's implementation assembly is off-limits; only its *.Contracts assembly may be used.
                var forbidden = TypeNames(other);

                var result = Types.InAssembly(module)
                    .ShouldNot()
                    .HaveDependencyOnAny(forbidden)
                    .GetResult();

                result.IsSuccessful.ShouldBeTrue($"{module.GetName().Name} -> {other.GetName().Name}: {FailureMessage(result)}");
            }
        }
    }

    [Fact]
    public void Contracts_DoNotDependOn_AnyModuleImplementation()
    {
        var implementations = LoadModuleAssemblies().SelectMany(TypeNames).ToArray();

        foreach (var contracts in LoadAssemblies(name => name.EndsWith(".Contracts", StringComparison.Ordinal)))
        {
            var result = Types.InAssembly(contracts)
                .ShouldNot()
                .HaveDependencyOnAny(implementations)
                .GetResult();

            result.IsSuccessful.ShouldBeTrue($"{contracts.GetName().Name}: {FailureMessage(result)}");
        }
    }

    private static Assembly[] LoadModuleAssemblies() =>
        LoadAssemblies(name => !name.EndsWith(".Contracts", StringComparison.Ordinal));

    private static Assembly[] LoadAssemblies(Func<string, bool> filter)
    {
        var assemblies = Directory.GetFiles(AppContext.BaseDirectory, $"{ModulesPrefix}*.dll")
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => filter(name!))
            .Select(name => Assembly.Load(name!))
            .ToArray();

        assemblies.ShouldNotBeEmpty();
        return assemblies;
    }

    // Compiler-generated types without a namespace (e.g. <PrivateImplementationDetails>) exist in every assembly; skip them.
    private static string[] TypeNames(Assembly assembly) =>
        assembly.GetTypes().Where(t => t.Namespace is not null).Select(t => t.FullName!).ToArray();

    private static string FailureMessage(NetArchTest.Rules.TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? []);
}
