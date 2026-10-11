using AlarmSystem.Models.Alarm;
using Microsoft.Data.SqlClient;
using System.Data;
using AlarmSystem.Models;
using AlarmSystem.Models;

namespace AlarmSystem.Services
{
    public class AlarmService
    {
        private readonly IConfiguration _configuration;

        public AlarmService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

      private SqlConnection GetConnection()
{
    return new SqlConnection(
        _configuration.GetConnectionString("TMS"));
}

        //=====================================================
        // GET ALL ALARMS
        //=====================================================

        public List<AlarmModel> GetAlarms(
            string platform = "~ALL~",
            string nwEng = "~ALL~",
            string alarmType = "~ALL~")
        {
            List<AlarmModel> alarms = new();

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand("REPORT_2_test", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@platform", platform);
            cmd.Parameters.AddWithValue("@nw_eng", nwEng);
            cmd.Parameters.AddWithValue("@alarm_type", alarmType);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                AlarmModel model = new AlarmModel();

                model.ID = Convert.ToInt32(dr["ID"]);
                model.NODE = dr["NODE"].ToString();
                model.PLATFORM = dr["PLATFORM"].ToString();
                model.FAULT = dr["FAULT"].ToString();
                model.ALARM_TYPE = dr["ALARM_TYPE"].ToString();

                if (dr["FAULT_TIME"] != DBNull.Value)
                    model.FAULT_TIME = Convert.ToDateTime(dr["FAULT_TIME"]);

                if (dr["HOURS"] != DBNull.Value)
                    model.HOURS = Convert.ToInt32(dr["HOURS"]);

                model.NW_ENG = dr["NW_ENG"].ToString();

                if (dr["REVENUE"] != DBNull.Value)
                    model.REVENUE = Convert.ToDecimal(dr["REVENUE"]);

                    

                alarms.Add(model);
            }

            conn.Close();

            return alarms;
        }

        //=====================================================
        // GET SINGLE ALARM
        //=====================================================

        public AlarmModel GetAlarmDetails(int id)
        {
            AlarmModel alarm = null;

            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(
                "SELECT * FROM FAULTS WHERE ID=@ID", conn);

            cmd.Parameters.AddWithValue("@ID", id);

            conn.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                alarm = new AlarmModel
{
    ID = Convert.ToInt32(dr["ID"]),
    NODE = dr["NODE"].ToString(),
    PLATFORM = dr["PLATFORM"].ToString(),

    
    FAULT = dr["FAULT"].ToString(),
    ALARM_TYPE = dr["ALARM_TYPE"].ToString(),

    FAULT_STATUS = dr["FAULT_STATUS"].ToString(),

   

    FAULT_TIME = dr["FAULT_TIME"] == DBNull.Value
        ? DateTime.MinValue
        : Convert.ToDateTime(dr["FAULT_TIME"]),

    CLOSE_TIME = dr["CLOSE_TIME"] == DBNull.Value
        ? null
        : Convert.ToDateTime(dr["CLOSE_TIME"]),

   HOURS = 0,

    NW_ENG = dr["NW_ENG"].ToString(),

    REVENUE = dr["REVENUE"] == DBNull.Value
        ? 0
        : Convert.ToDecimal(dr["REVENUE"]),
        VENDOR = dr["VENDOR"] == DBNull.Value
    ? ""
    : dr["VENDOR"].ToString(),

PROVINCE = dr["PROVINCE"] == DBNull.Value
    ? ""
    : dr["PROVINCE"].ToString(),

REGION = dr["REGION"] == DBNull.Value
    ? ""
    : dr["REGION"].ToString(),

TAG = dr["TAG"] == DBNull.Value
    ? ""
    : dr["TAG"].ToString(),

ACK_STATUS = dr["ACK_STATUS"] == DBNull.Value
    ? ""
    : dr["ACK_STATUS"].ToString(),

ACK_BY = dr["ACK_BY"] == DBNull.Value
    ? ""
    : dr["ACK_BY"].ToString(),

REF_ID = dr["REF_ID"] == DBNull.Value
    ? ""
    : dr["REF_ID"].ToString(),

SOURCEIP = dr["SOURCEIP"] == DBNull.Value
    ? ""
    : dr["SOURCEIP"].ToString(),

ACK_TIME = dr["ACK_TIME"] == DBNull.Value
    ? null
    : Convert.ToDateTime(dr["ACK_TIME"])
};


            }

