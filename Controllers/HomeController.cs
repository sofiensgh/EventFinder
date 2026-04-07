using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EventFinder.Data;
using EventFinder.Models;
using System.Security.Claims;

namespace EventFinder.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var upcomingEvents = await _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.StartDate >= DateTime.Today)
            .OrderBy(e => e.StartDate)
            .Take(6)
            .ToListAsync();

        return View(upcomingEvents);
    }

    public async Task<IActionResult> Events(string city, string category, DateTime? date, string search)
    {
        var query = _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.StartDate >= DateTime.Today.AddDays(-1)) // Show ongoing events too
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(city))
            query = query.Where(e => e.City == city);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);

        if (date.HasValue)
            query = query.Where(e => e.StartDate.Date == date.Value.Date);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(e => e.Title.Contains(search) || e.Description.Contains(search));

        var events = await query
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var @event = await _context.Events
            .Include(e => e.RSVPs)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (@event == null)
            return NotFound();

        // Get similar events for the view
        ViewBag.SimilarEvents = await GetSimilarEvents(@event);

        return View(@event);
    }

    [HttpGet]
    [Authorize]
    public IActionResult Create()
    {
        return View(new Event
        {
            StartDate = DateTime.Now.AddDays(7),
            EndDate = DateTime.Now.AddDays(7).AddHours(2)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Create(Event @event)
    {
        if (ModelState.IsValid)
        {
            @event.CreatedAt = DateTime.UtcNow;
            @event.OrganizerId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            @event.OrganizerName = User.Identity?.Name ?? "Unknown";

            _context.Add(@event);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Details), new { id = @event.Id });
        }
        return View(@event);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> RSVP(int eventId, RSVPStatus status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return RedirectToAction("Login", "Account");
        }

        var existingRSVP = await _context.RSVPs
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

        if (existingRSVP != null)
        {
            existingRSVP.Status = status;
            existingRSVP.RSVPDate = DateTime.UtcNow;
        }
        else
        {
            var rsvp = new RSVP
            {
                EventId = eventId,
                UserId = userId,
                UserName = User.Identity?.Name ?? "Unknown",
                Status = status
            };
            _context.RSVPs.Add(rsvp);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    // NEW: Map view
    public async Task<IActionResult> Map()
    {
        var events = await _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.StartDate >= DateTime.Today.AddDays(-1)) // Show ongoing and upcoming events
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return View(events);
    }

    // NEW: About page
    public IActionResult About()
    {
        return View();
    }

    // NEW: Contact page
    public IActionResult Contact()
    {
        return View();
    }

    // NEW: Privacy policy
    public IActionResult Privacy()
    {
        return View();
    }

    // NEW: Terms of service
    public IActionResult Terms()
    {
        return View();
    }

    // NEW: FAQ page
    public IActionResult FAQ()
    {
        return View();
    }

    // NEW: My Events (for authenticated users)
    [Authorize]
    public async Task<IActionResult> MyEvents()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var myEvents = await _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.OrganizerId == userId ||
                       e.RSVPs.Any(r => r.UserId == userId))
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return View(myEvents);
    }

    // NEW: Edit event (for organizers)
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var @event = await _context.Events.FindAsync(id);

        if (@event == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user is the organizer
        if (@event.OrganizerId != userId)
            return Forbid();

        return View(@event);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Edit(int id, Event @event)
    {
        if (id != @event.Id)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user is the organizer
        var existingEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        if (existingEvent == null || existingEvent.OrganizerId != userId)
            return Forbid();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(@event);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(@event.Id))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Details), new { id = @event.Id });
        }
        return View(@event);
    }

    // NEW: Delete event (for organizers)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var @event = await _context.Events
            .Include(e => e.RSVPs)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (@event == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Check if user is the organizer
        if (@event.OrganizerId != userId)
            return Forbid();

        // Remove all RSVPs first (cascade delete should handle this, but just to be safe)
        _context.RSVPs.RemoveRange(@event.RSVPs);

        // Remove the event
        _context.Events.Remove(@event);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Event cancelled successfully!";
        return RedirectToAction(nameof(Index));
    }

    // NEW: Cancel RSVP
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelRSVP(int eventId)
    {
        var rsvp = await _context.RSVPs
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == "temp-user-id");

        if (rsvp != null)
        {
            _context.RSVPs.Remove(rsvp);
            await _context.SaveChangesAsync();
            TempData["Success"] = "RSVP cancelled successfully!";
        }

        return RedirectToAction(nameof(Details), new { id = eventId });
    }

    // NEW: Get event attendees (for AJAX calls)
    [HttpGet]
    public async Task<IActionResult> GetAttendees(int eventId)
    {
        var attendees = await _context.RSVPs
            .Where(r => r.EventId == eventId && r.Status == RSVPStatus.Going)
            .Select(r => new { r.UserName, r.RSVPDate })
            .ToListAsync();

        return Json(attendees);
    }

    // NEW: Search events (for AJAX autocomplete)
    [HttpGet]
    public async Task<IActionResult> SearchEvents(string term)
    {
        var events = await _context.Events
            .Where(e => e.Title.Contains(term) || e.City.Contains(term) || e.Category.Contains(term))
            .Select(e => new { e.Id, e.Title, e.City, e.Category })
            .Take(10)
            .ToListAsync();

        return Json(events);
    }

    // Helper method to get similar events
    private async Task<List<Event>> GetSimilarEvents(Event currentEvent)
    {
        return await _context.Events
            .Include(e => e.RSVPs)
            .Where(e => e.Id != currentEvent.Id
                        && e.Category == currentEvent.Category
                        && e.City == currentEvent.City
                        && e.StartDate >= DateTime.Today)
            .OrderBy(e => e.StartDate)
            .Take(3)
            .ToListAsync();
    }

    // Helper method to check if event exists
    private bool EventExists(int id)
    {
        return _context.Events.Any(e => e.Id == id);
    }
}