using MediatR;
using Moongazing.PipelineEnhancer.Pipelines.Transaction;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Moongazing.PipelineEnhancer.Tests;

public class TransactionScopeBehaviorTests
{
    [Fact]
    public async Task Should_Commit_Transaction_When_No_Exception()
    {
        // Arrange
        var behavior = new TransactionScopeBehavior<SampleTransactionRequest, SampleTransactionResponse>();
        var request = new SampleTransactionRequest();

        // Act
        var response = await behavior.Handle(request, () => Task.FromResult(new SampleTransactionResponse { Success = true }), CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.Success); // Transaction başarıyla tamamlandı
    }

    [Fact]
    public async Task Should_Rollback_Transaction_When_Exception_Occurs()
    {
        // Arrange
        var behavior = new TransactionScopeBehavior<SampleTransactionRequest, SampleTransactionResponse>();
        var request = new SampleTransactionRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            behavior.Handle(request, () => throw new InvalidOperationException("Test Exception"), CancellationToken.None));
    }

    private class SampleTransactionRequest : IRequest<SampleTransactionResponse>, ITransactionalRequest
    {
    }

    private class SampleTransactionResponse
    {
        public bool Success { get; set; }
    }
}
