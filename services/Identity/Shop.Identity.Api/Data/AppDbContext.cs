using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shop.Identity.Api.Entities;

namespace Shop.Identity.Api.Data;

public class AppDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid> 
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
}