using Microsoft.AspNetCore.Mvc.Testing;

namespace ChronoKraft.Tests.EndToEnd;

[Trait("Category", "EndToEnd")]
public class SmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SmokeTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Application_Starts_And_Responds()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");
        Assert.True(response.IsSuccessStatusCode || (int)response.StatusCode == 302);
    }
}