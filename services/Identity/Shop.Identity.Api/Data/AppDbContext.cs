using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shop.Identity.Api.Entities;

using System.Security.Cryptography.X509Certificates;

namespace Shop.Identity.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid> 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        builder.Entity<AppUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasDatabaseName("EmailIndex")
            .IsUnique();
    }
    
    
}