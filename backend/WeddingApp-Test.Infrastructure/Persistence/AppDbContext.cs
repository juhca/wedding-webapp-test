using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// enum will be saved as a string to db
		modelBuilder.Entity<User>()
			.Property(u => u.Role)
			.HasConversion<string>();
	}
}
