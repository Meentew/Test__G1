using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Test____G1.Pages
{
    public class WelcomeModel : PageModel
    {
        public string FullName { get; set; } = "";

        public IActionResult OnGet()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToPage("Login");
            }

            var fullName = HttpContext.Session.GetString("FullName");
            FullName = !string.IsNullOrEmpty(fullName) ? fullName : username;

            return Page();
        }

        public IActionResult OnPost()
        {
            HttpContext.Session.Clear();
            return RedirectToPage("Login");
        }
    }
}
