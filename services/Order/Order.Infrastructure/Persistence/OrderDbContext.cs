using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Entities;

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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
