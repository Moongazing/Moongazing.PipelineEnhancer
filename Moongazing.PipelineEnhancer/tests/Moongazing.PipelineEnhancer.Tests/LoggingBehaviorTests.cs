using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moongazing.PipelineEnhancer.Pipelines.Logging;
using Moq;

namespace Moongazing.PipelineEnhancer.Tests;

public class LoggingBehaviorTests
{
    private readonly Mock<IHttpContextAccessor> httpContextAccessorMock;
    private readonly Mock<ILogger<LoggingBehavior<SampleLoggingRequest, SampleLoggingResponse>>> loggerMock;

    public LoggingBehaviorTests()
    {
        httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        loggerMock = new Mock<ILogger<LoggingBehavior<SampleLoggingRequest, SampleLoggingResponse>>>();
    }

    [Fact]
    public async Task Should_Log_Request_Type()
    {
        // Arrange
        var behavior = new LoggingBehavior<SampleLoggingRequest, SampleLoggingResponse>(httpContextAccessorMock.Object, loggerMock.Object);

        var request = new SampleLoggingRequest { Message = "Test Message" };
        var response = new SampleLoggingResponse { Result = "Test Result" };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(response), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Result", result.Result);

        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains(nameof(SampleLoggingRequest))),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Call_Next_Handler()
    {
        // Arrange
        var behavior = new LoggingBehavior<SampleLoggingRequest, SampleLoggingResponse>(httpContextAccessorMock.Object, loggerMock.Object);

        var request = new SampleLoggingRequest { Message = "Test Message" };
        var response = new SampleLoggingResponse { Result = "Test Result" };

        // Act
        var result = await behavior.Handle(request, () => Task.FromResult(response), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Result", result.Result);
    }

    public class SampleLoggingRequest : IRequest<SampleLoggingResponse>, ILoggableRequest
    {
        public string Message { get; set; } = string.Empty;
    }
    public class SampleLoggingResponse
    {
        public string Result { get; set; } = string.Empty;
    }
}
