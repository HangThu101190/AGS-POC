using AGS.SmartShift.Domain.Entities.Identity;
using NetArchTest.Rules;
using Xunit;

namespace AGS.SmartShift.ArchitectureTests;

public sealed class LayerDependencyTests
{
    private const string ApplicationNamespace = "AGS.SmartShift.Application";
    private const string InfrastructureNamespace = "AGS.SmartShift.Infrastructure";

    [Fact]
    public void Domain_should_not_reference_Application_or_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Site).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }

    [Fact]
    public void Application_should_not_reference_Infrastructure()
    {
        var result = Types.InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>()));
    }
}
