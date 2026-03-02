using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventFinder.Data;
using EventFinder.Models;

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

    public async Task<IActionResult> Events(string city, string category, DateTime? date)
    {
        var query = _context.Events
            .Include(e => e.RSVPs)
            .AsQueryable();

        if (!string.IsNullOrEmpty(city))
            query = query.Where(e => e.City == city);

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category == category);

        if (date.HasValue)
            query = query.Where(e => e.StartDate.Date == date.Value.Date);

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

        return View(@event);
    }

    [HttpGet]
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
    public async Task<IActionResult> Create(Event @event)
    {
        if (ModelState.IsValid)
        {
            @event.CreatedAt = DateTime.UtcNow;
            @event.OrganizerId = "temp-user-id";
            @event.OrganizerName = "Anonymous User";

            _context.Add(@event);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Details), new { id = @event.Id });
        }
        return View(@event);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RSVP(int eventId, RSVPStatus status)
    {
        var existingRSVP = await _context.RSVPs
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == "temp-user-id");

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
                UserId = "temp-user-id",
                UserName = "Anonymous User",
                Status = status
            };
            _context.RSVPs.Add(rsvp);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = eventId });
    }
}