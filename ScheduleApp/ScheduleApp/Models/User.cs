
namespace ScheduleApp.Models
{
    public class User
    {
        public string Username { get; protected set; }
        public int Id { get; protected set; }

        public User(string username, int id)
        {
            Username = username;
            Id = id;
        }
    }
}