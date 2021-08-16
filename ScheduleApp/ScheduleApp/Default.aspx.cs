using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using ScheduleApp.Models;

namespace ScheduleApp
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            string password = txtPassword.Text?.Trim()?.ToLowerInvariant();
            if (password != "moremen")
            {
                lblError.Text = "Invalid Password";
                lblError.Visible = true;
                return;
            }

            string username = txtName.Text?.Trim()?.ToLowerInvariant();

            bool existingUser = LoginExistingUser(username);

            if (!existingUser)
            {
                CreateNewUser(username);
            }

            Response.Redirect("~/Pages/Scheduling.aspx", true);
        }

        private bool LoginExistingUser(string username)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"SELECT user_id
                               FROM users u WITH (NOLOCK)
                               WHERE username=@username";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = Convert.ToInt32(reader["user_id"]);

                            User user = new User(username, id);
                            Session["User"] = user;

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void CreateNewUser(string username)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString))
            {
                conn.Open();
                string sql = @"INSERT INTO users (username) 
                               OUTPUT INSERTED.user_id
                               VALUES (@username)";
                using (var command = new SqlCommand(sql, conn))
                {
                    command.Parameters.AddWithValue("@username", username);

                    int newUserId = Convert.ToInt32(command.ExecuteScalar());

                    User user = new User(username, newUserId);
                    Session["User"] = user;
                }
            }
        }
    }
}