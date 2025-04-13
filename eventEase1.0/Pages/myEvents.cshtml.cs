using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Claims;

namespace eventEase1._0.Pages
{
    public class myEventsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public myEventsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<EventItem> UserEvents { get; set; } = new List<EventItem>();

        public class EventItem
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int Tickets { get; set; }
            public DateTime EventDate { get; set; }
            public string Location { get; set; }
            public string ImagePath { get; set; }
            public int SoldTickets => TotalTickets - Tickets;
            public int TotalTickets { get; set; }  // Original Ticket count
        }

        public void OnGet()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                TempData["ErrorMessage"] = "User not logged in.";
                return;
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT Id, Name, Price, Tickets, EventDate, Location, ImagePath, Tickets as TotalTickets
                                 FROM Events WHERE UserId = @UserId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var evt = new EventItem
                            {
                                Id = reader.GetGuid(0),
                                Name = reader.GetString(1),
                                Price = reader.GetDecimal(2),
                                Tickets = reader.GetInt32(3),
                                EventDate = reader.GetDateTime(4),
                                Location = reader.GetString(5),
                                ImagePath = reader.IsDBNull(6) ? "default-event.jpg" : reader.GetString(6),
                                TotalTickets = reader.GetInt32(7)  // Assuming the original tickets were stored here
                            };
                            UserEvents.Add(evt);
                        }
                    }
                }
            }
        }
    }
}
