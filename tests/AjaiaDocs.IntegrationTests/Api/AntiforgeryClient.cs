using System.Net.Http.Json;
using System.Text.Json;

namespace AjaiaDocs.IntegrationTests.Api;

public static class AntiforgeryClient
{
    public static async Task<HttpResponseMessage> SendWithAntiforgeryAsync(this HttpClient client,
        HttpMethod method, string uri, object? body = null)
    {
        var tokenResponse = await client.GetFromJsonAsync<AntiforgeryToken>(
            "/api/session/antiforgery");
        var request = new HttpRequestMessage(method, uri);
        request.Headers.Add("X-XSRF-TOKEN", tokenResponse!.Token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await client.SendAsync(request);
    }

    public static async Task<T> PostJsonWithAntiforgeryAsync<T>(this HttpClient client,
        string uri, object body) => await SendJsonAsync<T>(client, HttpMethod.Post, uri, body);

    public static async Task<T> PutJsonWithAntiforgeryAsync<T>(this HttpClient client,
        string uri, object body) => await SendJsonAsync<T>(client, HttpMethod.Put, uri, body);

    public static Task<HttpResponseMessage> PutWithAntiforgeryAsync(this HttpClient client,
        string uri, object body) => client.SendWithAntiforgeryAsync(HttpMethod.Put, uri, body);

    public static async Task<HttpResponseMessage> PostFileWithAntiforgeryAsync(
        this HttpClient client, string fileName, byte[] bytes)
    {
        var tokenResponse = await client.GetFromJsonAsync<AntiforgeryToken>(
            "/api/session/antiforgery");
        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(bytes), "file", fileName);
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/documents/import")
        {
            Content = form
        };
        request.Headers.Add("X-XSRF-TOKEN", tokenResponse!.Token);
        return await client.SendAsync(request);
    }

    private static async Task<T> SendJsonAsync<T>(HttpClient client, HttpMethod method,
        string uri, object body)
    {
        var response = await client.SendWithAntiforgeryAsync(method, uri, body);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    private sealed record AntiforgeryToken(string Token);
}
