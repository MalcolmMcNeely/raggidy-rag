using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaggidyRag.Api.Tests.Embedding;

// The vector is the text's length, so a test can tell the vectors apart.
public sealed class AnsweringVoyage : HttpMessageHandler
{
    readonly List<string> bodies = [];

    public IReadOnlyList<string> Bodies => bodies;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        bodies.Add(body);
        var texts = JsonDocument.Parse(body).RootElement.GetProperty("input").EnumerateArray().Select(text => text.GetString()!);
        var data = texts.Select(text => new { embedding = new[] { (float)text.Length } });
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { data }) };
    }
}
