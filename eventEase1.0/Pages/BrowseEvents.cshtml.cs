using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace eventEase1._0.Pages
{
    public class BrowseEventsModel : PageModel
    {
        public List<Event> Events { get; set; } = new List<Event>();

        private readonly IConfiguration _configuration;

        public BrowseEventsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet(string searchTerm = "", string category = "")
        {
            if (!User.HasClaim("Permission", Permissions.CanBrowseEvents))
            {
                RedirectToPage("/AccessDenied");
                return;
            }

            LoadEvents(searchTerm, category);
        }

        private void LoadEvents(string searchTerm, string category)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT e.Id, e.Name, e.Description, e.Location, e.StartDate, e.EndDate, 
                               e.Price, e.Capacity, e.Category, u.organization as Organizer
                               FROM Events e
                               JOIN Users u ON e.OrganizerId = u.Id
                               WHERE e.Status = 'approved' 
                               AND e.StartDate > GETDATE()";

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query += " AND (e.Name LIKE @SearchTerm OR e.Description LIKE @SearchTerm)";
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query += " AND e.Category = @Category";
                }

                query += " ORDER BY e.StartDate ASC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        command.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");
                    }

                    if (!string.IsNullOrEmpty(category))
                    {
                        command.Parameters.AddWithValue("@Category", category);
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Events.Add(new Event
                            {
                                Id = reader["Id"].ToString(),
                                Name = reader["Name"].ToString(),
                                Description = reader["Description"].ToString(),
                                Location = reader["Location"].ToString(),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                Price = decimal.Parse(reader["Price"].ToString()),
                                Capacity = int.Parse(reader["Capacity"].ToString()),
                                Category = reader["Category"].ToString(),
                                Organizer = reader["Organizer"].ToString()
                            });
                        }
                    }
                }
            }
        }

        [ValidateAntiForgeryToken]
        public IActionResult OnPostRegisterForEvent(string eventId)
        {
            if (!User.HasClaim("Permission", Permissions.CanBrowseEvents))
            {
                return RedirectToPage("/AccessDenied");
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if event exists and has capacity
                    string checkQuery = @"SELECT Capacity, 
                                (SELECT COUNT(*) FROM Tickets WHERE LOWER(EventId) = LOWER(@EventId)) AS Registered 
                                FROM Events WHERE LOWER(Id) = LOWER(@EventId)";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@EventId", eventId);
                        using (SqlDataReader reader = checkCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int capacity = int.Parse(reader["Capacity"].ToString());
                                int registered = int.Parse(reader["Registered"].ToString());

                                if (registered >= capacity)
                                {
                                    TempData["ErrorMessage"] = "This event is already full.";
                                    return RedirectToPage();
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Event not found.";
                                return RedirectToPage();
                            }
                        }
                    }

                    // Register user for event
                    string insertQuery = @"INSERT INTO Tickets (Id, UserId, EventId, PurchaseDate, Status) 
                                 VALUES (LOWER(NEWID()), @UserId, @EventId, GETDATE(), 'confirmed')";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@EventId", eventId);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "You have successfully registered for the event!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error registering for event: {ex.Message}";
            }

            return RedirectToPage();
        }


        public int GetRegisteredCount(string eventId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Tickets WHERE LOWER(EventId) = LOWER(@EventId)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EventId", eventId);
                    return (int)command.ExecuteScalar();
                }
            }
        }

    }




    public class Event
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string Category { get; set; }
        public string Organizer { get; set; }
    }
}