using System.ComponentModel.DataAnnotations;

namespace EventFinder.Models;

public class Event
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [Display(Name = "Event Title")]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [Display(Name = "Description")]
    [StringLength(2000, MinimumLength = 10)]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Start date is required")]
    [Display(Name = "Start Date")]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; } = DateTime.Now.AddDays(7);

    [Required(ErrorMessage = "End date is required")]
    [Display(Name = "End Date")]
    [DataType(DataType.DateTime)]
    public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7).AddHours(2);

    [Required(ErrorMessage = "Address is required")]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "City is required")]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "Country is required")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    public double Latitude { get; set; }

    [Required]
    public double Longitude { get; set; }

    [Required(ErrorMessage = "Category is required")]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "Event Image URL")]
    [DataType(DataType.ImageUrl)]
    public string? ImageUrl { get; set; }

    public string OrganizerId { get; set; } = "temp-user-id";

    [Display(Name = "Organized By")]
    public string OrganizerName { get; set; } = "Anonymous";

    [Display(Name = "Created On")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual ICollection<RSVP> RSVPs { get; set; } = new List<RSVP>();

    // Computed property
    [Display(Name = "Attendees")]
    public int AttendeeCount => RSVPs?.Count(r => r.Status == RSVPStatus.Going) ?? 0;
}