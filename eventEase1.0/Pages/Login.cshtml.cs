using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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

        public async Task<IActionResult> OnPostAsync()
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
                        command.Parameters.AddWithValue("@Password", Password);  // Note: In production, use password hashing

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

                                // Create claims for the user
                                var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.NameIdentifier, userId),
                                    new Claim(ClaimTypes.Name, email),
                                    new Claim("FirstName", firstName),
                                    new Claim(ClaimTypes.Role, role),
                                    new Claim("Organization", organization)
                                };

                                // Add permissions based on role
                                AddRolePermissions(claims, role);

                                // Create claims identity
                                var claimsIdentity = new ClaimsIdentity(
                                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                // Sign in the user
                                await HttpContext.SignInAsync(
                                    CookieAuthenticationDefaults.AuthenticationScheme,
                                    new ClaimsPrincipal(claimsIdentity),
                                    new AuthenticationProperties
                                    {
                                        IsPersistent = false // Set to true for "remember me" functionality
                                    });

                                // Redirect to the dashboard after successful login
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

        private void AddRolePermissions(List<Claim> claims, string role)
        {
            // Add permissions based on the user's role
            switch (role.ToLower())
            {
                case "admin":
                    claims.Add(new Claim("Permission", "CanApproveManagers"));
                    claims.Add(new Claim("Permission", "CanManageUsers"));
                    claims.Add(new Claim("Permission", "CanManageSettings"));
                    break;

                case "event_manager":
                    claims.Add(new Claim("Permission", "CanCreateEvents"));
                    claims.Add(new Claim("Permission", "CanViewOwnEvents"));
                    claims.Add(new Claim("Permission", "CanViewTicketSales"));
                    break;

                case "user":
                    claims.Add(new Claim("Permission", "CanBrowseEvents"));
                    claims.Add(new Claim("Permission", "CanViewOwnTickets"));
                    break;
            }

            // Common permissions for all roles
            claims.Add(new Claim("Permission", "CanManageProfile"));
        }
    }
}