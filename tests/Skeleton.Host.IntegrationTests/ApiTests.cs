using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Skeleton.Modules.Sample.Contracts;

namespace Skeleton.Host.IntegrationTests;

public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("/health/live")]
    [InlineData("/health/ready")]
    public async Task HealthEndpoints_ReturnOk(string path)
    {
        var response = await _client.GetAsync(new Uri(path, UriKind.Relative), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task OpenApiDocument_IsServed_InDevelopment()
    {
        var response = await _client.GetAsync(new Uri("/openapi/v1.json", UriKind.Relative), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateNote_ThenGet_RoundTrips()
    {
        var create = await _client.PostAsJsonAsync("/sample/notes", new { title = "hello" }, Ct);
        create.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<NoteDto>(Ct);

        var fetched = await _client.GetFromJsonAsync<NoteDto>(create.Headers.Location, Ct);

        fetched.ShouldBe(created);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"title":""}""")]
    public async Task CreateNote_WithInvalidBody_ReturnsValidationProblem(string body)
    {
        using var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        var response = await _client.PostAsync(new Uri("/sample/notes", UriKind.Relative), content, Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ReadProblemAsync(response);
        problem.GetProperty("errors").TryGetProperty("Title", out _).ShouldBeTrue();
    }

    [Fact]
    public async Task GetNote_Missing_ReturnsNotFoundProblem()
    {
        var response = await _client.GetAsync(new Uri($"/sample/notes/{Guid.NewGuid()}", UriKind.Relative), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var problem = await ReadProblemAsync(response);
        problem.GetProperty("title").GetString().ShouldBe("Sample.NoteNotFound");
    }

    [Fact]
    public async Task UnknownRoute_ReturnsProblemDetails()
    {
        var response = await _client.GetAsync(new Uri("/does-not-exist", UriKind.Relative), Ct);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
    }

    private static async Task<JsonElement> ReadProblemAsync(HttpResponseMessage response)
    {
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        return await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
    }
}
