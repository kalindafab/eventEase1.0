using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace eventEase1._0.Pages
{
    public class approveManagersModel : PageModel
    {
        public string FirstName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public List<PendingManager> PendingManagers { get; set; } = new List<PendingManager>();

        private readonly IConfiguration _configuration;

        public approveManagersModel(IConfiguration configuration)
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
                    LoadPendingManagers();
                }
            }
        }
        private void LoadPendingManagers()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT id, firstName, lastName, email, organization FROM Users WHERE role = 'manager' AND status = 'pending'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            PendingManagers.Add(new PendingManager
                            {
                                Id = reader["id"].ToString(),
                                FirstName = reader["firstName"].ToString(),
                                LastName = reader["lastName"].ToString(),
                                Email = reader["email"].ToString(),
                                Organization = reader["organization"].ToString()
                            });
                        }
                    }
                }
            }
        }

        public IActionResult OnPostApproveManager(string managerId)
        {
            UpdateManagerStatus(managerId, "approved");
            return RedirectToPage();
        }

        public IActionResult OnPostRejectManager(string managerId)
        {
            UpdateManagerStatus(managerId, "rejected");
            return RedirectToPage();
        }

        private void UpdateManagerStatus(string managerId, string status)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Users SET status = @Status WHERE id = @ManagerId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@ManagerId", managerId);
                    command.ExecuteNonQuery();
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
}
