using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eventEase1._0.Pages
{
    public class EventsModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public EventsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<EventItem> EventItem { get; set; } = new List<EventItem>();

        public async Task OnGetAsync()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT Id, Name, Price, Tickets, EventDate, Location, ImagePath FROM Events";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
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
                            };
                            EventItem.Add(evt);
                        }
                    }
                }
            }
        }
    }

    public class EventItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Tickets { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string ImagePath { get; set; }
    }
}
