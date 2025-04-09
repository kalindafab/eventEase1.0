using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace eventEase1._0.Pages
{
    public class SignupModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; }

        [BindProperty]
        public string Organization { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please select a role")]
        public string Role { get; set; }

        private readonly IConfiguration _configuration;

        public SignupModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet() { }

        public IActionResult OnPost()
        {
            if (Role == "manager" && string.IsNullOrEmpty(Organization))
            {
                ModelState.AddModelError("Organization", "Organization is required for managers");
            }
            else if (Role != "manager")
            {
                
                ModelState.Remove("Organization");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                string userId = Guid.NewGuid().ToString();
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Users (id, firstName, lastName, email, password, role, organization) " +
                                 "VALUES (@Id, @FirstName, @LastName, @Email, @Password, @Role, @Organization)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", userId);
                        command.Parameters.AddWithValue("@FirstName", FirstName);
                        command.Parameters.AddWithValue("@LastName", LastName);
                        command.Parameters.AddWithValue("@Email", Email);
                        command.Parameters.AddWithValue("@Password", Password);
                        command.Parameters.AddWithValue("@Role", Role);
                        command.Parameters.AddWithValue("@Organization",
                            string.IsNullOrEmpty(Organization) ? DBNull.Value : (object)Organization);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            return RedirectToPage("Login");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "There was an error signing up. Please try again.");
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