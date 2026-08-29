using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Persistence;

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

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(PaymentDbContext).Assembly);
    }
}
