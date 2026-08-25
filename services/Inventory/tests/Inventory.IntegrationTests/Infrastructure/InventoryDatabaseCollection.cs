using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.IntegrationTests.Infrastructure;

[CollectionDefinition(Name)]
public sealed class InventoryDatabaseCollection
    : ICollectionFixture<InventoryDatabaseFixture>
{
    public const string Name = "InventoryDatabase";
}
