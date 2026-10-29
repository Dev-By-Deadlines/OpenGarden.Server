using Microsoft.AspNetCore.SignalR.Client;

namespace OpenGarden.Server.Tests;

public class HubConnectionTests(OpenGardenWebApplicationFactory factory)
    : IClassFixture<OpenGardenWebApplicationFactory>
{
    [Fact]
    public async Task Client_CanConnectToTheHub()
    {
        await using var connection = factory.CreateHubConnection();

        await connection.StartAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HubConnectionState.Connected, connection.State);
    }
}
