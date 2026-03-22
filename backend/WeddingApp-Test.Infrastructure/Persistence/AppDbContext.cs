using Microsoft.EntityFrameworkCore;
using WeddingApp_Test.Domain.Entities;

namespace WeddingApp_Test.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<User> Users => Set<User>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<WeddingInfo> WeddingInfo => Set<WeddingInfo>();
	public DbSet<Rsvp> Rsvps => Set<Rsvp>();
	public DbSet<GuestCompanion> GuestCompanions => Set<GuestCompanion>();
	public DbSet<Gift> Gifts => Set<Gift>();
	public DbSet<GiftReservation> GiftReservations => Set<GiftReservation>();
	public DbSet<Reminder> Reminders => Set<Reminder>();

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
		
		// RSVP CONFIGURATION
		modelBuilder.Entity<Rsvp>(entity =>
		{
			entity.HasKey(r => r.Id);
			
			entity.HasOne(r => r.User)
				.WithOne()
				.HasForeignKey<Rsvp>(r => r.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			entity.HasIndex(r => r.UserId).IsUnique();
			entity.HasIndex(r => r.IsAttending);
			entity.HasIndex(r => r.RespondedAt);
			
			// Ignore computed property
			entity.Ignore(r => r.TotalGuests);
		});

		
		// GUEST COMPANION CONFIGURATION
		modelBuilder.Entity<GuestCompanion>(entity =>
		{
			entity.HasKey(g => g.Id);
			
			// Many companion belong to on RSVP
			entity.HasOne(g => g.Rsvp)
				.WithMany(c => c.Companions)
				.HasForeignKey(g => g.RsvpId)
				.OnDelete(DeleteBehavior.Cascade); // Delete companions when RSVP is deleted
			
			entity.HasIndex(gc => gc.RsvpId);
		});
		
		// GIFT CONFIGURATION
		modelBuilder.Entity<Gift>(entity =>
		{
			entity.HasKey(e => e.Id);
            
			entity.HasIndex(g => g.DisplayOrder);
			entity.HasIndex(g => g.IsVisible);
            
			// Decimal precision for price
			entity.Property(g => g.Price).HasPrecision(10, 2);
            
			// Ignore computed properties (don't save to DB)
			entity.Ignore(g => g.ReservationCount);
			entity.Ignore(g => g.IsFullyReserved);
			entity.Ignore(g => g.RemainingReservations);
		});
        
		// REMINDER CONFIGURATION
		modelBuilder.Entity<Reminder>(entity =>
		{
			entity.HasKey(r => r.Id);

			entity.Property(r => r.Type).HasConversion<string>();
			entity.Property(r => r.Unit).HasConversion<string>();

			// No DB-level FK — TargetId semantics depend on Type
			entity.HasIndex(r => new { r.Type, r.TargetId });
			entity.HasIndex(r => r.ScheduledFor);
		});

		// GIFT RESERVATION CONFIGURATION
		modelBuilder.Entity<GiftReservation>(entity =>
		{
			entity.HasKey(e => e.Id);
            
			// Many reservations belong to one gift
			entity.HasOne(gr => gr.Gift)
				.WithMany(g => g.Reservations)
				.HasForeignKey(gr => gr.GiftId)
				.OnDelete(DeleteBehavior.Cascade); // Delete reservations when gift is deleted
            
			// Many reservations belong to one user
			entity.HasOne(gr => gr.ReservedBy)
				.WithMany()
				.HasForeignKey(gr => gr.ReservedByUserId)
				.OnDelete(DeleteBehavior.Cascade); // Don't delete reservations when user is deleted
            
			entity.HasIndex(gr => gr.GiftId);
			entity.HasIndex(gr => gr.ReservedByUserId);
			entity.HasIndex(gr => gr.ReservedAt);
            
			// UNIQUE CONSTRAINT - user can reserve same gift only once
			entity.HasIndex(gr => new { gr.GiftId, gr.ReservedByUserId }).IsUnique();
		});
	}
}
