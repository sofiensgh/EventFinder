using System.ComponentModel.DataAnnotations;

namespace EventFinder.Models;

public class RSVP
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Your Name")]
    public string UserName { get; set; } = string.Empty;

    [Display(Name = "RSVP Date")]
    public DateTime RSVPDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Display(Name = "Status")]
    public RSVPStatus Status { get; set; }

    // Navigation property
    public virtual Event? Event { get; set; }
}

public enum RSVPStatus
{
    [Display(Name = "✅ Going")]
    Going,

    [Display(Name = "🤔 Maybe")]
    Maybe,

    [Display(Name = "❌ Not Going")]
    NotGoing
}