using Microsoft.EntityFrameworkCore;

using FluentValidation;

using Shop.Catalog.Api.Data;
using Shop.Catalog.Api.Seed;
using Shop.Catalog.Api.Services;
using Shop.Catalog.Api.Options;

using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Shop.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Catalog")
    ?? throw new InvalidOperationException("Connection string Catalog not found");

builder.Services.AddDbContext<CatalogDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICatalogReadService, CatalogReadService>();

builder.Services.AddAutoMapper(
    cfg => cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"],
    typeof(Program));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var jwt = 
    builder.Configuration
        .GetSection(JwtValidationOptions.SectionName)
        .Get<JwtValidationOptions>()
        ?? throw new InvalidOperationException("Configuration section Jwt no found");

Validator.ValidateObject(jwt, new ValidationContext(jwt), validateAllProperties: true);

var rsa = RSA.Create();
rsa.ImportFromPem(File.ReadAllText(jwt.PublicKeyPath));
var validationKey = new RsaSecurityKey(rsa);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = validationKey,
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Sub,
            RoleClaimType = ShopClaimTypes.Role,
        };
    });
builder.Services.AddAuthorization();


var app = builder.Build();
await app.SeedCatalogAsync();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
