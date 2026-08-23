using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.Models;
using Catalog.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence;

public class CatalogDbContext : DbContext, IUnitOfWork
{
    public CatalogDbContext(
        DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    internal DbSet<ProductSpecificationRecord> ProductSpecifications =>
    Set<ProductSpecificationRecord>();

    public DbSet<Category> Categories => Set<Category>();

    internal DbSet<CategoryAttributeDefinitionRecord>
    CategoryAttributeDefinitions =>
        Set<CategoryAttributeDefinitionRecord>();

    internal DbSet<AttributeOptionRecord>
        AttributeOptions =>
            Set<AttributeOptionRecord>();


    internal DbSet<OutboxMessage> OutboxMessages =>
    Set<OutboxMessage>();


    internal DbSet<InboxMessage> InboxMessages =>
    Set<InboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
    {
        var productsWithDomainEvents =
            ChangeTracker
                .Entries<Product>()
                .Select(x => x.Entity)
                .Where(x => x.DomainEvents.Count > 0)
                .ToList();

        var domainEvents =
            productsWithDomainEvents
                .SelectMany(x => x.DomainEvents)
                .ToList();

        var outboxMessages = domainEvents
      .Select(OutboxMessageFactory.Create)
      .Where(x => x is not null)
      .Select(x => x!)
      .ToList();

        if (outboxMessages.Count > 0)
        {
            await OutboxMessages.AddRangeAsync(
                outboxMessages,
                cancellationToken);
        }

        var result = await base.SaveChangesAsync(
            cancellationToken);

        foreach (var product in productsWithDomainEvents)
        {
            product.ClearDomainEvents();
        }

        return result;
    }
}
