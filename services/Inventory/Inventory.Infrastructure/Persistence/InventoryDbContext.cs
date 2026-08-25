using Inventory.Application.Abstractions.Persistence;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence;

internal sealed class InventoryDbContext
    : DbContext, IUnitOfWork
{
    public InventoryDbContext(
        DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems =>
        Set<InventoryItem>();

    internal DbSet<InboxMessage> InboxMessages =>
    Set<InboxMessage>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InventoryDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}