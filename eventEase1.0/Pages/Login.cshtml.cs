using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace eventEase1._0.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Select query to fetch user details by email and password
                    string query = "SELECT id, firstName, lastName, email, role, organization FROM Users WHERE email = @Email AND password = @Password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", Password);  // Ensure password is securely hashed in production

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // User found, retrieve details
                                string userId = reader["id"].ToString();
                                string firstName = reader["firstName"].ToString();
                                string role = reader["role"].ToString();
                                string organization = reader["organization"].ToString();
                                string email = reader["email"].ToString();

                                // Store user information in session or claims
                                HttpContext.Session.SetString("UserId", userId);
                                HttpContext.Session.SetString("FirstName", firstName);
                                HttpContext.Session.SetString("Role", role);
                                HttpContext.Session.SetString("Email", email);
                                HttpContext.Session.SetString("Organization", organization);

                                // Redirect to the dashboard or home page after successful login
                                return RedirectToPage("Dashboard");
                            }
                            else
                            {
                                ModelState.AddModelError(string.Empty, "Invalid login attempt. Please check your email and password.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error: {ex.Message}");
            }

            return Page();
        }

    }
}
