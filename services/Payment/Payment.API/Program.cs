using Payment.Infrastructure;
using Payment.Application;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();




app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
