using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Persistence;
using Payment.Infrastructure.Persistence.Inbox;
using Payment.Infrastructure.Persistence.Outbox;

namespace Payment.Infrastructure.Persistence;

public sealed class PaymentDbContext
 : DbContext, IUnitOfWork
{
    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Domain.Payments.Payment> Payments =>
        Set<Domain.Payments.Payment>();


    public DbSet<InboxMessage> InboxMessages =>
    Set<InboxMessage>();

    public DbSet<OutboxMessage> OutboxMessages =>
    Set<OutboxMessage>();



    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var payments =
            ChangeTracker
                .Entries<Domain.Payments.Payment>()
                .Where(x => x.Entity.DomainEvents.Count != 0)
                .Select(x => x.Entity)
                .ToList();

        var domainEvents =
            payments
                .SelectMany(x => x.DomainEvents)
                .ToList();

        foreach (var domainEvent in domainEvents)
        {
            OutboxMessages.Add(
                OutboxMessageFactory.Create(domainEvent));
        }

        var result =
            await base.SaveChangesAsync(
                cancellationToken);

        foreach (var payment in payments)
        {
            payment.ClearDomainEvents();
        }

        return result;
    }



    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentDbContext).Assembly);
    }
}
