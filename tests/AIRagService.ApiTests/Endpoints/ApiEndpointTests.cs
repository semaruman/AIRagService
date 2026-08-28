using System.Net;
using System.Net.Http.Json;
using AIRagService.ApiTests.Fixtures;
using AIRagService.Application.Common;
using AIRagService.Application.DTOs;

namespace AIRagService.ApiTests.Endpoints;

[Collection("Api")]
public class ApiEndpointTests(ApiTestFixture fixture)
{
    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await fixture.Client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetDocuments_ReturnsEmptyPagedList()
    {
        var response = await fixture.Client.GetAsync("/api/v1/documents?page=1&pageSize=20");

        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<DocumentDto>>();

        Assert.NotNull(payload);
        Assert.Empty(payload.Items);
        Assert.Equal(0, payload.TotalCount);
        Assert.Equal(1, payload.Page);
        Assert.Equal(20, payload.PageSize);
    }

    [Fact]
    public async Task Query_WithoutQuestion_ReturnsBadRequest()
    {
        var response = await fixture.Client.PostAsJsonAsync(
            "/api/v1/query",
            new QueryRequestDto { Question = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Question is required", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Upload_InvalidFile_ReturnsBadRequest()
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent("not-a-valid-pdf"u8.ToArray());
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", "invalid.txt");

        var response = await fixture.Client.PostAsync("/api/v1/documents", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("valid PDF", body, StringComparison.OrdinalIgnoreCase);
    }
}
