using Microsoft.EntityFrameworkCore;
using Payment.Infrastructure.Messaging;

namespace Payment.Infrastructure.Persistence.Inbox;

internal sealed class InboxProcessor
{
    private readonly PaymentDbContext _dbContext;
    private readonly IntegrationEventDispatcher _dispatcher;

    public InboxProcessor(
        PaymentDbContext dbContext,
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
                .SingleOrDefaultAsync(
                    x => x.MessageId == messageId,
                    cancellationToken);

        if (existingMessage is
            { ProcessedOnUtc: not null })
        {
            return false;
        }

        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
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
                _dbContext.InboxMessages.Add(
                    inboxMessage);
            }

            await _dispatcher.DispatchAsync(
                messageId,
                type,
                content,
                cancellationToken);

            inboxMessage.ProcessedOnUtc =
                DateTime.UtcNow;

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
