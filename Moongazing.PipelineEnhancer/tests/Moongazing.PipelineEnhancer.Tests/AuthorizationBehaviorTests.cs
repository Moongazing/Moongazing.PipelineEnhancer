using Moq;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Doing.Retail.Core.Application.Pipelines.Authorization;
using Moongazing.PipelineEnhancer.Exceptions;
using MediatR;

namespace Moongazing.PipelineEnhancer.Tests;

public class AuthorizationBehaviorTests
{
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;

    public AuthorizationBehaviorTests()
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    }

    [Fact]
    public async Task Should_Allow_Request_When_Roles_Are_Valid()
    {
        // Arrange
        var roles = new[] { "Admin", "User" };
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var behavior = new AuthorizationBehavior<SampleRequest, SampleResponse>(httpContextAccessorMock.Object);

        var request = new SampleRequest { Roles = ["Admin"] };

        // Act
        var response = await behavior.Handle(request, () => Task.FromResult(new SampleResponse()), default);

        // Assert
        Assert.NotNull(response);
    }

    [Fact]
    public async Task Should_Throw_AuthorizationException_When_Roles_Are_Invalid()
    {
        // Arrange
        var roles = new[] { "User" };
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns(httpContext);

        var behavior = new AuthorizationBehavior<SampleRequest, SampleResponse>(httpContextAccessorMock.Object);

        var request = new SampleRequest { Roles = ["Admin"] };

        // Act & Assert
        await Assert.ThrowsAsync<AuthorizationException>(() =>
            behavior.Handle(request, () => Task.FromResult(new SampleResponse()), default));
    }

    private class SampleRequest : IRequest<SampleResponse>, ISecuredRequest
    {
        public string[] Roles { get; set; } = [];
    }

    private class SampleResponse
    {
    }
}
