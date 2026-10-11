using AlarmSystem.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
    public class WorkOrderService
    {
        private readonly IConfiguration _configuration;

        public WorkOrderService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        //=========================================
        // GET WORK ORDERS
        //=========================================

        public List<WorkOrder> GetWorkOrders(string workGroup)
        {
            List<WorkOrder> list = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(
                "WORK_ORDERS_2", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@work_group", workGroup);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                WorkOrder item = new WorkOrder();

                item.CLARITY_REF = dr["CLARITY_REF"].ToString();
                item.CIRCUIT = dr["CIRCUIT"].ToString();
                item.CUSTOMER = dr["CUSTOMER"].ToString();
                item.WORK_ORDER = dr["WORK ORDER"].ToString();
                item.WO_STATUS = dr["WO STATUS"].ToString();

                if (dr["PENDING HRS"] != DBNull.Value)
                    item.PENDING_HRS =
                        Convert.ToInt32(dr["PENDING HRS"]);

                item.WORKGROUP = dr["WORKGROUP"].ToString();

                list.Add(item);
            }

            conn.Close();

            return list;
        }
    }
}