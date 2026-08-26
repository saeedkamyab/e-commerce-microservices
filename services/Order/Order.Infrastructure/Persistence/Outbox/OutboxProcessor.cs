using Order.Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
namespace Order.Infrastructure.Persistence.Outbox;

internal sealed class OutboxProcessor
{
    private const int BatchSize = 20;

    private readonly OrderDbContext _dbContext;
    private readonly IIntegrationEventPublisher _publisher;

    public OutboxProcessor(
        OrderDbContext dbContext,
        IIntegrationEventPublisher publisher)
    {
        _dbContext = dbContext;
        _publisher = publisher;
    }

    public async Task ProcessAsync(
        CancellationToken cancellationToken)
    {
        var messages = await _dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(
                    message.Id,
                    message.Type,
                    message.Content,
                    cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}