using BlipChallengeApi.Models;

namespace BlipChallengeApi.Services.Interfaces;

public interface IGithubService
{
    Task<List<RepositoryResponse>> GetRepositoriesAsync();
}