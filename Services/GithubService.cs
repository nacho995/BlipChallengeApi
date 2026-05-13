using BlipChallengeApi.Models;
using BlipChallengeApi.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace BlipChallengeApi.Services;

public class GithubService : IGithubService
{
    private const string GithubReposUrl = "https://api.github.com/users/takenet/repos?sort=created&direction=asc&per_page=100";
    private readonly HttpClient _httpClient;


    public GithubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<RepositoryResponse>> GetRepositoriesAsync()
    {
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("BlipChallengeApi", "1.0"));
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        using var response = await _httpClient.GetAsync(GithubReposUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Github API Error: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var json = await response.Content.ReadAsStringAsync();

        var repositories =
            JsonSerializer.Deserialize<List<GithubRepository>>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            });

        if (repositories == null)
        {
            return new List<RepositoryResponse>();
        }


        return repositories
        .Where(r => r.Language == "C#")
        .OrderBy(r => r.Created_At)
        .Take(5)
        .Select(r => new RepositoryResponse
        {
            FullName = r.Full_Name,
            Description = r.Description,
            AvatarUrl = r.Owner!.Avatar_Url,
            CreatedAt = r.Created_At
        })
            .ToList();
    }
}
