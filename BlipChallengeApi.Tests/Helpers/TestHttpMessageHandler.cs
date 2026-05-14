using System.Net;

namespace BlipChallengeApi.Tests.Helpers;

/// <summary>
/// HttpMessageHandler falso para tests. Permite inyectar una respuesta predefinida
/// en un HttpClient sin tocar la red real.
///
/// Es el patrón estándar recomendado por Microsoft para testear código que usa
/// HttpClient: en vez de mockear HttpClient (que es difícil porque sus métodos no
/// son virtuales), se mockea el handler subyacente.
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage _response;

    public TestHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(_response);
    }
}