            conn.Close();

            return alarm;
        }

        public int GetTicketIdFromAlarm(int alarmId)
{
    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT TOP 1 TICKET_ID
        FROM ATTRIBUTES
        WHERE KEY_FIELD='ALARM_REF'
        AND VALUE_FIELD=@AlarmId", conn);

    cmd.Parameters.AddWithValue("@AlarmId", alarmId);

    conn.Open();

    object result = cmd.ExecuteScalar();

    conn.Close();

    if (result == null || result == DBNull.Value)
        return 0;

    return Convert.ToInt32(result);
}

        //=====================================================
        // CLOSE ALARM
        //=====================================================

        public bool CloseAlarm(int id)
        {
            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
                UPDATE FAULTS
                SET FAULT_STATUS='CLOSE',
                    CLOSE_TIME=GETDATE()
                WHERE ID=@ID", conn);

            cmd.Parameters.AddWithValue("@ID", id);

            conn.Open();

            int rows = cmd.ExecuteNonQuery();

            conn.Close();

            return rows > 0;
        }
                //=====================================================
        // CHECK TICKET EXISTS
        //=====================================================

        public int TicketExists(string node, string fault)
        {
            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 ID
                FROM T_TICKETS
                WHERE NODE=@NODE
                AND DESCRIPTION=@FAULT", conn);

            cmd.Parameters.AddWithValue("@NODE", node);
            cmd.Parameters.AddWithValue("@FAULT", fault);

            conn.Open();

            object obj = cmd.ExecuteScalar();

            conn.Close();

            if (obj == null || obj == DBNull.Value)
                return 0;

            return Convert.ToInt32(obj);
        }
                //=====================================================
        // ADD ALARM ATTRIBUTE
        //=====================================================

        public void AddAlarmReference(int ticketId, int alarmId)
        {
            using SqlConnection conn = GetConnection();

            SqlCommand cmd = new SqlCommand(@"
                INSERT INTO T_ATTRIBUTES
                (
                    TICKET_ID,
                    ATTRIBUTE_NAME,
                    ATTRIBUTE_VALUE
                )
                VALUES
                (
                    @TicketId,
                    'ALARM_REF',
                    @AlarmId
                )", conn);

            cmd.Parameters.AddWithValue("@TicketId", ticketId);
            cmd.Parameters.AddWithValue("@AlarmId", alarmId);

            conn.Open();

            cmd.ExecuteNonQuery();

            conn.Close();
        }
        //=====================================================
// CREATE TICKET FROM ALARM
//=====================================================

public int CreateTicketFault(
    string node,
    string severity,
    string ticketType,
    string tag,
    string platform,
    string workgroup,
    string description,
    string creationMethod,
    DateTime issueTime,
    DateTime reportingTime,
    string reportingBy,
    string reportingPlatform,
    string responsibleOfficer,
    string message,
    string userProfile)
{
    node = node.Replace("'", " ");
    description = description.Replace("'", " ");

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
    INSERT INTO T_TICKETS
    (
        NODE,
        SEVERITY,
        TICKET_TYPE,
        TAG,
        PLATFORM,
        WORKGROUP,
        DESCRIPTION,
        CREATION_METHOD,
        ISSUE_TIME,
        REPORTING_TIME,
        REPORTING_BY,
        REPORTING_PLATFORM,
        STATUS,
        RESPONSIBLE_OFFICER,
        MESSAGE_TEXT,
        USER_PROFILE
    )
    VALUES
    (
        @NODE,
        @SEVERITY,
        @TICKET_TYPE,
        @TAG,
        @PLATFORM,
        @WORKGROUP,
        @DESCRIPTION,
        @CREATION_METHOD,
        @ISSUE_TIME,
        @REPORTING_TIME,
        @REPORTING_BY,
        @REPORTING_PLATFORM,
        'OPEN',
        @RESPONSIBLE_OFFICER,
        @MESSAGE_TEXT,
        @USER_PROFILE
    );

    SELECT SCOPE_IDENTITY();
    ", conn);

    cmd.Parameters.AddWithValue("@NODE", node);
    cmd.Parameters.AddWithValue("@SEVERITY", severity);
    cmd.Parameters.AddWithValue("@TICKET_TYPE", ticketType);
    cmd.Parameters.AddWithValue("@TAG", tag);
    cmd.Parameters.AddWithValue("@PLATFORM", platform);
    cmd.Parameters.AddWithValue("@WORKGROUP", workgroup);
    cmd.Parameters.AddWithValue("@DESCRIPTION", description);
    cmd.Parameters.AddWithValue("@CREATION_METHOD", creationMethod);
    cmd.Parameters.AddWithValue("@ISSUE_TIME", issueTime);
    cmd.Parameters.AddWithValue("@REPORTING_TIME", reportingTime);
    cmd.Parameters.AddWithValue("@REPORTING_BY", reportingBy);
    cmd.Parameters.AddWithValue("@REPORTING_PLATFORM", reportingPlatform);
    cmd.Parameters.AddWithValue("@RESPONSIBLE_OFFICER", responsibleOfficer);
    cmd.Parameters.AddWithValue("@MESSAGE_TEXT", message);
    cmd.Parameters.AddWithValue("@USER_PROFILE", userProfile);

    conn.Open();

    object result = cmd.ExecuteScalar();

    conn.Close();

    return Convert.ToInt32(result);
}
public List<string> GetPlatforms()
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT PLATFORM
        FROM FAULTS
        WHERE PLATFORM IS NOT NULL
        ORDER BY PLATFORM", conn);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    list.Add("~ALL~");

    while (dr.Read())
    {
        list.Add(dr["PLATFORM"].ToString());
    }

    conn.Close();

    return list;
}

public List<string> GetAlarmTypes()
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT ALARM_TYPE
        FROM FAULTS
        WHERE ALARM_TYPE IS NOT NULL
        ORDER BY ALARM_TYPE", conn);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    list.Add("~ALL~");

