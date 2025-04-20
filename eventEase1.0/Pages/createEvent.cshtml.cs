using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Security.Claims;

namespace eventEase1._0.Pages
{
    public class createEventModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public createEventModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        // Form-bound properties
        [BindProperty]
        [Required]
        public string Name { get; set; }

        [BindProperty]
        [Required]
        public decimal Price { get; set; }

        [BindProperty]
        [Required]
        public int Tickets { get; set; }

        [BindProperty]
        [Required]
        public DateTime EventDate { get; set; }

        [BindProperty]
        [Required]
        public string Location { get; set; }

        [BindProperty]
        [Required]
        public IFormFile EventImage { get; set; }

        [BindProperty]
        [Required]
        public string EventDescription { get; set; }

        public void OnGet() 
        {}

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            string imageFileName = null;

            try
            {
                // 1. Handle Image Upload
                if (EventImage != null)
                {
                    string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    imageFileName = Guid.NewGuid().ToString() + Path.GetExtension(EventImage.FileName);
                    string filePath = Path.Combine(uploadsFolder, imageFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await EventImage.CopyToAsync(stream);
                    }
                }

                // 2. Get Logged-in User's ID from Claims
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    ModelState.AddModelError(string.Empty, "User is not authenticated.");
                    return Page();
                }
                var userId = Guid.Parse(userIdClaim.Value); // Make sure it's a valid UUID

                // 3. Insert Into DB
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"INSERT INTO Events 
                (Id, Name, Price, Tickets, EventDate, Location, ImagePath, Description, UserId) 
                VALUES (@Id, @Name, @Price, @Tickets, @EventDate, @Location, @ImagePath, @Description, @UserId)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", Guid.NewGuid());
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Price", Price);
                        command.Parameters.AddWithValue("@Tickets", Tickets);
                        command.Parameters.AddWithValue("@EventDate", EventDate);
                        command.Parameters.AddWithValue("@Location", Location);
                        command.Parameters.AddWithValue("@ImagePath", imageFileName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", EventDescription);
                        command.Parameters.AddWithValue("@UserId", userId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            TempData["SuccessMessage"] = " Event created successfully!";
                            return RedirectToPage("/Dashboard");
                        }

                        ModelState.AddModelError(string.Empty, "Failed to create event.");
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

