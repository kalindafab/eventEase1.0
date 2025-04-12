using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;



public class Event
{
    public int EventId { get; set; }
    public string EventTitle { get; set; }
    public string EventDescription { get; set; }
    public DateTime EventDate { get; set; }
    public string EventLocation { get; set; }
    public string EventImage { get; set; } // URL or file path for the image
}




namespace eventEase1._0.Pages
{
    public class myEventsModel : PageModel
    {
        private void FetchManagerEvents(string managerId)
        {
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT EventId, EventTitle, EventDate, EventLocation FROM Events WHERE ManagerId = @ManagerId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ManagerId", managerId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            myEventsModel = new List<Event>();
                            while (reader.Read())
                            {
                                ManagerEvents.Add(new Event
                                {
                                    EventId = (int)reader["EventId"],
                                    EventTitle = reader["EventTitle"].ToString(),
                                    EventDate = (DateTime)reader["EventDate"],
                                    EventLocation = reader["EventLocation"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions here, like database connectivity issues.
            }
        }
    }


}