    while (dr.Read())
    {
        list.Add(dr["ALARM_TYPE"].ToString());
    }

    conn.Close();

    return list;
}

public List<string> GetNetworkEngineers()
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT NW_ENG
        FROM FAULTS
        WHERE NW_ENG IS NOT NULL
        ORDER BY NW_ENG", conn);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    list.Add("~ALL~");

    while (dr.Read())
    {
        list.Add(dr["NW_ENG"].ToString());
    }

    conn.Close();


    return list;
}
//=====================================================
// POWER & AC ALARMS
//=====================================================

public List<AlarmModel> GetPowerAcAlarms()
{
    List<AlarmModel> alarms = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("VIEW_P_AC_2", conn);
    cmd.CommandType = CommandType.StoredProcedure;

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        AlarmModel model = new AlarmModel();

        model.ID = Convert.ToInt32(dr["ID"]);
        model.NODE = dr["NODE"].ToString();
        model.HOURS = Convert.ToInt32(dr["HOURS"]);
        model.FAULT = dr["FAULT"].ToString();
        model.NW_ENG = dr["NW_ENG"].ToString();
        model.ACK_STATUS = dr["ACK_STATUS"].ToString();
        model.ACK_BY = dr["ACK_BY"].ToString();

        alarms.Add(model);
    }

    conn.Close();

    return alarms;
}
//=====================================================
// ACK POWER & AC ALARM
//=====================================================

public void AckAlarm(int id, string user)
{
    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        UPDATE FAULTS
        SET ACK_STATUS='ACK',
            ACK_TIME=GETDATE(),
            ACK_BY=@user
        WHERE ID=@id
          AND ACK_STATUS='OPEN'", conn);

    cmd.Parameters.AddWithValue("@id", id);
    cmd.Parameters.AddWithValue("@user", user);

    conn.Open();

    cmd.ExecuteNonQuery();

    conn.Close();

}

//=====================================================
// AUTO ESCALATIONS
//=====================================================

public List<AutoEscalationModel> GetAutoEscalations(int faultId)
{
    List<AutoEscalationModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT DISTINCT
            ESCALATION_LEVEL,
            CLI,
            MESSAGE
        FROM ESCALATIONS_AUTO
        WHERE FAULT_ID=@FAULT_ID
        ORDER BY ESCALATION_LEVEL", conn);

    cmd.Parameters.AddWithValue("@FAULT_ID", faultId);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new AutoEscalationModel
        {
            ESCALATION_LEVEL = Convert.ToInt32(dr["ESCALATION_LEVEL"]),
            CLI = dr["CLI"].ToString() ?? "",
            MESSAGE = dr["MESSAGE"].ToString() ?? ""
        });
    }

    conn.Close();

    return list;
}

