using Order.API.Endpoints.Orders;
using Order.Application;
using Order.Infrastructure;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();


app.MapCreateOrderEndpoint();
app.MapGetOrderByIdEndpoint();


app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
