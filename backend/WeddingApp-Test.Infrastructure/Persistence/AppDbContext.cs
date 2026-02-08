using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// enum will be saved as a string to db
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(u => u.Id);
			
			entity.Property(u => u.Role)
				.HasConversion<string>();
		});
			

		modelBuilder.Entity<RefreshToken>(entity =>
		{
			entity.HasKey(rt => rt.Id);
			
			entity.Property(rt => rt.Token)
				.IsRequired()
				.HasMaxLength(256);
			
			// Each token has to be unique
			entity.HasIndex(rt => rt.Token)
				.IsUnique();

			entity.Property(rt => rt.Created)
				.IsRequired();
			
			entity.Property(rt => rt.Expires)
				.IsRequired();

			entity.Property(rt => rt.IsRevoked)
				.IsRequired();

			// Ignore computed property
			entity.Ignore(rt => rt.IsActive);

			// Configure relationship
			entity.HasOne(rt => rt.User)
				.WithMany(u => u.RefreshTokens)
				.HasForeignKey(rt => rt.UserId)
				.OnDelete(DeleteBehavior.Cascade);
		});

	}
}