//=====================================================
// SMS ESCALATIONS
//=====================================================

public List<SmsEscalationModel> GetSmsEscalations(int faultId)
{
    List<SmsEscalationModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
        SELECT
            MOBILE,
            SEND_STATUS,
            SENT_TIME,
            SCHEDULE_TIME,
            ESCALATION_LEVEL,
            NAME,
            DESIGNATION,
            MESSAGE
        FROM ESCALATIONS_AUTO
        WHERE FAULT_ID=@FAULT_ID
        ORDER BY SCHEDULE_TIME, EMP_ID", conn);

    cmd.Parameters.AddWithValue("@FAULT_ID", faultId);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new SmsEscalationModel
        {
            ESCALATION_LEVEL = Convert.ToInt32(dr["ESCALATION_LEVEL"]),
            MOBILE = dr["MOBILE"].ToString() ?? "",
            NAME = dr["NAME"].ToString() ?? "",
            DESIGNATION = dr["DESIGNATION"].ToString() ?? "",
            SEND_STATUS = dr["SEND_STATUS"].ToString() ?? "",
            MESSAGE = dr["MESSAGE"].ToString() ?? "",

            SCHEDULE_TIME = dr["SCHEDULE_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["SCHEDULE_TIME"]),

            SENT_TIME = dr["SENT_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["SENT_TIME"])
        });
    }

    conn.Close();

    return list;
}
public List<CurrentDataFaultModel> GetCurrentDataFaults()
{
    List<CurrentDataFaultModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("REPORT1", conn);
    cmd.CommandType = CommandType.StoredProcedure;

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new CurrentDataFaultModel
        {
            SerialNo = Convert.ToInt32(dr["Serial No."]),
            DataCircuitID = dr["Data Circuit ID"].ToString(),
            DetailsOfCustomer = dr["Details Of Customer"].ToString(),
            ReportedTime = Convert.ToDateTime(dr["reportedtime"]),
            Date = dr["Date"].ToString(),
            Time = dr["Time"].ToString(),
            PendingHours = dr["Pending Hours"].ToString()
        });
    }

    conn.Close();

    return list;
}
public List<CurrentNetworkFaultModel> GetCurrentNetworkFaults()
{
    List<CurrentNetworkFaultModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("VIEW_CURRENT_NW_FAULTS", conn);
    cmd.CommandType = CommandType.StoredProcedure;

    conn.Open();

    SqlDataReader reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        list.Add(new CurrentNetworkFaultModel
        {
            Platform = reader["PLATFORM"].ToString() ?? "",
            Node = reader["NODE"].ToString() ?? "",
            ResponsibleOfficer = reader["RESPONSIBLE_OFFICER"].ToString() ?? "",
            Date = reader["DATE"].ToString() ?? "",
            FaultOccuredAt = reader["FAULT OCCURED AT"].ToString() ?? "",
            PendingHours = reader["PENDING HOURS"].ToString() ?? ""
        });
    }

    conn.Close();

    return list;
}
public List<CurrentAluLteModel> GetCurrentAluLte()
{
    List<CurrentAluLteModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("VIEW_CURRENT_ALU_LTE", conn);
    cmd.CommandType = CommandType.StoredProcedure;

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new CurrentAluLteModel
        {
            Node = dr["NODE"].ToString() ?? "",
            Fault = dr["FAULT"].ToString() ?? "",
            AlarmType = dr["ALARM_TYPE"].ToString() ?? "",
            FaultTime = Convert.ToDateTime(dr["FAULT_TIME"]),
            NwEng = dr["NW_ENG"].ToString() ?? "",
            Province = dr["PROVINCE"].ToString() ?? "",
            Region = dr["REGION"].ToString() ?? ""
        });
    }

    conn.Close();

    return list;
}
public List<HistoryAluLteModel> GetHistoryAluLte()
{
    List<HistoryAluLteModel> list = new();

    using SqlConnection conn = GetConnection();

   SqlCommand cmd = new SqlCommand(@"
SELECT TOP 100
    NODE,
    FAULT,
    ALARM_TYPE,
    FAULT_TIME,
    CLOSE_TIME,
    NW_ENG,
    PROVINCE,
    REGION
FROM HISTORY_FAULTS
WHERE PLATFORM='LTE'
AND VENDOR='NOKIA'
ORDER BY FAULT_TIME DESC", conn);

cmd.CommandType = CommandType.Text;
cmd.CommandTimeout = 120;

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new HistoryAluLteModel
        {
            Node = dr["NODE"].ToString(),
            Fault = dr["FAULT"].ToString(),
            AlarmType = dr["ALARM_TYPE"].ToString(),
            FaultTime = Convert.ToDateTime(dr["FAULT_TIME"]),
            CloseTime = dr["CLOSE_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["CLOSE_TIME"]),
            NwEng = dr["NW_ENG"].ToString(),
            Province = dr["PROVINCE"].ToString(),
            Region = dr["REGION"].ToString()
        });
    }

    conn.Close();

    return list;
}
public List<LteBatteryBackup> GetLteBatteryBackup()
{
    List<LteBatteryBackup> list = new();

    using SqlConnection conn = GetConnection();

    string sql = @"
    SELECT
        NODE,
        VENDOR,
        CASE
            WHEN MAX(DATEDIFF(SECOND, START_TIME, END_TIME)) > 900
             AND MAX(DATEDIFF(SECOND, START_TIME, END_TIME)) < 1800
            THEN 0.5
            ELSE MAX(DATEDIFF(HOUR, START_TIME, END_TIME))
        END AS BATTERY_BACKUP
    FROM BATTERY_BACKUP_2
    GROUP BY NODE, VENDOR
    ORDER BY NODE";

    SqlCommand cmd = new SqlCommand(sql, conn);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new LteBatteryBackup
        {
            NODE = dr["NODE"].ToString() ?? "",
            VENDOR = dr["VENDOR"].ToString() ?? "",
            BATTERY_BACKUP = Convert.ToDecimal(dr["BATTERY_BACKUP"])
        });
    }

    conn.Close();

    return list;
    
}

