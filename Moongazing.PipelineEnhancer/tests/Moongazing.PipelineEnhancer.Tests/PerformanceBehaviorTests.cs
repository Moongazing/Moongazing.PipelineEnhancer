using MediatR;
using Microsoft.Extensions.Logging;
using Moongazing.PipelineEnhancer.Pipelines.Performance;
using Moq;
using System.Diagnostics;

namespace Moongazing.PipelineEnhancer.Tests;

public class PerformanceBehaviorTests
{
    private readonly Stopwatch stopwatch;
    private readonly Mock<ILogger<PerformanceBehavior<SampleIntervalRequest, SampleIntervalResponse>>> loggerMock;

    public PerformanceBehaviorTests()
    {
        loggerMock = new Mock<ILogger<PerformanceBehavior<SampleIntervalRequest, SampleIntervalResponse>>>();
        stopwatch = new Stopwatch();
    }
    [Fact]
    public async Task Should_Not_Log_When_Execution_Time_Is_Within_Interval()
    {

        // Arrange
        var request = new SampleIntervalRequest { }; // Interval 5 saniye
        var behavior = new PerformanceBehavior<SampleIntervalRequest, SampleIntervalResponse>(stopwatch, loggerMock.Object);

        // Act
        var response = await behavior.Handle(request, async () =>
        {
            await Task.Delay(1000); // 1 saniyelik gecikme
            return new SampleIntervalResponse();
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        loggerMock.Verify(
            logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Never); // Loglama yapılmamalı
    }

    [Fact]
    public async Task Should_Log_When_Execution_Time_Exceeds_Interval()
    {
        // Arrange
        var request = new SampleIntervalRequest { }; // Interval 1 saniye
        var behavior = new PerformanceBehavior<SampleIntervalRequest, SampleIntervalResponse>(stopwatch, loggerMock.Object);

        // Act
        var response = await behavior.Handle(request, async () =>
        {
            await Task.Delay(2000); // 2 saniyelik gecikme
            return new SampleIntervalResponse();
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Performance")),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once); // Loglama yapılmalı
    }

    [Fact]
    public async Task Should_Handle_Request_Successfully()
    {
        // Arrange
        var request = new SampleIntervalRequest { };
        var behavior = new PerformanceBehavior<SampleIntervalRequest, SampleIntervalResponse>(stopwatch, loggerMock.Object);

        // Act
        var response = await behavior.Handle(request, async () =>
        {
            return new SampleIntervalResponse { Message = "Success" };
        }, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Success", response.Message);
    }

    public class SampleIntervalRequest : IRequest<SampleIntervalResponse>, IIntervalRequest
    {
        public int Interval => 15;
    }

    public class SampleIntervalResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
