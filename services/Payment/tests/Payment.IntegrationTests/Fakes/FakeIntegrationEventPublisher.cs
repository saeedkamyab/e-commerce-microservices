using Payment.Application.Abstractions.Messaging;

namespace Payment.IntegrationTests.Fakes;

internal sealed class FakeIntegrationEventPublisher
    : IIntegrationEventPublisher
{
    private readonly bool _shouldFail;

    public FakeIntegrationEventPublisher(
        bool shouldFail = false)
    {
        _shouldFail = shouldFail;
    }

    public int PublishCount { get; private set; }

    public Task PublishAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        PublishCount++;

        if (_shouldFail)
        {
            throw new InvalidOperationException(
                "Simulated publish failure.");
        }

        return Task.CompletedTask;
    }
}
