using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eventEase1._0.Pages
{
    public class DashboardModel : PageModel
    {
        public string UserName { get; set; } = "User"; // Replace with actual session or claims logic

        public void OnGet()
        {
            // Example: get username from session or claims
            if (User.Identity.IsAuthenticated)
            {
                UserName = User.Identity.Name;
            }
        }
    }
}
