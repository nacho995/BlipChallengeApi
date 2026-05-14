using System.Net;
using BlipChallengeApi.Services;
using BlipChallengeApi.Tests.Helpers;
using Xunit;

namespace BlipChallengeApi.Tests.Services;

public class GithubServiceTests
{
    private const string ValidJsonWithMixedLanguages = """
    [
        {
            "full_name": "takenet/old-csharp",
            "description": "Repo antiguo en C#",
            "language": "C#",
            "created_at": "2014-01-15T10:00:00Z",
            "owner": { "avatar_url": "https://avatars.example.com/takenet.png" }
        },
        {
            "full_name": "takenet/javascript-lib",
            "description": "Repo en JS, debe filtrarse",
            "language": "JavaScript",
            "created_at": "2013-05-20T10:00:00Z",
            "owner": { "avatar_url": "https://avatars.example.com/takenet.png" }
        },
        {
            "full_name": "takenet/older-csharp",
            "description": "Repo más antiguo en C#",
            "language": "C#",
            "created_at": "2013-11-29T09:16:51Z",
            "owner": { "avatar_url": "https://avatars.example.com/takenet.png" }
        },
        {
            "full_name": "takenet/recent-csharp",
            "description": "Repo reciente en C#",
            "language": "C#",
            "created_at": "2023-06-01T12:00:00Z",
            "owner": { "avatar_url": "https://avatars.example.com/takenet.png" }
        }
    ]
    """;

    [Fact]
    public async Task GetRepositoriesAsync_FiltersOnlyCSharpRepos_AndOrdersByCreatedAtAscending()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, ValidJsonWithMixedLanguages);
        var httpClient = new HttpClient(handler);
        var service = new GithubService(httpClient);

        // Act
        var result = await service.GetRepositoriesAsync();

        // Assert
        Assert.Equal(3, result.Count); // 3 repos C# en el mock, el JS se filtra
        Assert.Equal("takenet/older-csharp", result[0].FullName);   // el más antiguo primero
        Assert.Equal("takenet/old-csharp", result[1].FullName);
        Assert.Equal("takenet/recent-csharp", result[2].FullName);  // el más reciente último
    }

    [Fact]
    public async Task GetRepositoriesAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var json = """
        [{
            "full_name": "takenet/test-repo",
            "description": "Descripción de prueba",
            "language": "C#",
            "created_at": "2020-01-15T08:30:00Z",
            "owner": { "avatar_url": "https://avatars.example.com/avatar.png" }
        }]
        """;
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, json);
        var service = new GithubService(new HttpClient(handler));

        // Act
        var result = await service.GetRepositoriesAsync();

        // Assert
        var repo = Assert.Single(result);
        Assert.Equal("takenet/test-repo", repo.FullName);
        Assert.Equal("Descripción de prueba", repo.Description);
        Assert.Equal("https://avatars.example.com/avatar.png", repo.AvatarUrl);
        Assert.Equal(new DateTime(2020, 1, 15, 8, 30, 0, DateTimeKind.Utc), repo.CreatedAt);
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsAtMost5Repos_EvenIfMoreCSharpAvailable()
    {
        // Arrange: 7 repos C#
        var repos = string.Join(",", Enumerable.Range(1, 7).Select(i => $$"""
        {
            "full_name": "takenet/repo{{i}}",
            "description": "desc",
            "language": "C#",
            "created_at": "2020-01-{{i:00}}T00:00:00Z",
            "owner": { "avatar_url": "url" }
        }
        """));
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, $"[{repos}]");
        var service = new GithubService(new HttpClient(handler));

        // Act
        var result = await service.GetRepositoriesAsync();

        // Assert
        Assert.Equal(5, result.Count); // máximo 5, no 7
    }

    [Fact]
    public async Task GetRepositoriesAsync_ThrowsHttpRequestException_WhenApiReturnsServerError()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(HttpStatusCode.InternalServerError, "");
        var service = new GithubService(new HttpClient(handler));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetRepositoriesAsync());
    }

    [Fact]
    public async Task GetRepositoriesAsync_ThrowsHttpRequestException_WhenApiReturnsNotFound()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(HttpStatusCode.NotFound, "");
        var service = new GithubService(new HttpClient(handler));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetRepositoriesAsync());
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsEmptyList_WhenJsonIsNull()
    {
        // Arrange: GitHub devuelve literalmente "null" como JSON
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, "null");
        var service = new GithubService(new HttpClient(handler));

        // Act
        var result = await service.GetRepositoriesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRepositoriesAsync_ReturnsEmptyList_WhenNoCSharpReposInResponse()
    {
        // Arrange: solo repos JavaScript
        var json = """
        [
            { "full_name": "x/y", "language": "JavaScript", "created_at": "2020-01-01T00:00:00Z", "owner": { "avatar_url": "url" } },
            { "full_name": "x/z", "language": "Python", "created_at": "2020-01-02T00:00:00Z", "owner": { "avatar_url": "url" } }
        ]
        """;
        var handler = new TestHttpMessageHandler(HttpStatusCode.OK, json);
        var service = new GithubService(new HttpClient(handler));

        // Act
        var result = await service.GetRepositoriesAsync();

        // Assert
        Assert.Empty(result);
    }
}
