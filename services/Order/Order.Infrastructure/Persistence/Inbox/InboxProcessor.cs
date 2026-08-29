using Microsoft.EntityFrameworkCore;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence.Models;

namespace Order.Infrastructure.Persistence.Inbox;

internal sealed class InboxProcessor
{
    private readonly OrderDbContext _dbContext;
    private readonly IntegrationEventDispatcher _dispatcher;

    public InboxProcessor(
        OrderDbContext dbContext,
        IntegrationEventDispatcher dispatcher)
    {
        _dbContext = dbContext;
        _dispatcher = dispatcher;
    }

    public async Task<bool> ProcessAsync(
        Guid messageId,
        string type,
        string content,
        CancellationToken cancellationToken)
    {
        var existingMessage =
            await _dbContext.InboxMessages
                .FirstOrDefaultAsync(
                    x => x.MessageId == messageId,
                    cancellationToken);

        if (existingMessage?.ProcessedOnUtc is not null)
            return false;

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var inboxMessage =
                existingMessage ??
                new InboxMessage
                {
                    MessageId = messageId,
                    Type = type,
                    ReceivedOnUtc = DateTime.UtcNow
                };

            if (existingMessage is null)
            {
                await _dbContext.InboxMessages.AddAsync(
                    inboxMessage,
                    cancellationToken);
            }

            await _dispatcher.DispatchAsync(
                messageId,
                type,
                content,
                cancellationToken);

            inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            inboxMessage.Error = null;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}
