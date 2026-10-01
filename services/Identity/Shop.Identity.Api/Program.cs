using FluentValidation;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using Shop.Identity.Api.Data;
using Shop.Identity.Api.Entities;
using Shop.Identity.Api.Options;
using Shop.Identity.Api.Services;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() 
    ?? throw new InvalidOperationException("...");

Validator.ValidateObject(jwtOptions, new ValidationContext(jwtOptions), validateAllProperties: true);
var rsa = RSA.Create();
rsa.ImportFromPem(jwtOptions.PrivateKey);
var signingKey = new RsaSecurityKey(rsa);

builder.Services.AddSingleton(signingKey);

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<ITokenService, TokenService>();
var app = builder.Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var connectionString = builder.Configuration.GetConnectionString("Identity") ?? throw new InvalidOperationException("Connection string 'Identity' not found");
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
builder.Services
    .AddIdentityCore<AppUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();




// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseAuthorization();

app.MapControllers();

app.Run();