public List<MsanBatteryBackup> GetMsanBatteryBackup()
{
    List<MsanBatteryBackup> list = new();

    using SqlConnection conn = GetConnection();

    string sql = @"
SELECT
    BB.NODE,
    MAX(R.Region) AS Region,
    MAX(R.Province) AS Province,
    MAX(R.LEA_Code) AS LEA,
    MAX(R.NW_Designation) AS [NW Eng],
    MAX(NW.Name) AS [NW Eng Name],
    MAX(DATEDIFF(hour, BB.START_TIME, BB.END_TIME)) AS HOURS,
    MAX(DATEDIFF(minute, BB.START_TIME, BB.END_TIME)) AS MINUTES,
    MAX(ISNULL(msan_cx_count.cx,0)) AS [Customer Count]
FROM BATTERY_BACKUP_2 BB
LEFT JOIN msan_cx_count
    ON BB.NODE = msan_cx_count.node_name
LEFT JOIN NW_REGION_MAPPING R
    ON SUBSTRING(REPLACE(BB.NODE,'_','-'),1,
        CHARINDEX('-',REPLACE(BB.NODE,'_','-'))-1)=R.LEA_Code
LEFT JOIN NW_ENG NW
    ON R.NW_Designation = NW.position
GROUP BY BB.NODE
ORDER BY BB.NODE";

    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.CommandType = CommandType.Text;
    cmd.CommandTimeout = 120;

    conn.Open();

    SqlDataReader reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        MsanBatteryBackup model = new MsanBatteryBackup();

        model.NODE = reader["NODE"]?.ToString() ?? "";
        model.Region = reader["Region"]?.ToString() ?? "";
        model.Province = reader["Province"]?.ToString() ?? "";
        model.LEA = reader["LEA"]?.ToString() ?? "";
        model.NwEng = reader["NW Eng"]?.ToString() ?? "";
        model.NwEngName = reader["NW Eng Name"]?.ToString() ?? "";

        model.HOURS = reader["HOURS"] == DBNull.Value
            ? 0
            : Convert.ToInt32(reader["HOURS"]);

        model.MINUTES = reader["MINUTES"] == DBNull.Value
            ? 0
            : Convert.ToInt32(reader["MINUTES"]);

        model.CustomerCount = reader["Customer Count"] == DBNull.Value
            ? 0
            : Convert.ToInt32(reader["Customer Count"]);

        list.Add(model);
    }

    conn.Close();

    return list;
}

