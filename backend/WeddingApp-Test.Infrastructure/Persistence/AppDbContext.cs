using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();
}
