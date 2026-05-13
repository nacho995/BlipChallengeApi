using BlipChallengeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlipChallengeApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RepositoriesController : ControllerBase
{
    private readonly IGithubService _githubService;

    public RepositoriesController(
        IGithubService githubService
    )
    {
        _githubService = githubService;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var repositories =
                await _githubService.GetRepositoriesAsync();

            return Ok(repositories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = ex.Message
            });
        }
    }
}