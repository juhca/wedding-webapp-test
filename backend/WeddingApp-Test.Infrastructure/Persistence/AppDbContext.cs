using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<WeddingInfo> WeddingInfo => Set<WeddingInfo>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// USER CONFIGURATION
		// enum will be saved as a string to db
		modelBuilder.Entity<User>(entity =>
		{
			entity.HasKey(u => u.Id);
			
			entity.Property(u => u.Role)
				.HasConversion<string>();
		});
			
		// REFRESH TOKEN CONFIGURATION
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
		
		// WEDDING INFO CONFIGURATION
		modelBuilder.Entity<WeddingInfo>(entity =>
		{
			entity.HasKey(w => w.Id);
			entity.HasOne(w => w.UpdatedBy)
				.WithMany()
				.HasForeignKey(w => w.UpdatedByUserId)
				.OnDelete(DeleteBehavior.SetNull);
			
			// Decimal precision for coordinates
			entity.Property(w => w.CivilLocationLatitude).HasPrecision(10, 7);
			entity.Property(w => w.CivilLocationLongitude).HasPrecision(10, 7);
			entity.Property(w => w.ChurchLocationLatitude).HasPrecision(10, 7);
			entity.Property(w => w.ChurchLocationLongitude).HasPrecision(10, 7);
			entity.Property(w => w.PartyLocationLatitude).HasPrecision(10, 7);
			entity.Property(w => w.PartyLocationLongitude).HasPrecision(10, 7);
			entity.Property(w => w.HouseLocationLatitude).HasPrecision(10, 7);
			entity.Property(w => w.HouseLocationLongitude).HasPrecision(10, 7);
		});

	}
}
