using Microsoft.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace eventEase1._0.Pages.User
{
    public class MyTicketsModel : PageModel
    {
        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        private readonly IConfiguration _configuration;

        public MyTicketsModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            if (!User.HasClaim("Permission", Permissions.CanViewOwnTickets))
            {
                RedirectToPage("/AccessDenied");
                return;
            }

            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            LoadTickets(userId);
        }

        private void LoadTickets(string userId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"SELECT t.Id, t.PurchaseDate, t.Status, 
                   e.Name as EventName, e.StartDate, e.Location,
                   u.organization as Organizer
                   FROM Tickets t
                   JOIN Events e ON LOWER(t.EventId) = LOWER(e.Id)
                   JOIN Users u ON LOWER(e.OrganizerId) = LOWER(u.Id)
                   WHERE LOWER(t.UserId) = LOWER(@UserId)
                   ORDER BY e.StartDate ASC";


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Tickets.Add(new Ticket
                            {
                                Id = reader["Id"].ToString(),
                                PurchaseDate = DateTime.Parse(reader["PurchaseDate"].ToString()),
                                Status = reader["Status"].ToString(),
                                EventName = reader["EventName"].ToString(),
                                EventDate = DateTime.Parse(reader["StartDate"].ToString()),
                                Location = reader["Location"].ToString(),
                                Organizer = reader["Organizer"].ToString()
                            });
                        }
                    }
                }
            }
        }


        [ValidateAntiForgeryToken]
        public IActionResult OnPostCancelTicket(string ticketId)
        {
            if (!User.HasClaim("Permission", Permissions.CanViewOwnTickets))
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

                    // Verify the ticket belongs to the user
                    string verifyQuery = "SELECT COUNT(*) FROM Tickets WHERE LOWER(Id) = LOWER(@TicketId) AND LOWER(UserId) = LOWER(@UserId)";
                    using (SqlCommand verifyCommand = new SqlCommand(verifyQuery, connection))
                    {
                        verifyCommand.Parameters.AddWithValue("@TicketId", ticketId);
                        verifyCommand.Parameters.AddWithValue("@UserId", userId);

                        int count = (int)verifyCommand.ExecuteScalar();
                        if (count == 0)
                        {
                            TempData["ErrorMessage"] = "Ticket not found or doesn't belong to you.";
                            return RedirectToPage();
                        }
                    }

                    // Cancel the ticket
                    string updateQuery = "UPDATE Tickets SET Status = 'cancelled' WHERE Id = @TicketId";
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@TicketId", ticketId);
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = "Ticket cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error cancelling ticket: {ex.Message}";
            }

            return RedirectToPage();
        }
    }

    public class Ticket
    {
        public string Id { get; set; }
        public DateTime PurchaseDate { get; set; }
        public string Status { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string Location { get; set; }
        public string Organizer { get; set; }
    }
}