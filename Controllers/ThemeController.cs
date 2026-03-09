using Microsoft.AspNetCore.Mvc;

namespace EventFinder.Controllers
{
    public class ThemeController : Controller
    {
        // Save theme preference in session or cookie
        [HttpPost]
        public IActionResult SetTheme(string theme)
        {
            // Store in cookie (lasts 1 year)
            CookieOptions option = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1),
                HttpOnly = true,
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Append("UserTheme", theme, option);

            return Json(new { success = true, theme = theme });
        }

        // Get current theme
        [HttpGet]
        public IActionResult GetTheme()
        {
            var theme = Request.Cookies["UserTheme"] ?? "light";
            return Json(new { theme = theme });
        }
    }
}