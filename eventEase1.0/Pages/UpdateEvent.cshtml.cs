using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace eventEase1._0.Pages
{
    public class UpdateEventModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public UpdateEventModel(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        [BindProperty]
        public Guid EventId { get; set; }

        [BindProperty]
        public string Name { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public int Tickets { get; set; }

        [BindProperty]
        public DateTime EventDate { get; set; }

        [BindProperty]
        public string Location { get; set; }

        [BindProperty]
        public string EventDescription { get; set; }

        [BindProperty]
        public IFormFile EventImage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = "SELECT * FROM Events WHERE Id = @Id";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                EventId = id;
                Name = reader["Name"].ToString();
                Price = Convert.ToDecimal(reader["Price"]);
                Tickets = Convert.ToInt32(reader["Tickets"]);
                EventDate = Convert.ToDateTime(reader["EventDate"]);
                Location = reader["Location"].ToString();
                EventDescription = reader["Description"].ToString();
                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            string imagePath = null;

            if (EventImage != null)
            {
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                imagePath = Guid.NewGuid().ToString() + Path.GetExtension(EventImage.FileName);
                string filePath = Path.Combine(uploadsFolder, imagePath);

                using var stream = new FileStream(filePath, FileMode.Create);
                await EventImage.CopyToAsync(stream);
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using SqlConnection connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            string query = @"UPDATE Events 
                             SET Name = @Name, Price = @Price, Tickets = @Tickets, EventDate = @EventDate, 
                                 Location = @Location, Description = @Description" +
                            (imagePath != null ? ", ImagePath = @ImagePath" : "") +
                            " WHERE Id = @Id";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", Name);
            command.Parameters.AddWithValue("@Price", Price);
            command.Parameters.AddWithValue("@Tickets", Tickets);
            command.Parameters.AddWithValue("@EventDate", EventDate);
            command.Parameters.AddWithValue("@Location", Location);
            command.Parameters.AddWithValue("@Description", EventDescription);
            if (imagePath != null) command.Parameters.AddWithValue("@ImagePath", imagePath);
            command.Parameters.AddWithValue("@Id", EventId);

            int result = await command.ExecuteNonQueryAsync();

            if (result > 0)
            {
                TempData["SuccessMessage"] = "Event updated successfully!";
                return RedirectToPage("/myEvents");
            }

            ModelState.AddModelError("", "Failed to update event.");
            return Page();
        }
    }
}
