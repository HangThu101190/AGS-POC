using Xunit;

namespace AGS.SmartShift.Api.IntegrationTests;

[CollectionDefinition(ApiIntegrationCollection.Name)]
public sealed class ApiIntegrationCollection : ICollectionFixture<SmartShiftApiFactory>
{
    public const string Name = "ApiIntegration";
}
