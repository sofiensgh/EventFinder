using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace EventFinder.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "Full Name")]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Registration Date")]
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public virtual ICollection<RSVP> RSVPs { get; set; } = new List<RSVP>();
}
