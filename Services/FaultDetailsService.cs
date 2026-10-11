using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class FaultDetailsService
    {
        private readonly IConfiguration _configuration;

        public FaultDetailsService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        // ===========================
        // CLARITY FAULT DETAILS
        // ===========================
        public FaultDetail GetFaultDetail(int clarityRef)
        {
            FaultDetail item = null;

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
SELECT *
FROM CLARITY_DATA_NW_FAULTS
WHERE problem_no=@id", conn);

            cmd.Parameters.AddWithValue("@id", clarityRef);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                item = new FaultDetail();

                item.ProblemNo = Convert.ToInt32(dr["problem_no"]);
                item.CustomerName = dr["customer_name"]?.ToString();
                item.CircuitName = dr["circuit_name"]?.ToString();
                item.CircuitType = dr["circuit_type"]?.ToString();
                item.Description = dr["description"]?.ToString();
                item.Nature = dr["nature"]?.ToString();

                if (dr["reportedtime"] != DBNull.Value)
                    item.ReportedTime =
                        Convert.ToDateTime(dr["reportedtime"]);

                item.Priority = dr["priority"]?.ToString();
                item.SLAType = dr["SLA_type"]?.ToString();
                item.SLATime = dr["SLA_time"]?.ToString();
                item.WorkGroup = dr["workgroup"]?.ToString();
                item.CustomerType = dr["customer_type"]?.ToString();
                item.EnterBy = dr["enter_by"]?.ToString();
                item.Status = dr["problem_status"]?.ToString();
                item.ReportedBy = dr["reported_by"]?.ToString();
                item.ReportedContact = dr["reported_contact"]?.ToString();

                // Escalation values
                item.Node = dr["cr"]?.ToString();
                item.FaultName = dr["description"]?.ToString();

                if (dr["reportedtime"] != DBNull.Value)
                    item.FaultTime =
                        Convert.ToDateTime(dr["reportedtime"]);
            }

            conn.Close();

            return item;
        }

        // ===========================
        // WORK ORDERS
        // ===========================
        public List<FaultWorkOrder> GetWorkOrders(int clarityRef)
        {
            List<FaultWorkOrder> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
SELECT *
FROM CLARITY_WO_STATUS
WHERE CLARITY_REF=@id", conn);

            cmd.Parameters.AddWithValue("@id", clarityRef);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                FaultWorkOrder item = new FaultWorkOrder();

                item.WO_REF = dr["WO_REF"]?.ToString();
                item.WO_STATUS = dr["WO_STATUS"]?.ToString();

                if (dr["STARTED_TIME"] != DBNull.Value)
                    item.STARTED_TIME =
                        Convert.ToDateTime(dr["STARTED_TIME"]);

                item.WORK_GROUP = dr["WORK_GROUP"]?.ToString();

                list.Add(item);
            }

            conn.Close();

            return list;
        }

        // ===========================
        // COMMENTS / HISTORY
        // ===========================
        public List<FaultComment> GetComments(int clarityRef)
        {
            List<FaultComment> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
SELECT *
FROM CLARITY_COMMENTS
WHERE docket_id=@id
ORDER BY event_time DESC", conn);

            cmd.Parameters.AddWithValue("@id", clarityRef);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                FaultComment item = new FaultComment();

                if (dr["docket_id"] != DBNull.Value)
                    item.DOCKET_ID =
                        Convert.ToInt32(dr["docket_id"]);

                item.DESCRIPTION = dr["description"]?.ToString();

                if (dr["event_time"] != DBNull.Value)
                    item.EVENT_TIME =
                        Convert.ToDateTime(dr["event_time"]);

                list.Add(item);
            }

            conn.Close();

            return list;
        }
    }
}