using Inventory.API.Endpoints;
using Inventory.API.Endpoints.InventoryItems;
using Inventory.Application;
using Inventory.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapInventoryEndpoints();

app.Run();


