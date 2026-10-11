using System.Data;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class Functions
    {
        private readonly string _connectionString;

        public Functions(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("TMS") ?? throw new InvalidOperationException("Connection string 'TMS' not found in configuration.");
        }

        public void INSERT_LOGS(string log_text, string log_by)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            SqlCommand command = new SqlCommand(
                "insert into MYLOGS(LOG_TEXT,LOG_BY,LOG_TIME) values(@log_text,@log_by,GETDATE())",
                connection);

            command.Parameters.AddWithValue("@log_text", log_text);
            command.Parameters.AddWithValue("@log_by", log_by);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public string GET_ESCALATION_TIME_DIFFERENCE_SECONDARY(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);

            SqlCommand cmd = new SqlCommand("SELECT SCHEDULE_TIME FROM ESCALATIONS_SECONDARY WHERE ID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            SqlDataAdapter da = new SqlDataAdapter(cmd);

            DataTable dt = new DataTable();

            conn.Open();
            da.Fill(dt);
            conn.Close();

            if (dt.Rows.Count == 0)
                return "UNKNOWN";

            if (!DateTime.TryParse(dt.Rows[0]["SCHEDULE_TIME"]?.ToString(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime scheduleTime))
            {
                return "UNKNOWN";
            }

            return scheduleTime > DateTime.Now.AddMinutes(1)
                ? "OK"
                : "UNKNOWN";
        }

        public int CANCLE_ESCALATIONS_SECONDARY_SINGLE(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            SqlCommand command = new SqlCommand(
                "UPDATE ESCALATIONS_SECONDARY SET SEND_STATUS='CANCEL' WHERE ID=@id",
                connection);

            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            return command.ExecuteNonQuery();
        }
    }
}