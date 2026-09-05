using Identity.API.Authentication;
using Identity.API.Endpoints;
using Identity.API.ExceptionHandling;
using Identity.Application;
using Identity.Application.Abstractions.Authentication;
using Identity.Infrastructure;
using Identity.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions =
            builder.Configuration
                .GetSection("Jwt")
                .Get<JwtOptions>()
            ?? throw new InvalidOperationException(
                "Jwt configuration is missing.");
        options.MapInboundClaims = false;
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,

                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SecretKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<
    ICurrentUser,
    CurrentUser>();

builder.Services.AddExceptionHandler<
    GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();


app.MapRegisterUserEndpoint();
app.MapLoginUserEndpoint();
app.MapGoogleLogin();
app.MapLogoutUserEndpoint();
app.MapGetCurrentUserEndpoint();
app.MapRefreshTokenEndpoint();


app.Run();


