using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moongazing.PipelineEnhancer.Pipelines.Caching;
using Moq;
using Xunit;

namespace Moongazing.PipelineEnhancer.Tests;

public class CachingBehaviorTests
{
    private readonly Mock<IDistributedCache> cacheMock;
    private readonly Mock<ILogger<CachingBehavior<SampleCachableRequest, SampleCachableResponse>>> loggerMock;

    public CachingBehaviorTests()
    {
        cacheMock = new Mock<IDistributedCache>();
        loggerMock = new Mock<ILogger<CachingBehavior<SampleCachableRequest, SampleCachableResponse>>>();
    }

    [Fact]
    public async Task Should_Return_Response_From_Cache_When_Exists()
    {
        // Arrange
        var cachedResponse = new SampleCachableResponse { Message = "Cached Response" };
        var serializedResponse = JsonSerializer.SerializeToUtf8Bytes(cachedResponse);

        cacheMock.Setup(x => x.GetAsync("SampleKey", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(serializedResponse);

        var request = new SampleCachableRequest { CacheKey = "SampleKey", BypassCache = false };
        var behavior = new CachingBehavior<SampleCachableRequest, SampleCachableResponse>(
            cacheMock.Object,
            loggerMock.Object,
            Mock.Of<IConfiguration>());

        // Act
        var response = await behavior.Handle(request, () => Task.FromResult(new SampleCachableResponse()), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Cached Response", response.Message);

        loggerMock.Verify(logger =>
            logger.Log(LogLevel.Information,
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Fetched from Cache")),
                       null,
                       It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Process_And_Add_To_Cache_When_Not_Exists()
    {
        // Arrange
        cacheMock.Setup(x => x.GetAsync("SampleKey", It.IsAny<CancellationToken>()))
                  .ReturnsAsync((byte[]?)null);

        var request = new SampleCachableRequest { CacheKey = "SampleKey", BypassCache = false };
        var behavior = new CachingBehavior<SampleCachableRequest, SampleCachableResponse>(
            cacheMock.Object,
            loggerMock.Object,
            Mock.Of<IConfiguration>());

        var expectedResponse = new SampleCachableResponse { Message = "Processed Response" };

        // Act
        var response = await behavior.Handle(request, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Processed Response", response.Message);

        cacheMock.Verify(x => x.SetAsync(
            "SampleKey",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);

        loggerMock.Verify(logger =>
            logger.Log(LogLevel.Information,
                       It.IsAny<EventId>(),
                       It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Added to Cache")),
                       null,
                       It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Bypass_Cache_When_BypassCache_Is_True()
    {
        // Arrange
        var request = new SampleCachableRequest { CacheKey = "SampleKey", BypassCache = true };
        var behavior = new CachingBehavior<SampleCachableRequest, SampleCachableResponse>(
            cacheMock.Object,
            loggerMock.Object,
            Mock.Of<IConfiguration>());

        var expectedResponse = new SampleCachableResponse { Message = "Bypassed Response" };

        // Act
        var response = await behavior.Handle(request, () => Task.FromResult(expectedResponse), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Bypassed Response", response.Message);

        cacheMock.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        cacheMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    public class SampleCachableRequest : IRequest<SampleCachableResponse>, ICachableRequest
    {
        public string CacheKey { get; set; } = string.Empty;
        public bool BypassCache { get; set; }
        public string? CacheGroupKey { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }
    }

    public class SampleCachableResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
