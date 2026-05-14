using BlipChallengeApi.Controllers;
using BlipChallengeApi.Models;
using BlipChallengeApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace BlipChallengeApi.Tests.Controllers;

public class RepositoriesControllerTests
{
    [Fact]
    public async Task Get_ReturnsOk_WithRepositories_WhenServiceSucceeds()
    {
        // Arrange
        var service = Substitute.For<IGithubService>();
        var fakeRepos = new List<RepositoryResponse>
        {
            new()
            {
                FullName = "takenet/repo1",
                Description = "Primer repo",
                AvatarUrl = "https://avatars.example.com/1.png",
                CreatedAt = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new()
            {
                FullName = "takenet/repo2",
                Description = "Segundo repo",
                AvatarUrl = "https://avatars.example.com/2.png",
                CreatedAt = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        };
        service.GetRepositoriesAsync().Returns(fakeRepos);
        var controller = new RepositoriesController(service);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRepos = Assert.IsAssignableFrom<List<RepositoryResponse>>(okResult.Value);
        Assert.Equal(2, returnedRepos.Count);
        Assert.Equal("takenet/repo1", returnedRepos[0].FullName);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithEmptyList_WhenServiceReturnsNoRepos()
    {
        // Arrange
        var service = Substitute.For<IGithubService>();
        service.GetRepositoriesAsync().Returns(new List<RepositoryResponse>());
        var controller = new RepositoriesController(service);

        // Act
        var result = await controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRepos = Assert.IsAssignableFrom<List<RepositoryResponse>>(okResult.Value);
        Assert.Empty(returnedRepos);
    }

    [Fact]
    public async Task Get_Returns500_WithErrorMessage_WhenServiceThrowsHttpRequestException()
    {
        // Arrange
        var service = Substitute.For<IGithubService>();
        service.GetRepositoriesAsync().ThrowsAsync(new HttpRequestException("Github API Error: 500"));
        var controller = new RepositoriesController(service);

        // Act
        var result = await controller.Get();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.NotNull(objectResult.Value);
    }

    [Fact]
    public async Task Get_Returns500_WhenServiceThrowsAnyException()
    {
        // Arrange
        var service = Substitute.For<IGithubService>();
        service.GetRepositoriesAsync().ThrowsAsync(new InvalidOperationException("Algo inesperado"));
        var controller = new RepositoriesController(service);

        // Act
        var result = await controller.Get();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task Get_CallsServiceExactlyOnce()
    {
        // Arrange
        var service = Substitute.For<IGithubService>();
        service.GetRepositoriesAsync().Returns(new List<RepositoryResponse>());
        var controller = new RepositoriesController(service);

        // Act
        await controller.Get();

        // Assert
        await service.Received(1).GetRepositoriesAsync();
    }
}
