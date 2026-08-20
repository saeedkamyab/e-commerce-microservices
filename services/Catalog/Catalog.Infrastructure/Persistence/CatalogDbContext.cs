using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Infrastructure.Persistence.Models;
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


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CatalogDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
