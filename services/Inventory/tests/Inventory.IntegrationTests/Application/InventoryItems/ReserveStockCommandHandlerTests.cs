using Inventory.Application.Abstractions.Persistence;
using Inventory.Application.InventoryItems.Commands.ReserveStock;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.IntegrationTests.Application.InventoryItems;

public sealed class ReserveStockCommandHandlerTests
{
    [Fact]
    public async Task Handle_When_First_Save_Has_Concurrency_Conflict_Should_Reload_And_Retry()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var firstState =
            InventoryItem.Create(productId);

        firstState.IncreaseStock(10);

        var reloadedState =
            InventoryItem.Create(productId);

        reloadedState.IncreaseStock(10);

        var repository =
            new Mock<IInventoryItemRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        repository
            .SetupSequence(x =>
                x.GetByProductIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstState)
            .ReturnsAsync(reloadedState);

        repository
            .Setup(x =>
                x.ReloadByProductIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(reloadedState);

        unitOfWork
            .SetupSequence(x =>
                x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DbUpdateConcurrencyException())
            .ReturnsAsync(1);

        var handler =
            new ReserveStockCommandHandler(
                repository.Object,
                unitOfWork.Object);

        // Act
        await handler.Handle(
            new ReserveStockCommand(
                productId,
                4),
            CancellationToken.None);

        // Assert
        repository.Verify(
            x => x.ReloadByProductIdAsync(
                productId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        Assert.Equal(
            4,
            reloadedState.ReservedQuantity);
    }
    [Fact]
    public async Task Handle_After_Concurrency_Conflict_Should_Throw_When_Reloaded_Stock_Is_Insufficient()
    {
        // Arrange
        var productId = Guid.NewGuid();

        var staleState =
            InventoryItem.Create(productId);

        staleState.IncreaseStock(10);

        var currentState =
            InventoryItem.Create(productId);

        currentState.IncreaseStock(10);

        // فرض کن درخواست دیگری قبلاً 7 عدد رزرو کرده
        currentState.Reserve(7);

        var repository =
            new Mock<IInventoryItemRepository>();

        var unitOfWork =
            new Mock<IUnitOfWork>();

        repository
            .SetupSequence(x =>
                x.GetByProductIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(staleState)
            .ReturnsAsync(currentState);

        repository
            .Setup(x =>
                x.ReloadByProductIdAsync(
                    productId,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentState);

        unitOfWork
            .Setup(x =>
                x.SaveChangesAsync(
                    It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DbUpdateConcurrencyException());

        var handler =
            new ReserveStockCommandHandler(
                repository.Object,
                unitOfWork.Object);

        // Act
        var action = () =>
            handler.Handle(
                new ReserveStockCommand(
                    productId,
                    6),
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            action);

        Assert.Equal(
            3,
            currentState.AvailableQuantity);
    }
}
