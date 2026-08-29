using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Entities;
using Order.Infrastructure.Persistence.Models;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.Infrastructure.Persistence;

internal sealed class OrderDbContext
    : DbContext, IUnitOfWork
{
    public OrderDbContext(
        DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order.Domain.Entities.Order> Orders =>
        Set<Order.Domain.Entities.Order>();

    public DbSet<OrderItem> OrderItems =>
        Set<OrderItem>();

    internal DbSet<OutboxMessage> OutboxMessages =>
    Set<OutboxMessage>();


    internal DbSet<InboxMessage> InboxMessages =>
    Set<InboxMessage>();

    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker
            .Entries<Order.Domain.Entities.Order>()
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        foreach (var domainEvent in domainEvents)
        {
            var outboxMessage =
                OutboxMessageFactory.Create(domainEvent);

            await OutboxMessages.AddAsync(
                outboxMessage,
                cancellationToken);
        }

        var result =
            await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in ChangeTracker
                     .Entries<Order.Domain.Entities.Order>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return result;
    }
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