public List<GeneratorModel> GetGeneratorRunningTime(
    DateTime from,
    DateTime to,
    string nwEng)
{
    List<GeneratorModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("GENERATOR", conn);

    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@from", from);
    cmd.Parameters.AddWithValue("@to", to);
    cmd.Parameters.AddWithValue("@nw_eng", nwEng);

    conn.Open();

    SqlDataReader reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        list.Add(new GeneratorModel
        {
            Location = reader["Location"].ToString() ?? "",

            RunningTimeHours =
                reader["Running Time (Hours)"] == DBNull.Value
                    ? 0
                    : Convert.ToInt32(reader["Running Time (Hours)"])
        });
    }

    conn.Close();

    return list;
}

public List<string> GetGeneratorEngineList()
{
    List<string> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
SELECT '~ALL~' AS NW_ENG
UNION
SELECT DISTINCT NW_ENG
FROM HISTORY_FAULTS
WHERE NW_ENG IS NOT NULL
ORDER BY NW_ENG", conn);

    conn.Open();

    SqlDataReader reader = cmd.ExecuteReader();

    while (reader.Read())
    {
        list.Add(reader["NW_ENG"].ToString() ?? "");
    }

    conn.Close();

    return list;
}

public List<WifiFaultOssModel> GetWifiFaultsOss()
{
    List<WifiFaultOssModel> list = new();

    using SqlConnection conn = GetConnection();

    string sql = @"
    SELECT
        clarity_ref AS REF,
        node AS AP,
        work_group AS WORKGROUP,
        MODEL,
        fault_time AS FAULT_TIME
    FROM Faults_OSS
    WHERE node LIKE '%-WP-%'
    ORDER BY fault_time DESC";

    SqlCommand cmd = new SqlCommand(sql, conn);
    cmd.CommandType = CommandType.Text;
    cmd.CommandTimeout = 120;

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new WifiFaultOssModel
        {
            REF = dr["REF"]?.ToString() ?? "",
            AP = dr["AP"]?.ToString() ?? "",
            WORKGROUP = dr["WORKGROUP"]?.ToString() ?? "",
            MODEL = dr["MODEL"]?.ToString() ?? "",
            FAULT_TIME = dr["FAULT_TIME"] == DBNull.Value
                ? null
                : Convert.ToDateTime(dr["FAULT_TIME"])
        });
    }

    conn.Close();

    return list;
}
public List<OutageReasonModel> GetOutageReasons(string platform, string nwEng)
{
    List<OutageReasonModel> list = new();

    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand("REPORT_3", conn);
    cmd.CommandType = CommandType.StoredProcedure;

    cmd.Parameters.AddWithValue("@platform", platform);
    cmd.Parameters.AddWithValue("@nw_eng", nwEng);

    conn.Open();

    SqlDataReader dr = cmd.ExecuteReader();

    while (dr.Read())
    {
        list.Add(new OutageReasonModel
        {
            ID = Convert.ToInt32(dr["ID"]),
            NODE = dr["NODE"].ToString() ?? "",
            PLATFORM = dr["PLATFORM"].ToString() ?? "",
            FAULT_TIME = Convert.ToDateTime(dr["FAULT_TIME"]),
            HOURS = Convert.ToInt32(dr["HOURS"]),
            FAULT = dr["FAULT"].ToString() ?? "",
            NW_ENG = dr["NW_ENG"].ToString() ?? "",
            TAG = dr["TAG"]?.ToString() ?? ""
        });
    }

    conn.Close();

    return list;
}

public void UpdateTag(int id, string tag)
{
    using SqlConnection conn = GetConnection();

    SqlCommand cmd = new SqlCommand(@"
UPDATE HISTORY_FAULTS
SET TAG=@TAG
WHERE ID=@ID", conn);

    cmd.Parameters.AddWithValue("@TAG", tag);
    cmd.Parameters.AddWithValue("@ID", id);

    conn.Open();

    cmd.ExecuteNonQuery();

    conn.Close();
}
    }
}