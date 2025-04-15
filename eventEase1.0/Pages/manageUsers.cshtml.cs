using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eventEase1._0.Pages
{
    public class manageUsersModel : PageModel
    {
        public string FirstName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public List<allUser> allUsers { get; set; } = new List<allUser>();

        private readonly IConfiguration _configuration;

        public manageUsersModel(IConfiguration configuration)
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
                    LoadAllUsers();
                }
            }
        }
        private void LoadAllUsers()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, firstName, lastName, email, organization, role FROM Users";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allUsers.Add(new allUser
                            {
                                Id = reader["id"].ToString(),
                                FirstName = reader["firstName"].ToString(),
                                LastName = reader["lastName"].ToString(),
                                Email = reader["email"].ToString(),
                                Organization = reader["organization"].ToString(),
                                Role = reader["role"].ToString()
                            });
                        }
                    }
                }
            }
        }

        public IActionResult OnPostApproveManager(string managerId)
        {
            //UpdateManagerStatus(managerId, "approved");
            return RedirectToPage();
        }

        public IActionResult OnPostRejectManager(string managerId)
        {
            //UpdateManagerStatus(managerId, "rejected");
            return RedirectToPage();
        }

        

        public class allUser
        {
            public string Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Organization { get; set; }
            public string Role { get; set; }
        }

    }
}
