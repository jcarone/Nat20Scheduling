
namespace ScheduleApp.Models
{
    public class Availability
    {
        public string Username { get; protected set; }
        public int Id { get; protected set; }
        public int StartMinutes { get; protected set; }
        public int EndMinutes { get; protected set; }
        public string Comments { get; protected set; }

        public Availability(string username, int id, int startMinutes, int endMinutes, string comments)
        {
            Username = username;
            Id = id;
            StartMinutes = startMinutes;
            EndMinutes = endMinutes;
            Comments = comments;
        }
    }
}