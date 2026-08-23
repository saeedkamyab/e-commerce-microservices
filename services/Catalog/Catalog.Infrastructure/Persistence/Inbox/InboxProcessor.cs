using Catalog.Application.Abstractions.Messaging;
using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Inbox;

internal sealed class InboxProcessor
{
    private readonly CatalogDbContext _dbContext;
    private readonly IIntegrationEventHandler _handler;

    public InboxProcessor(
        CatalogDbContext dbContext,
        IIntegrationEventHandler handler)
    {
        _dbContext = dbContext;
        _handler = handler;
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

        if (existingMessage is not null &&
            existingMessage.ProcessedOnUtc is not null)
        {
            return false;
        }

        InboxMessage inboxMessage;

        if (existingMessage is null)
        {
            inboxMessage = new InboxMessage
            {
                MessageId = messageId,
                Type = type,
                ReceivedOnUtc = DateTime.UtcNow
            };

            await _dbContext.InboxMessages.AddAsync(
                inboxMessage,
                cancellationToken);
        }
        else
        {
            inboxMessage = existingMessage;
        }

        try
        {
            await _handler.HandleAsync(
                messageId,
                type,
                content,
                cancellationToken);

            inboxMessage.ProcessedOnUtc = DateTime.UtcNow;
            inboxMessage.Error = null;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            inboxMessage.Error = ex.Message;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw;
        }
    }
}
