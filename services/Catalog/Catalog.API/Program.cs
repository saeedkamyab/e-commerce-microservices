using Catalog.API.Endpoints.Categories;
using Catalog.API.Endpoints.Products;
using Catalog.Application;
using Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

//app.MapControllers();

//Category
app.MapCreateCategoryEndpoint();
app.MapGetCategoryByIdEndpoint();

//Product
app.MapCreateProductEndpoint();
app.MapGetProductByIdEndpoint();
app.MapActivateProductEndpoint();


app.Run();
