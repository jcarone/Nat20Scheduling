using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using ScheduleApp.Models;

namespace ScheduleApp.Pages
{
    public partial class Scheduling : Page
    {
        int daysToShow = 14;
        int dayWidth = 210;
        int dayHeight = 300;
        DateTime today;
        User loggedInUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["User"] == null)
            {
                Response.Redirect("~/", true);
                return;
            }

            loggedInUser = Session["User"] as User;
            lblUsername.InnerText = loggedInUser.Username + "'s";

            today = DateTime.UtcNow.AddHours(-5);

            PopulateCalendar();

            if (!this.IsPostBack)
            {
                PopulateAvailabilityDropdowns(today);
            }
        }

        private void PopulateCalendar()
        {
            pnlDayList.Controls.Clear();

            DateTime day = today;
            int weekOffset = 0;

            if (today.DayOfWeek == DayOfWeek.Sunday)
            {
                //if today is Sunday shift everything up one row so there isn't a blank row at the top
                weekOffset--;
            }

            for (int i = 0; i < daysToShow; i++)
            {
                //move the new week to the next row. If today is Sunday don't leave the entire top row blank
                if (day.DayOfWeek == DayOfWeek.Sunday)
                {
                    weekOffset++;
                }

                //create the calendar day
                HtmlGenericControl dayDiv = GetDayDiv(day, weekOffset);

                //dropdown option for each day's Availability
                if (!this.IsPostBack)
                {
                    string dropDownOptionText = day.ToString("dddd, MMMM d");
                    string dropDownOptionValue = day.ToString("d");
                    drpAvailabilityDay.Items.Add(new ListItem(dropDownOptionText, dropDownOptionValue));
                }

                day = day.AddDays(1);

                pnlDayList.Controls.Add(dayDiv);
            }
        }

        private void PopulateAvailabilityDropdowns(DateTime date)
        {
            //6pm to 10pm with a wider range on weekends
            int startTime = 1080;
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                startTime = 600;
            }

            for (int i = startTime; i <= 1320; i += 30)
            {
                DateTime timeOfDay = new DateTime(2000, 1, 1).AddMinutes(i);
                string dropDownOptionText = timeOfDay.ToString("h:mm tt EST");

                drpAvailabilityStart.Items.Add(new ListItem(dropDownOptionText, timeOfDay.ToString()));
                drpAvailabilityEnd.Items.Add(new ListItem(dropDownOptionText, timeOfDay.ToString()));
            }

            //check if this user already saved a response for this date so we can make it easier to edit
            bool foundExisiting = false;
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT top 1 start_time_minutes, end_time_minutes, comments 
                               FROM user_availability
                               WHERE user_id=@user_id 
                               AND CONVERT(date, date) = CONVERT(date, @date)";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@date", date.ToString());
                    command.Parameters.AddWithValue("@user_id", loggedInUser.Id);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            DateTime selectedStartTime = new DateTime(2000, 1, 1).AddMinutes(Convert.ToInt32(reader["start_time_minutes"]));
                            drpAvailabilityStart.SelectedValue = selectedStartTime.ToString();

                            DateTime selectedEndTime = new DateTime(2000, 1, 1).AddMinutes(Convert.ToInt32(reader["end_time_minutes"]));
                            drpAvailabilityEnd.SelectedValue = selectedEndTime.ToString();

                            txtComments.Text = reader["comments"]?.ToString();
                            foundExisiting = true;
                        }
                    }
                }
            }

            if (!foundExisiting)
            {
                //default to selecting the last day
                drpAvailabilityEnd.SelectedIndex = drpAvailabilityEnd.Items.Count - 1;
            }
        }

        //returns what this calendar day looks like
        private HtmlGenericControl GetDayDiv(DateTime date, int weekOffset)
        {
            int xOffset = GetDayOfWeekOffset(date.DayOfWeek);
            int yOffset = weekOffset * dayHeight;

            HtmlGenericControl container = new HtmlGenericControl();
            container.Attributes["class"] = "Day";
            container.Attributes["style"] = $"left:{xOffset}px; top:{yOffset}px;";

            //Name of the day
            HtmlGenericControl dayName = new HtmlGenericControl();
            dayName.Attributes["class"] = "DayName";
            dayName.InnerText = date.ToString("dddd");
            container.Controls.Add(dayName);

            //Month + date
            HtmlGenericControl dayDateDetails = new HtmlGenericControl();
            dayDateDetails.Attributes["class"] = "DayDate";

            HtmlGenericControl monthName = new HtmlGenericControl();
            monthName.InnerText = date.ToString("MMM");
            monthName.Attributes["style"] = "float:left; padding-left:5px;";
            dayDateDetails.Controls.Add(monthName);

            HtmlGenericControl dateNumber = new HtmlGenericControl();
            dateNumber.InnerText = date.Date.ToString(" d");
            dateNumber.Attributes["style"] = "float:right; padding-right:5px;";
            dayDateDetails.Controls.Add(dateNumber);
            container.Controls.Add(dayDateDetails);

            //who is available on this day
            List<Availability> availableUsers = GetAvailableUsersOnDate(date);
            foreach (var availableUser in availableUsers)
            {
                HtmlGenericControl userDiv = new HtmlGenericControl();
                userDiv.Attributes["class"] = "UserAvailability";

                HtmlGenericControl userName = new HtmlGenericControl();
                userName.InnerText = availableUser.Username;
                if (availableUser.Comments?.Length != 0)
                {
                    userName.InnerText = userName.InnerText + "*";
                    userDiv.Attributes["title"] = availableUser.Comments;
                }
                userName.Attributes["style"] = "float:left;";
                userDiv.Controls.Add(userName);

                DateTime startTime = new DateTime(2000, 1, 1).AddMinutes(availableUser.StartMinutes);
                DateTime endTime = new DateTime(2000, 1, 1).AddMinutes(availableUser.EndMinutes);
                string availableTime = $"{startTime.ToString("h:mmtt")}-{endTime.ToString("h:mmtt")}";

                HtmlGenericControl userAvailableTimes = new HtmlGenericControl();
                userAvailableTimes.InnerText = availableTime;
                userAvailableTimes.Attributes["style"] = "float:right;";
                userDiv.Controls.Add(userAvailableTimes);

                //only add the cancel option if it's for a time this user set
                if (availableUser.Id == loggedInUser.Id)
                {
                    ImageButton cancelAvailability = new ImageButton();
                    cancelAvailability.ImageUrl = "../Images/x.png";
                    cancelAvailability.Attributes["class"] = "CancelAvailability";
                    cancelAvailability.Click += (sender, e) => CancelAvailability(date);
                    userAvailableTimes.Controls.Add(cancelAvailability);
                }

                container.Controls.Add(userDiv);
            }

            //if everyone is available on this day mark it green
            //TO-DO: actually check the users rather than assume 5 distinct 
            //TO-DO: check time overlap
            if (availableUsers.Count == 5)
            {
                container.Attributes["class"] = container.Attributes["class"] + " AllAvailableDay";
            }

            return container;
        }

        private void CancelAvailability(DateTime selectedDate)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
            {
                conn.Open();

                string sql = @"DELETE FROM user_availability WHERE user_id=@user_id AND CONVERT(date, date) = CONVERT(date, @date)";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@user_id", loggedInUser.Id);
                    command.Parameters.AddWithValue("@date", selectedDate.ToString());
                    command.ExecuteNonQuery();
                }
            }

            PopulateCalendar();
        }

        private List<Availability> GetAvailableUsersOnDate(DateTime date)
        {
            List<Availability> availableUsers = new List<Availability>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT username, u.user_id, start_time_minutes, end_time_minutes, comments 
                               FROM user_availability ua WITH (NOLOCK)
                               INNER JOIN users u WITH (NOLOCK) ON u.user_id=ua.user_id
                               WHERE CONVERT(date, date) = CONVERT(date, @date)";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@date", date.ToString());

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string userName = reader["username"].ToString();
                            int id = Convert.ToInt32(reader["user_id"]);
                            int start = Convert.ToInt32(reader["start_time_minutes"]);
                            int end = Convert.ToInt32(reader["end_time_minutes"]);
                            string comments = reader["comments"].ToString();

                            availableUsers.Add(new Availability(userName, id, start, end, comments));
                        }
                    }
                }
            }

            return availableUsers;
        }

        protected void drpAvailabilityDay_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DateTime.TryParse(drpAvailabilityDay.SelectedValue, out DateTime selectedDate))
            {
                drpAvailabilityStart.Items.Clear();
                drpAvailabilityEnd.Items.Clear();

                PopulateAvailabilityDropdowns(selectedDate);
            }
        }

        protected void btnAvailability_Click(object sender, EventArgs e)
        {
            if (DateTime.TryParse(drpAvailabilityDay.SelectedValue, out DateTime selectedDate) &&
                DateTime.TryParse(drpAvailabilityStart.SelectedValue, out DateTime selectedStart) &&
                DateTime.TryParse(drpAvailabilityEnd.SelectedValue, out DateTime selectedEnd))
            {
                int startMinutes = selectedStart.Hour * 60 + selectedStart.Minute;
                int endMinutes = selectedEnd.Hour * 60 + selectedEnd.Minute;
                string comments = txtComments.Text?.Trim();

                if (endMinutes <= startMinutes || endMinutes < 0 || startMinutes < 0 || endMinutes >= 1440 || startMinutes >= 1440)
                {
                    return;
                }

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
                {
                    conn.Open();

                    string sql = @"
IF EXISTS (SELECT 1 FROM user_availability WHERE user_id=@user_id AND CONVERT(date, date) = CONVERT(date, @date))
BEGIN
	UPDATE user_availability SET
		start_time_minutes = @start_time_minutes,
		end_time_minutes = @end_time_minutes,
        comments = @comments
	WHERE user_id=@user_id AND CONVERT(date, date) = CONVERT(date, @date)
END
ELSE
BEGIN
	INSERT INTO user_availability (user_id, date, start_time_minutes, end_time_minutes, comments) VALUES (@user_id, @date, @start_time_minutes, @end_time_minutes, @comments)
END";
                    using (var command = new SqlCommand(sql, conn))
                    {
                        command.Parameters.AddWithValue("@user_id", loggedInUser.Id);
                        command.Parameters.AddWithValue("@date", selectedDate.ToString());
                        command.Parameters.AddWithValue("@start_time_minutes", startMinutes);
                        command.Parameters.AddWithValue("@end_time_minutes", endMinutes);
                        command.Parameters.AddWithValue("@comments", comments);
                        command.ExecuteNonQuery();
                    }
                }

                txtComments.Text = "";
                PopulateCalendar();
            }
        }

        private int GetDayOfWeekOffset(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                default:
                case DayOfWeek.Sunday:
                    return 0;
                case DayOfWeek.Monday:
                    return dayWidth;
                case DayOfWeek.Tuesday:
                    return dayWidth * 2;
                case DayOfWeek.Wednesday:
                    return dayWidth * 3;
                case DayOfWeek.Thursday:
                    return dayWidth * 4;
                case DayOfWeek.Friday:
                    return dayWidth * 5;
                case DayOfWeek.Saturday:
                    return dayWidth * 6;
            }
        }
    }
}