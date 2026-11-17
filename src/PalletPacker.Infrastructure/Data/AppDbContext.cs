using Microsoft.EntityFrameworkCore;
using System;

namespace PalletPacker.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }
}
