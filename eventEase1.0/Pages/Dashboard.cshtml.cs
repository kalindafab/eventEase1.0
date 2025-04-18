using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace eventEase1._0.Pages
{
    public class DashboardModel : PageModel
    {
        public string FirstName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }

        private readonly IConfiguration _configuration;

        public DashboardModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public void OnGet()
        {
            if (User.Identity.IsAuthenticated)
            {
              
                FirstName = User.FindFirst("FirstName")?.Value ?? "User";
                Role = User.FindFirst(ClaimTypes.Role)?.Value ?? "Guest";
                Email = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;
                Organization = User.FindFirst("Organization")?.Value ?? string.Empty;

                if (User.HasClaim("Permission", "CanApproveManagers"))
                {
                    
                }
            }
        }
        


        

    }


    public class PendingManager
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
    }
}


