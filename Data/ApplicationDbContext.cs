using Microsoft.EntityFrameworkCore;
using EventFinder.Models;

namespace EventFinder.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<RSVP> RSVPs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Event
        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            // Indexes for faster queries
            entity.HasIndex(e => e.StartDate);
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.City);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });
        });

        // Configure RSVP
        modelBuilder.Entity<RSVP>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasIndex(e => e.EventId);
            entity.HasIndex(e => e.UserId);

            // One RSVP per user per event
            entity.HasIndex(e => new { e.EventId, e.UserId })
                .IsUnique();

            // Relationship
            entity.HasOne(e => e.Event)
                .WithMany(e => e.RSVPs)
                .HasForeignKey(e => e.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}