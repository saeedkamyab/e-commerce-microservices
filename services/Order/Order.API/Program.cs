using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Order.API.Authentication;
using Order.API.Endpoints.Orders;
using Order.Application;
using Order.Application.Abstractions.Authentication;
using Order.Infrastructure;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUser,
    CurrentUser>();


var jwtSection =
    builder.Configuration.GetSection("Jwt");

var issuer =
    jwtSection["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is not configured.");

var audience =
    jwtSection["Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is not configured.");

var secretKey =
    jwtSection["SecretKey"]
    ?? throw new InvalidOperationException(
        "JWT SecretKey is not configured.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            options.MapInboundClaims = false;

            options.TokenValidationParameters =
                new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,

                    ValidateAudience = true,
                    ValidAudience = audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                secretKey)),

                    ValidateLifetime = true,

                    ClockSkew = TimeSpan.Zero
                };
        });

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();



app.MapCreateOrderEndpoint();
app.MapGetOrderByIdEndpoint();


app.Run();


