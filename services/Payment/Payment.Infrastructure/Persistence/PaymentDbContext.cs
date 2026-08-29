using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Persistence;
using Payment.Infrastructure.Persistence.Inbox;

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
    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentDbContext).Assembly);
    }
}
