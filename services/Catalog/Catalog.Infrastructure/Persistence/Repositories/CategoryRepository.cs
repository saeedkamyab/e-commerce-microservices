using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class CategoryRepository : ICategoryRepository
{
    private readonly CatalogDbContext _dbContext;

    public CategoryRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await _dbContext.Categories.AsNoTracking().
            FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (category is null)
            return null;

        var definitions = await _dbContext.CategoryAttributeDefinitions
            .AsNoTracking()
            .Where(x => x.CategoryId == id)
            .ToListAsync(cancellationToken);

        var definitionIds = definitions
    .Select(x => x.Id)
    .ToList();

        var options = await _dbContext
    .AttributeOptions
    .AsNoTracking()
    .Where(x =>
        definitionIds.Contains(x.AttributeDefinitionId))
    .ToListAsync(cancellationToken);


        foreach (var definitionRecord in definitions)
        {
            var definition =
                CategoryAttributeDefinition.Rehydrate(
                    definitionRecord.Id,
                    definitionRecord.Name,
                    definitionRecord.Type,
                    definitionRecord.IsRequired);

            var definitionOptions = options
                .Where(x =>
                    x.AttributeDefinitionId == definitionRecord.Id);

            foreach (var optionRecord in definitionOptions)
            {
                definition.AddOption(
                    AttributeOption.Create(optionRecord.Value));
            }

            category.AddAttributeDefinition(definition);
        }

        return category;
    }


    public async Task AddAsync(
        Category category,
        CancellationToken cancellationToken)
    {
        await _dbContext.Categories.AddAsync(
            category,
            cancellationToken);

        foreach (var definition in category.AttributeDefinitions)
        {
            await _dbContext.CategoryAttributeDefinitions.AddAsync(
                new CategoryAttributeDefinitionRecord
                {
                    Id = definition.Id,
                    CategoryId = category.Id,
                    Name = definition.Name,
                    Type = definition.Type,
                    IsRequired = definition.IsRequired
                },
                cancellationToken);

            foreach (var option in definition.Options)
            {
                await _dbContext.AttributeOptions.AddAsync(
                    new AttributeOptionRecord
                    {
                        Id = Guid.NewGuid(),
                        AttributeDefinitionId = definition.Id,
                        Value = option.Value
                    },
                    cancellationToken);
            }
        }
    }

}
