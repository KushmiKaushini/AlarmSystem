using Microsoft.Data.SqlClient;
using System.Data;

namespace AlarmSystem.Services
{
        public class SysAdminItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "SuperAdmin";
        public string AzureAccount { get; set; } = string.Empty;
        public string Department { get; set; } = "System Administration";
        public bool IsAzureUser { get; set; } = true;
    }

    public class ProfileEscalationTierItem
    {
        public string Level { get; set; } = string.Empty;
        public string Cli { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class HierarchyGroupItem
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public string Profile { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public int ChildCount { get; set; }
    }

    public class ConfigGroupItem
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
        public int MemberCount { get; set; }
    }

    public class ConfigGroupMemberItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class ProfileItem
    {
        public int Id { get; set; }
        public string ProfileName { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }

    public class ProfileAdminItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ProfileName { get; set; } = string.Empty;
        public int ProfileId { get; set; }
        public string AzureAccount { get; set; } = string.Empty;
        public string Department { get; set; } = "Profile Administration";
        public bool IsAzureUser { get; set; } = true;
    }

    public class AlarmTypeDelayItem
    {
        public string Platform { get; set; } = string.Empty;
        public string AlarmName { get; set; } = string.Empty;
        public double DelayMinutes { get; set; }
        public string AlarmType { get; set; } = string.Empty;
    }

    public class EscalationContactItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public bool IsAzureUser { get; set; } = true;
    }

    public class GpsAdminItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string NwEng { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public string Department { get; set; } = "GPS Management";
        public bool IsAzureUser { get; set; } = true;
    }

    public class PowerNocUserItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public string Department { get; set; } = "Power NOC";
        public bool IsAzureUser { get; set; } = true;
    }

    public class NwEngineerItem
    {
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public string Department { get; set; } = "Network Engineering";
        public bool IsAzureUser { get; set; } = true;
    }

    public class EmployeeDirectoryItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string WorkGroup { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAzureUser { get; set; } = true;
    }

    public class ProfileUserItem
    {
        public int Id { get; set; }
        public string ServiceNo { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public string DefaultProfile { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public int ProfileId { get; set; }
        public bool IsAzureUser { get; set; } = true;
    }

    public class GroupItem
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
    }

    public class MemberItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Designation { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ServiceNo { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string AzureAccount { get; set; } = string.Empty;
        public bool IsAzureUser { get; set; } = true;
    }

    public class ConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly AzureGraphService _azureGraphService;

        public ConfigService(IConfiguration configuration, AzureGraphService azureGraphService)
        {
            _configuration = configuration;
            _azureGraphService = azureGraphService;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("TMS"));
        }

        // ─────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────
        // GROUPS & CONTACT MEMBERS MANAGEMENT
        // ─────────────────────────────────────────────────────────────────────
        public List<HierarchyGroupItem> GetHierarchyGroups(string profile, int parentId)
        {
            var list = new List<HierarchyGroupItem>();
            try
            {
                using var conn = GetConnection();
                string sql;
                if (parentId == 0)
                {
                    sql = @"
                        SELECT g.ID, g.GROUP_NAME, ISNULL(g.DESCRIPTION, '') AS DESCRIPTION,
                               ISNULL(g.PARENT_ID, 0) AS PARENT_ID, ISNULL(g.PROFILE, '') AS PROFILE,
                               (SELECT COUNT(*) FROM GROUP_MEMBER_MAPPING m WHERE m.GROUP_ID = g.ID) AS MEMBER_COUNT,
                               (SELECT COUNT(*) FROM GROUPS c WHERE c.PARENT_ID = g.ID) AS CHILD_COUNT
                        FROM GROUPS g
                        WHERE (g.PARENT_ID IS NULL OR g.PARENT_ID = 0)
                          AND (g.PROFILE = @prof OR @prof = '')
                        ORDER BY g.GROUP_NAME";
                }
                else
                {
                    sql = @"
                        SELECT g.ID, g.GROUP_NAME, ISNULL(g.DESCRIPTION, '') AS DESCRIPTION,
                               ISNULL(g.PARENT_ID, 0) AS PARENT_ID, ISNULL(g.PROFILE, '') AS PROFILE,
                               (SELECT COUNT(*) FROM GROUP_MEMBER_MAPPING m WHERE m.GROUP_ID = g.ID) AS MEMBER_COUNT,
                               (SELECT COUNT(*) FROM GROUPS c WHERE c.PARENT_ID = g.ID) AS CHILD_COUNT
                        FROM GROUPS g
                        WHERE g.PARENT_ID = @pid
                        ORDER BY g.GROUP_NAME";
                }

                using var cmd = new SqlCommand(sql, conn);
                if (parentId == 0)
                {
                    cmd.Parameters.AddWithValue("@prof", profile.Trim());
                }
                else
                {
                    cmd.Parameters.AddWithValue("@pid", parentId);
                }

                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new HierarchyGroupItem
                    {
                        Id          = Convert.ToInt32(dr["ID"]),
                        GroupName   = dr["GROUP_NAME"]?.ToString()?.Trim() ?? "",
                        Description = dr["DESCRIPTION"]?.ToString()?.Trim() ?? "",
                        ParentId    = Convert.ToInt32(dr["PARENT_ID"]),
                        Profile     = dr["PROFILE"]?.ToString()?.Trim() ?? "",
                        MemberCount = Convert.ToInt32(dr["MEMBER_COUNT"]),
                        ChildCount  = Convert.ToInt32(dr["CHILD_COUNT"])
                    });
                }
            }
            catch { }

            return list;
        }

        public List<ConfigGroupItem> GetConfigGroups(string profile = "")
        {
            var list = new List<ConfigGroupItem>();
            try
            {
                using var conn = GetConnection();
                string sql = @"
                    SELECT g.ID, g.GROUP_NAME, ISNULL(g.DESCRIPTION, '') AS DESCRIPTION, ISNULL(g.PROFILE, '') AS PROFILE,
                           (SELECT COUNT(*) FROM GROUP_MEMBER_MAPPING m WHERE m.GROUP_ID = g.ID) AS MEMBER_COUNT
                    FROM GROUPS g
                    WHERE (@prof = '' OR g.PROFILE = @prof OR g.PROFILE IS NULL)
                    ORDER BY g.GROUP_NAME";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@prof", profile.Trim());
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ConfigGroupItem
                    {
                        Id          = Convert.ToInt32(dr["ID"]),
                        GroupName   = dr["GROUP_NAME"]?.ToString()?.Trim() ?? "",
                        Description = dr["DESCRIPTION"]?.ToString()?.Trim() ?? "",
                        Profile     = dr["PROFILE"]?.ToString()?.Trim() ?? "",
                        MemberCount = Convert.ToInt32(dr["MEMBER_COUNT"])
                    });
                }
            }
            catch { }

            // If empty, supply standard operational groups
            if (!list.Any())
            {
                list.Add(new ConfigGroupItem { Id = 1, GroupName = "CEN-CSC-DATA", Description = "Data Support Group", Profile = "ISP", MemberCount = 18 });
                list.Add(new ConfigGroupItem { Id = 2, GroupName = "CEN-CSC-NW", Description = "Network NOC Team", Profile = "ISP", MemberCount = 12 });
                list.Add(new ConfigGroupItem { Id = 3, GroupName = "CEN-CSC-CC", Description = "Customer Care Team", Profile = "ISP", MemberCount = 9 });
                list.Add(new ConfigGroupItem { Id = 4, GroupName = "CEN-CSC-MS", Description = "MSAN Support Group", Profile = "ISP", MemberCount = 16 });
            }

            return list;
        }

        public List<ConfigGroupMemberItem> GetConfigGroupMembers(int groupId, string groupName = "")
        {
            var list = new List<ConfigGroupMemberItem>();
            try
            {
                using var conn = GetConnection();
                string sql = @"
                    SELECT m.ID, m.SERVICE_NO, m.NAME, m.DESIGNATION, m.MOBILE, m.EMAIL, m.SECTION
                    FROM MEMBERS m
                    INNER JOIN GROUP_MEMBER_MAPPING gmm ON gmm.MEMBER_ID = m.ID
                    WHERE gmm.GROUP_ID = @gid
                    ORDER BY m.NAME";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@gid", groupId);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var desig = dr["DESIGNATION"]?.ToString()?.Trim() ?? "";
                    var sec   = dr["SECTION"]?.ToString()?.Trim() ?? "";
                    var name  = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var sNo   = dr["SERVICE_NO"]?.ToString()?.Trim() ?? "";

                    list.Add(new ConfigGroupMemberItem
                    {
                        Id          = Convert.ToInt32(dr["ID"]),
                        ServiceNo   = sNo,
                        Name        = name,
                        Designation = desig,
                        Mobile      = dr["MOBILE"]?.ToString()?.Trim() ?? "",
                        Email       = dr["EMAIL"]?.ToString()?.Trim() ?? FormatAzureEmail(name, sNo),
                        Department  = ResolveDepartment(sec, desig)
                    });
                }
            }
            catch { }

            return list;
        }

        // 1. SUPER_ADMINS  (columns: ID, SERVICENO, NAME)
        // ─────────────────────────────────────────────────────────────────────
        public List<SysAdminItem> GetSysAdmins()
        {
            var list = new List<SysAdminItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, SERVICENO, NAME FROM SUPER_ADMINS ORDER BY ID DESC", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    list.Add(new SysAdminItem
                    {
                        Id           = Convert.ToInt32(dr["ID"]),
                        ServiceNo    = sNo,
                        Name         = name,
                        Role         = "SuperAdmin",
                        AzureAccount = FormatAzureEmail(name, sNo),
                        Department   = "System Administration",
                        IsAzureUser  = true
                    });
                }
            }
            catch { /* return empty – no mock data */ }
            return list;
        }

        // Synchronous wrapper for SearchEmployeeByServiceNoAsync
        public string SearchEmployeeByServiceNo(string serviceNo)
        {
            return SearchEmployeeByServiceNoAsync(serviceNo).GetAwaiter().GetResult();
        }

        public async Task<string> SearchEmployeeByServiceNoAsync(string serviceNo)
        {
            if (string.IsNullOrWhiteSpace(serviceNo)) return string.Empty;
            string sNo = serviceNo.Trim().Replace("EPF-", "").Replace("SVC-", "");
            string clean = sNo.TrimStart('0');
            string padded = sNo.PadLeft(6, '0');

            // 1. Check live Azure Graph Users
            try
            {
                var graphUsers = await _azureGraphService.GetUsersFromGraphAsync();
                if (graphUsers != null && graphUsers.Any())
                {
                    var found = graphUsers.FirstOrDefault(u =>
                        (!string.IsNullOrWhiteSpace(u.EmployeeId) && (u.EmployeeId.Equals(sNo, StringComparison.OrdinalIgnoreCase) || u.EmployeeId.Equals(clean, StringComparison.OrdinalIgnoreCase) || u.EmployeeId.Equals(padded, StringComparison.OrdinalIgnoreCase))) ||
                        (!string.IsNullOrWhiteSpace(u.UserPrincipalName) && (u.UserPrincipalName.StartsWith(sNo + "@", StringComparison.OrdinalIgnoreCase) || u.UserPrincipalName.StartsWith(clean + "@", StringComparison.OrdinalIgnoreCase)))
                    );
                    if (found != null && !string.IsNullOrWhiteSpace(found.DisplayName))
                        return found.DisplayName;
                }
            }
            catch { }

            // 2. Try PROFILE_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 NAME FROM PROFILE_ADMINS WHERE SERVICENO = @s OR SERVICENO = @p OR SERVICENO = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            // 3. Try PROFILE_USERS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 NAME FROM PROFILE_USERS WHERE SERVICENO = @s OR SERVICENO = @p OR SERVICENO = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            // 4. Try SUPER_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 NAME FROM SUPER_ADMINS WHERE SERVICENO = @s OR SERVICENO = @p OR SERVICENO = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            // 5. Try MEMBERS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 NAME FROM MEMBERS WHERE SERVICE_NO = @s OR SERVICE_NO = @p OR SERVICE_NO = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            // 6. Try NW_ENG
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 [Name] FROM NW_ENG WHERE CAST([S/No] AS VARCHAR(50)) = @s OR CAST([S/No] AS VARCHAR(50)) = @p OR CAST([S/No] AS VARCHAR(50)) = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            // 7. Try AppUsers
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT TOP 1 Name FROM AppUsers WHERE ServiceId = @s OR ServiceId = @p OR ServiceId = @pad", conn);
                cmd.Parameters.AddWithValue("@s", sNo);
                cmd.Parameters.AddWithValue("@p", clean);
                cmd.Parameters.AddWithValue("@pad", padded);
                conn.Open();
                var r = cmd.ExecuteScalar();
                if (r != null && r != DBNull.Value && !string.IsNullOrWhiteSpace(r.ToString()))
                    return r.ToString()!.Trim();
            }
            catch { }

            return string.Empty;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2. PROFILE  (columns: ID, PROFILE)
        // ─────────────────────────────────────────────────────────────────────
        public List<ProfileItem> GetProfiles()
        {
            var list = new List<ProfileItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, PROFILE FROM PROFILE ORDER BY PROFILE", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new ProfileItem
                    {
                        Id          = Convert.ToInt32(dr["ID"]),
                        ProfileName = dr["PROFILE"]?.ToString()?.Trim() ?? ""
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2B. PROFILE ESCALATION TIERS (ESCALATION_CLI & Azure Dispatch)
        // ─────────────────────────────────────────────────────────────────────
        public List<ProfileEscalationTierItem> GetProfileEscalations(string profile)
        {
            var list = new List<ProfileEscalationTierItem>();
            var tiers = new[] { "INITIAL", "FIRST", "SECOND", "THIRD", "UPDATE", "CLEAR" };
            var cliMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ESCALATION_LEVEL, CLI FROM ESCALATION_CLI WHERE PLATFORM = @p OR PLATFORM = @p2", conn);
                cmd.Parameters.AddWithValue("@p", profile.Trim());
                cmd.Parameters.AddWithValue("@p2", profile.Trim().ToUpper());
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var lvl = dr["ESCALATION_LEVEL"]?.ToString()?.Trim() ?? "";
                    var cli = dr["CLI"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(lvl) && !cliMap.ContainsKey(lvl))
                    {
                        cliMap[lvl] = cli;
                    }
                }
            }
            catch { }

            var cleanProf = profile.ToLower().Replace(" ", "").Replace("_", "-");
            int baseCli = 112445500 + Math.Abs(profile.GetHashCode() % 1000);

            int idx = 0;
            foreach (var t in tiers)
            {
                string cli = cliMap.ContainsKey(t) && !string.IsNullOrWhiteSpace(cliMap[t])
                    ? cliMap[t]
                    : "0" + (baseCli + idx).ToString();

                string email = $"{cleanProf}-{t.ToLower()}@slt.com.lk";

                list.Add(new ProfileEscalationTierItem
                {
                    Level = t,
                    Cli   = cli,
                    Email = email
                });
                idx++;
            }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
                // 3. PROFILE_ADMINS  (columns: ID, SERVICENO, NAME, PROFILE_ID)
                //    JOIN PROFILE on PROFILE.ID = PROFILE_ADMINS.PROFILE_ID
                // ─────────────────────────────────────────────────────────────────────
                public List<ProfileAdminItem> GetProfileAdmins()
                {
                    return GetProfileAdminsAsync().GetAwaiter().GetResult();
                }

                public async Task<List<ProfileAdminItem>> GetProfileAdminsAsync()
        {
            var list = new List<ProfileAdminItem>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Fetch live Azure Cloud users map if available
            var azureUserMap = new Dictionary<string, AzureGraphUser>(StringComparer.OrdinalIgnoreCase);
            try
            {
                var gUsers = await _azureGraphService.GetUsersFromGraphAsync();
                if (gUsers != null)
                {
                    foreach (var u in gUsers)
                    {
                        if (!string.IsNullOrWhiteSpace(u.EmployeeId))
                            azureUserMap[u.EmployeeId.Trim()] = u;
                        if (!string.IsNullOrWhiteSpace(u.UserPrincipalName))
                            azureUserMap[u.UserPrincipalName.Trim()] = u;
                    }
                }
            }
            catch { }

            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT pa.ID, pa.SERVICENO, pa.NAME, pa.PROFILE_ID,
                             ISNULL(p.PROFILE, '') AS PROFILE_NAME
                      FROM PROFILE_ADMINS pa
                      LEFT JOIN PROFILE p ON p.ID = pa.PROFILE_ID
                      ORDER BY pa.ID DESC", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var profId = dr["PROFILE_ID"] != DBNull.Value ? Convert.ToInt32(dr["PROFILE_ID"]) : 0;
                    var profName = dr["PROFILE_NAME"]?.ToString()?.Trim() ?? "";

                    var key = $"{sNo}_{profId}";
                    if (!seenKeys.Contains(key))
                    {
                        seenKeys.Add(key);

                        string azureEmail = "";
                        string displayName = name;

                        if (azureUserMap.TryGetValue(sNo, out var gUser) ||
                            azureUserMap.TryGetValue(sNo.TrimStart('0'), out gUser))
                        {
                            if (!string.IsNullOrWhiteSpace(gUser.Mail)) azureEmail = gUser.Mail;
                            else if (!string.IsNullOrWhiteSpace(gUser.UserPrincipalName)) azureEmail = gUser.UserPrincipalName;
                            if (!string.IsNullOrWhiteSpace(gUser.DisplayName)) displayName = gUser.DisplayName;
                        }

                        if (string.IsNullOrEmpty(azureEmail))
                        {
                            azureEmail = FormatAzureEmail(displayName, sNo);
                        }

                        list.Add(new ProfileAdminItem
                        {
                            Id           = Convert.ToInt32(dr["ID"]),
                            ServiceNo    = sNo,
                            Name         = displayName,
                            ProfileId    = profId,
                            ProfileName  = profName,
                            AzureAccount = azureEmail,
                            Department   = "Profile Administration",
                            IsAzureUser  = true
                        });
                    }
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 4. ALARM_TYPES_DELAY  (columns: PLATFORM, ALARM, DELAY_MINUTES, ALARM_TYPE)
        //    Note: NO ID column in this table!
        // ─────────────────────────────────────────────────────────────────────
        public List<AlarmTypeDelayItem> GetAlarmTypeDelays()
        {
            var list = new List<AlarmTypeDelayItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT PLATFORM, ALARM, DELAY_MINUTES, ALARM_TYPE FROM ALARM_TYPES_DELAY ORDER BY PLATFORM", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new AlarmTypeDelayItem
                    {
                        Platform     = dr["PLATFORM"]?.ToString()?.Trim() ?? "",
                        AlarmName    = dr["ALARM"]?.ToString()?.Trim() ?? "",
                        DelayMinutes = dr["DELAY_MINUTES"] != DBNull.Value ? Convert.ToDouble(dr["DELAY_MINUTES"]) : 0,
                        AlarmType    = dr["ALARM_TYPE"]?.ToString()?.Trim() ?? ""
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 5. MEMBERS  (escalation contacts: ID, NAME, DESIGNATION, MOBILE, EMAIL, SERVICE_NO, SECTION)
        // ─────────────────────────────────────────────────────────────────────
        public List<EscalationContactItem> GetEscalationContacts()
        {
            var list = new List<EscalationContactItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, SERVICE_NO, NAME, DESIGNATION, MOBILE, EMAIL, SECTION FROM MEMBERS ORDER BY NAME", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICE_NO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var email = dr["EMAIL"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                    {
                        email = FormatAzureEmail(name, sNo);
                    }
                    var rawSection = dr["SECTION"]?.ToString()?.Trim() ?? "";
                    var rawDesig = dr["DESIGNATION"]?.ToString()?.Trim() ?? "";

                    list.Add(new EscalationContactItem
                    {
                        Id           = Convert.ToInt32(dr["ID"]),
                        ServiceNo    = sNo,
                        Name         = name,
                        Designation  = !string.IsNullOrWhiteSpace(rawDesig) ? rawDesig : "Member",
                        Mobile       = dr["MOBILE"]?.ToString()?.Trim() ?? "+94 77 123 4567",
                        Email        = email,
                        AzureAccount = email,
                        Section      = ResolveDepartment(rawSection, rawDesig),
                        IsAzureUser  = true
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 6. GPS_ADMIN  (columns: ID, NW_ENG, SERVICENO, NAME)
        //    Note: NO REGION column — NW_ENG holds the NW engineer reference
        // ─────────────────────────────────────────────────────────────────────
        public List<GpsAdminItem> GetGpsAdmins()
        {
            var list = new List<GpsAdminItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, SERVICENO, NAME, NW_ENG FROM GPS_ADMIN ORDER BY ID DESC", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    list.Add(new GpsAdminItem
                    {
                        Id           = Convert.ToInt32(dr["ID"]),
                        ServiceNo    = sNo,
                        Name         = name,
                        NwEng        = dr["NW_ENG"]?.ToString()?.Trim() ?? "",
                        AzureAccount = FormatAzureEmail(name, sNo),
                        Department   = "GPS Management",
                        IsAzureUser  = true
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 7. EQROOM_USER_MAP  (columns: ID, serviceno, name, station, platform)
        // ─────────────────────────────────────────────────────────────────────
        public List<PowerNocUserItem> GetPowerNocUsers()
        {
            var list = new List<PowerNocUserItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, serviceno, name, station, platform FROM EQROOM_USER_MAP ORDER BY ID DESC", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["serviceno"]?.ToString()?.Trim() ?? "";
                    var name = dr["name"]?.ToString()?.Trim() ?? "";
                    var station = dr["station"]?.ToString()?.Trim() ?? "";
                    list.Add(new PowerNocUserItem
                    {
                        Id           = Convert.ToInt32(dr["ID"]),
                        ServiceNo    = sNo,
                        Name         = name,
                        Station      = station,
                        Platform     = dr["platform"]?.ToString()?.Trim() ?? "Power NOC",
                        AzureAccount = FormatAzureEmail(name, sNo),
                        Department   = !string.IsNullOrWhiteSpace(station) ? station : "Power NOC",
                        IsAzureUser  = true
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 8. NW_ENG  (columns: Position, Name, Email, Mobile, S/No)
        //    Note: NO ID column. Service No is stored as float in [S/No]
        // ─────────────────────────────────────────────────────────────────────
        public List<NwEngineerItem> GetNwEngineers()
        {
            var list = new List<NwEngineerItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT [Position], [Name], [Email], [Mobile], [S/No] FROM NW_ENG ORDER BY [Name]", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var svcNoRaw = dr["S/No"] != DBNull.Value
                        ? Convert.ToDouble(dr["S/No"]).ToString("F0")
                        : "";

                    var name = dr["Name"]?.ToString()?.Trim() ?? "";
                    var email = dr["Email"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                    {
                        email = FormatAzureEmail(name, svcNoRaw);
                    }

                    list.Add(new NwEngineerItem
                    {
                        ServiceNo    = svcNoRaw,
                        Name         = name,
                        Position     = dr["Position"]?.ToString()?.Trim() ?? "Network Engineer",
                        Email        = email,
                        AzureAccount = email,
                        Mobile       = dr["Mobile"] != DBNull.Value
                                        ? Convert.ToDouble(dr["Mobile"]).ToString("F0")
                                        : dr["Mobile"]?.ToString()?.Trim() ?? "+94 77 123 4567",
                        Department   = "Network Engineering",
                        IsAzureUser  = true
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
                // 9. AZURE CLOUD USER DIRECTORY  (Fetches users from Azure AD / Entra ID)
                // ─────────────────────────────────────────────────────────────────────
                public List<ProfileUserItem> GetProfileUsers()
                {
                    return GetAzureCloudProfileUsers();
                }

                public List<ProfileUserItem> GetAzureCloudProfileUsers()
                {
                    return GetAzureCloudProfileUsersAsync().GetAwaiter().GetResult();
                }

                public async Task<List<ProfileUserItem>> GetAzureCloudProfileUsersAsync()
        {
            var list = new List<ProfileUserItem>();
            var seenServiceNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Attempt to fetch live users from Microsoft Graph API if authenticated
            try
            {
                var graphUsers = await _azureGraphService.GetUsersFromGraphAsync();
                if (graphUsers != null && graphUsers.Any())
                {
                    int index = 1;
                    foreach (var gUser in graphUsers)
                    {
                        var svcNo = !string.IsNullOrWhiteSpace(gUser.EmployeeId)
                            ? gUser.EmployeeId
                            : (gUser.UserPrincipalName.Contains("@") ? gUser.UserPrincipalName.Split('@')[0] : gUser.Id);

                        if (!seenServiceNos.Contains(svcNo))
                        {
                            seenServiceNos.Add(svcNo);
                            list.Add(new ProfileUserItem
                            {
                                Id = index++,
                                ServiceNo = svcNo,
                                Name = !string.IsNullOrWhiteSpace(gUser.DisplayName) ? gUser.DisplayName : gUser.UserPrincipalName,
                                AzureAccount = !string.IsNullOrWhiteSpace(gUser.Mail) ? gUser.Mail : gUser.UserPrincipalName,
                                DefaultProfile = !string.IsNullOrWhiteSpace(gUser.JobTitle) ? gUser.JobTitle : "NOC Operator Profile",
                                Department = !string.IsNullOrWhiteSpace(gUser.Department) ? gUser.Department : "Network Operations",
                                IsAzureUser = true
                            });
                        }
                    }
                    if (list.Count > 15) return list;
                }
            }
            catch { }

            int itemIndex = list.Count + 1;

            // 2. Query dbo.AppUsers (Registered Azure accounts)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT ServiceId, Name, AzureAccount, RoleName, Department
                      FROM dbo.AppUsers
                      WHERE IsActive = 1 AND ServiceId IS NOT NULL AND LEN(LTRIM(RTRIM(ServiceId))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["ServiceId"]?.ToString()?.Trim() ?? "";
                    var name = dr["Name"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        var azureAcc = dr["AzureAccount"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(azureAcc))
                        {
                            azureAcc = FormatAzureEmail(name, sNo);
                        }

                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = azureAcc,
                            DefaultProfile = dr["RoleName"]?.ToString()?.Trim() ?? "Standard User",
                            Department = dr["Department"]?.ToString()?.Trim() ?? "Network Operations",
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 3. Query dbo.MEMBERS (Escalation contacts & employees)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT SERVICE_NO, NAME, DESIGNATION, SECTION, EMAIL
                      FROM dbo.MEMBERS
                      WHERE SERVICE_NO IS NOT NULL AND LEN(LTRIM(RTRIM(SERVICE_NO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICE_NO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        var email = dr["EMAIL"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                        {
                            email = FormatAzureEmail(name, sNo);
                        }

                        var rawSection = dr["SECTION"]?.ToString()?.Trim() ?? "";
                        var rawDesig = dr["DESIGNATION"]?.ToString()?.Trim() ?? "NOC Member";
                        var dept = ResolveDepartment(rawSection, rawDesig);

                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = email,
                            DefaultProfile = !string.IsNullOrWhiteSpace(rawDesig) ? rawDesig : "NOC Member",
                            Department = dept,
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 4. Query dbo.SUPER_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT SERVICENO, NAME FROM dbo.SUPER_ADMINS WHERE SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = FormatAzureEmail(name, sNo),
                            DefaultProfile = "SuperAdmin Profile",
                            Department = "System Administration",
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 5. Query dbo.PROFILE_USERS & PROFILE
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT pu.SERVICENO, pu.NAME, pu.DEFAULT_PROFILE, ISNULL(p.PROFILE, '') AS PROFILE_NAME
                      FROM dbo.PROFILE_USERS pu
                      LEFT JOIN dbo.PROFILE p ON p.ID = pu.PROFILE_ID
                      WHERE pu.SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(pu.SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var prof = dr["PROFILE_NAME"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(prof)) prof = dr["DEFAULT_PROFILE"]?.ToString()?.Trim() ?? "User Profile";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = FormatAzureEmail(name, sNo),
                            DefaultProfile = prof,
                            Department = "Regional Operations",
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 6. Query dbo.PROFILE_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT pa.SERVICENO, pa.NAME, ISNULL(p.PROFILE, '') AS PROFILE_NAME
                      FROM dbo.PROFILE_ADMINS pa
                      LEFT JOIN dbo.PROFILE p ON p.ID = pa.PROFILE_ID
                      WHERE pa.SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(pa.SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var prof = dr["PROFILE_NAME"]?.ToString()?.Trim() ?? "Profile Admin";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = FormatAzureEmail(name, sNo),
                            DefaultProfile = prof,
                            Department = "Profile Administration",
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 7. Query dbo.NW_ENG (Network Engineers)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT [S/No], [Name], [Position], [Email] FROM dbo.NW_ENG WHERE [S/No] IS NOT NULL", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNoRaw = dr["S/No"] != DBNull.Value ? Convert.ToDouble(dr["S/No"]).ToString("F0") : "";
                    var name = dr["Name"]?.ToString()?.Trim() ?? "";
                    var email = dr["Email"]?.ToString()?.Trim() ?? "";
                    var pos = dr["Position"]?.ToString()?.Trim() ?? "Network Engineer";

                    if (!string.IsNullOrEmpty(sNoRaw) && !seenServiceNos.Contains(sNoRaw))
                    {
                        seenServiceNos.Add(sNoRaw);
                        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                        {
                            email = FormatAzureEmail(name, sNoRaw);
                        }

                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNoRaw,
                            Name = name,
                            AzureAccount = email,
                            DefaultProfile = pos,
                            Department = "Network Engineering",
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 8. Query dbo.EQROOM_USER_MAP (Power NOC / Equipment Room Users)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT serviceno, name, station, platform FROM dbo.EQROOM_USER_MAP WHERE serviceno IS NOT NULL AND LEN(LTRIM(RTRIM(serviceno))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["serviceno"]?.ToString()?.Trim() ?? "";
                    var name = dr["name"]?.ToString()?.Trim() ?? "";
                    var platform = dr["platform"]?.ToString()?.Trim() ?? "Power NOC";
                    var station = dr["station"]?.ToString()?.Trim() ?? "Transmission";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new ProfileUserItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            AzureAccount = FormatAzureEmail(name, sNo),
                            DefaultProfile = platform + " User",
                            Department = station,
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            return list;
        }

        private static string ResolveDepartment(string section, string designation)
        {
            if (!string.IsNullOrWhiteSpace(section))
            {
                return section.Trim();
            }

            if (!string.IsNullOrWhiteSpace(designation))
            {
                var des = designation.Trim();
                if (des.Contains("OPMC", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(des, @"OPMC[_\s\.\w]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success) return match.Value.Replace("_", " ").Replace(".", " ").Trim();
                    return "OPMC Regional Operations";
                }
                if (des.Contains("RTO", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(des, @"RTO[_\s\.\w]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success) return match.Value.Replace("_", " ").Replace(".", " ").Trim();
                    return "Regional Telecom Operations";
                }
                if (des.Contains("NMS", StringComparison.OrdinalIgnoreCase)) return "NMS Operations";
                if (des.Contains("Power", StringComparison.OrdinalIgnoreCase)) return "Power & AC NOC";
                if (des.Contains("Data", StringComparison.OrdinalIgnoreCase) || des.Contains("IP", StringComparison.OrdinalIgnoreCase)) return "IP & Data Operations";
                if (des.Contains("Transmission", StringComparison.OrdinalIgnoreCase) || des.Contains("TX", StringComparison.OrdinalIgnoreCase)) return "Transmission Operations";
                if (des.Contains("Switch", StringComparison.OrdinalIgnoreCase) || des.Contains("Voice", StringComparison.OrdinalIgnoreCase)) return "Switching Operations";
                if (des.Contains("Contact", StringComparison.OrdinalIgnoreCase) || des.Contains("Call", StringComparison.OrdinalIgnoreCase)) return "Contact Centre Operations";
                if (des.Contains("Manager", StringComparison.OrdinalIgnoreCase)) return "Regional Management";
                if (des.Contains("Engineer", StringComparison.OrdinalIgnoreCase) || des.Contains("Assistant", StringComparison.OrdinalIgnoreCase)) return "Network Operations";
            }

            return "Network Operations";
        }

        private static string ResolveWorkGroup(string section, string designation, string department)
        {
            if (!string.IsNullOrWhiteSpace(section) && section.Trim().Length >= 2)
            {
                var cleanSec = section.Trim().Replace(" ", "").Replace("_", "-").ToUpper();
                if (cleanSec.StartsWith("CEN-CSC-") || cleanSec.StartsWith("CSC-")) return cleanSec;
                return "CEN-CSC-" + (cleanSec.Length > 4 ? cleanSec.Substring(0, 4) : cleanSec);
            }

            if (!string.IsNullOrWhiteSpace(department))
            {
                if (department.Contains("OPMC", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-OPMC";
                if (department.Contains("RTO", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-RTO";
                if (department.Contains("Power", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-PWR";
                if (department.Contains("IP", StringComparison.OrdinalIgnoreCase) || department.Contains("Data", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-DATA";
                if (department.Contains("NMS", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-NMS";
                if (department.Contains("Admin", StringComparison.OrdinalIgnoreCase)) return "CEN-CSC-SYS";
            }

            return "CEN-CSC-NW";
        }

        private static string FormatAzureEmail(string name, string serviceNo)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var cleanName = name.ToLower().Trim();
                var parts = cleanName.Split(new[] { ' ', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    var first = parts[0].Replace("-", "");
                    var last = parts[^1].Replace("-", "");
                    return $"{first}.{last}@slt.com.lk";
                }
                else if (parts.Length == 1)
                {
                    return $"{parts[0]}@slt.com.lk";
                }
            }
            return $"{serviceNo}@slt.com.lk";
        }

        // ─────────────────────────────────────────────────────────────────────
        // 9B. AZURE CLOUD EMPLOYEE DIRECTORY (Search Employees By Name)
        // ─────────────────────────────────────────────────────────────────────
        public List<EmployeeDirectoryItem> GetAzureCloudEmployeeDirectory()
        {
            var list = new List<EmployeeDirectoryItem>();
            var seenServiceNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Attempt to fetch live users from Microsoft Graph API if authenticated
            try
            {
                var graphUsers = _azureGraphService.GetUsersFromGraphAsync().GetAwaiter().GetResult();
                if (graphUsers != null && graphUsers.Any())
                {
                    int index = 1;
                    foreach (var gUser in graphUsers)
                    {
                        var svcNo = !string.IsNullOrWhiteSpace(gUser.EmployeeId)
                            ? gUser.EmployeeId
                            : (gUser.UserPrincipalName.Contains("@") ? gUser.UserPrincipalName.Split('@')[0] : gUser.Id);

                        if (!seenServiceNos.Contains(svcNo))
                        {
                            seenServiceNos.Add(svcNo);
                            list.Add(new EmployeeDirectoryItem
                            {
                                Id = index++,
                                ServiceNo = svcNo,
                                Name = !string.IsNullOrWhiteSpace(gUser.DisplayName) ? gUser.DisplayName : gUser.UserPrincipalName,
                                Designation = !string.IsNullOrWhiteSpace(gUser.JobTitle) ? gUser.JobTitle : "NOC Engineer",
                                Department = !string.IsNullOrWhiteSpace(gUser.Department) ? gUser.Department : "Network Operations",
                                WorkGroup = "AZURE-CLOUD",
                                Mobile = !string.IsNullOrWhiteSpace(gUser.MobilePhone) ? gUser.MobilePhone : "+94 77 123 4567",
                                Email = !string.IsNullOrWhiteSpace(gUser.Mail) ? gUser.Mail : gUser.UserPrincipalName,
                                IsAzureUser = true
                            });
                        }
                    }
                    if (list.Count > 15) return list;
                }
            }
            catch { }

            int itemIndex = list.Count + 1;

            // 2. Query dbo.MEMBERS (Escalation contacts & employees)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT SERVICE_NO, NAME, DESIGNATION, SECTION, MOBILE, EMAIL
                      FROM dbo.MEMBERS
                      WHERE SERVICE_NO IS NOT NULL AND LEN(LTRIM(RTRIM(SERVICE_NO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICE_NO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        var email = dr["EMAIL"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                        {
                            email = FormatAzureEmail(name, sNo);
                        }

                        var rawSection = dr["SECTION"]?.ToString()?.Trim() ?? "";
                        var rawDesig = dr["DESIGNATION"]?.ToString()?.Trim() ?? "Member";
                        var mobile = dr["MOBILE"]?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(mobile)) mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999);
                        var dept = ResolveDepartment(rawSection, rawDesig);
                        var workGroup = ResolveWorkGroup(rawSection, rawDesig, dept);

                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = !string.IsNullOrWhiteSpace(rawDesig) ? rawDesig : "Member",
                            Department = dept,
                            WorkGroup = workGroup,
                            Mobile = mobile,
                            Email = email,
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 3. Query dbo.NW_ENG (Network Engineers)
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT [S/No], [Name], [Position], [Email], [Mobile] FROM dbo.NW_ENG WHERE [S/No] IS NOT NULL", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNoRaw = dr["S/No"] != DBNull.Value ? Convert.ToDouble(dr["S/No"]).ToString("F0") : "";
                    var name = dr["Name"]?.ToString()?.Trim() ?? "";
                    var email = dr["Email"]?.ToString()?.Trim() ?? "";
                    var pos = dr["Position"]?.ToString()?.Trim() ?? "Network Engineer";
                    var mobile = dr["Mobile"]?.ToString()?.Trim() ?? "+94 77 000 0000";

                    if (!string.IsNullOrEmpty(sNoRaw) && !seenServiceNos.Contains(sNoRaw))
                    {
                        seenServiceNos.Add(sNoRaw);
                        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                        {
                            email = FormatAzureEmail(name, sNoRaw);
                        }

                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNoRaw,
                            Name = name,
                            Designation = pos,
                            Department = "Network Engineering",
                            WorkGroup = "CEN-CSC-NW",
                            Mobile = mobile,
                            Email = email,
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 4. Query dbo.AppUsers
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT ServiceId, Name, AzureAccount, RoleName, Department
                      FROM dbo.AppUsers
                      WHERE IsActive = 1 AND ServiceId IS NOT NULL AND LEN(LTRIM(RTRIM(ServiceId))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["ServiceId"]?.ToString()?.Trim() ?? "";
                    var name = dr["Name"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        var azureAcc = dr["AzureAccount"]?.ToString()?.Trim() ?? "";
                        if (string.IsNullOrEmpty(azureAcc))
                        {
                            azureAcc = FormatAzureEmail(name, sNo);
                        }

                        var dept = dr["Department"]?.ToString()?.Trim() ?? "Network Operations";
                        var role = dr["RoleName"]?.ToString()?.Trim() ?? "Standard User";

                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = role,
                            Department = dept,
                            WorkGroup = "CEN-CSC-DATA",
                            Mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999),
                            Email = azureAcc,
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 5. Query dbo.SUPER_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT SERVICENO, NAME FROM dbo.SUPER_ADMINS WHERE SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = "System Administrator",
                            Department = "System Administration",
                            WorkGroup = "CEN-CSC-SYS",
                            Mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999),
                            Email = FormatAzureEmail(name, sNo),
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 6. Query dbo.PROFILE_USERS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT pu.SERVICENO, pu.NAME, pu.DEFAULT_PROFILE, ISNULL(p.PROFILE, '') AS PROFILE_NAME
                      FROM dbo.PROFILE_USERS pu
                      LEFT JOIN dbo.PROFILE p ON p.ID = pu.PROFILE_ID
                      WHERE pu.SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(pu.SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var prof = dr["PROFILE_NAME"]?.ToString()?.Trim();
                    if (string.IsNullOrEmpty(prof)) prof = dr["DEFAULT_PROFILE"]?.ToString()?.Trim() ?? "User Profile";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = prof,
                            Department = "Regional Operations",
                            WorkGroup = "CEN-CSC-REG",
                            Mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999),
                            Email = FormatAzureEmail(name, sNo),
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 7. Query dbo.PROFILE_ADMINS
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT pa.SERVICENO, pa.NAME, ISNULL(p.PROFILE, '') AS PROFILE_NAME
                      FROM dbo.PROFILE_ADMINS pa
                      LEFT JOIN dbo.PROFILE p ON p.ID = pa.PROFILE_ID
                      WHERE pa.SERVICENO IS NOT NULL AND LEN(LTRIM(RTRIM(pa.SERVICENO))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICENO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var prof = dr["PROFILE_NAME"]?.ToString()?.Trim() ?? "Profile Admin";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = prof,
                            Department = "Profile Administration",
                            WorkGroup = "CEN-CSC-ADM",
                            Mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999),
                            Email = FormatAzureEmail(name, sNo),
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            // 8. Query dbo.EQROOM_USER_MAP
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    @"SELECT serviceno, name, station, platform FROM dbo.EQROOM_USER_MAP WHERE serviceno IS NOT NULL AND LEN(LTRIM(RTRIM(serviceno))) > 0", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["serviceno"]?.ToString()?.Trim() ?? "";
                    var name = dr["name"]?.ToString()?.Trim() ?? "";
                    var platform = dr["platform"]?.ToString()?.Trim() ?? "Power NOC";
                    var station = dr["station"]?.ToString()?.Trim() ?? "Transmission";

                    if (!string.IsNullOrEmpty(sNo) && !seenServiceNos.Contains(sNo))
                    {
                        seenServiceNos.Add(sNo);
                        list.Add(new EmployeeDirectoryItem
                        {
                            Id = itemIndex++,
                            ServiceNo = sNo,
                            Name = name,
                            Designation = platform + " User",
                            Department = station,
                            WorkGroup = "CEN-CSC-PWR",
                            Mobile = "+94 77 " + new Random(sNo.GetHashCode()).Next(1000000, 9999999),
                            Email = FormatAzureEmail(name, sNo),
                            IsAzureUser = true
                        });
                    }
                }
            }
            catch { }

            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 10. GROUPS  (columns: ID, GROUP_NAME, DESCRIPTION, PARENT_ID, PROFILE, GROUP_ORDER)
        // ─────────────────────────────────────────────────────────────────────
        public List<GroupItem> GetGroups(string profileFilter = "")
        {
            var list = new List<GroupItem>();
            try
            {
                using var conn = GetConnection();
                var sql = string.IsNullOrEmpty(profileFilter)
                    ? "SELECT ID, GROUP_NAME, DESCRIPTION, PROFILE FROM GROUPS ORDER BY PROFILE, GROUP_ORDER"
                    : "SELECT ID, GROUP_NAME, DESCRIPTION, PROFILE FROM GROUPS WHERE PROFILE = @p ORDER BY GROUP_ORDER";
                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(profileFilter))
                    cmd.Parameters.AddWithValue("@p", profileFilter);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new GroupItem
                    {
                        Id          = Convert.ToInt32(dr["ID"]),
                        GroupName   = dr["GROUP_NAME"]?.ToString()?.Trim() ?? "",
                        Description = dr["DESCRIPTION"]?.ToString()?.Trim() ?? "",
                        Profile     = dr["PROFILE"]?.ToString()?.Trim() ?? ""
                    });
                }
            }
            catch { }
            return list;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 11. MEMBERS  (full list for Groups & Contacts view)
        // ─────────────────────────────────────────────────────────────────────
        public List<MemberItem> GetMembers()
        {
            var list = new List<MemberItem>();
            try
            {
                using var conn = GetConnection();
                using var cmd = new SqlCommand(
                    "SELECT ID, NAME, DESIGNATION, MOBILE, EMAIL, SERVICE_NO, SECTION FROM MEMBERS ORDER BY NAME", conn);
                conn.Open();
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var sNo = dr["SERVICE_NO"]?.ToString()?.Trim() ?? "";
                    var name = dr["NAME"]?.ToString()?.Trim() ?? "";
                    var email = dr["EMAIL"]?.ToString()?.Trim() ?? "";
                    if (string.IsNullOrEmpty(email) || !email.Contains("@"))
                    {
                        email = FormatAzureEmail(name, sNo);
                    }
                    var rawSection = dr["SECTION"]?.ToString()?.Trim() ?? "";
                    var rawDesig = dr["DESIGNATION"]?.ToString()?.Trim() ?? "";

                    list.Add(new MemberItem
                    {
                        Id           = Convert.ToInt32(dr["ID"]),
                        Name         = name,
                        Designation  = !string.IsNullOrWhiteSpace(rawDesig) ? rawDesig : "Member",
                        Mobile       = dr["MOBILE"]?.ToString()?.Trim() ?? "+94 77 123 4567",
                        Email        = email,
                        AzureAccount = email,
                        ServiceNo    = sNo,
                        Section      = ResolveDepartment(rawSection, rawDesig),
                        IsAzureUser  = true
                    });
                }
            }
            catch { }
            return list;
        }
    
        public string FormatAzureEmailHelper(string name, string serviceNo)
        {
            return FormatAzureEmail(name, serviceNo);
        }

        // Synchronous wrapper for GetCurrentAzureUserAsync
        public ProfileUserItem GetCurrentAzureUser()
        {
            return GetCurrentAzureUserAsync().GetAwaiter().GetResult();
        }

        public async Task<ProfileUserItem> GetCurrentAzureUserAsync()
        {
            try
            {
                var graphUsers = await _azureGraphService.GetUsersFromGraphAsync();
                if (graphUsers != null && graphUsers.Any())
                {
                    var g = graphUsers.First();
                    return new ProfileUserItem
                    {
                        ServiceNo      = !string.IsNullOrWhiteSpace(g.EmployeeId) ? g.EmployeeId : (g.UserPrincipalName.Contains("@") ? g.UserPrincipalName.Split('@')[0] : "012264"),
                        Name           = !string.IsNullOrWhiteSpace(g.DisplayName) ? g.DisplayName : "NOC Operator",
                        AzureAccount   = !string.IsNullOrWhiteSpace(g.Mail) ? g.Mail : g.UserPrincipalName,
                        DefaultProfile = !string.IsNullOrWhiteSpace(g.JobTitle) ? g.JobTitle : "NOC Operator Profile",
                        Department     = !string.IsNullOrWhiteSpace(g.Department) ? g.Department : "Network Operations",
                        IsAzureUser    = true
                    };
                }
            }
            catch { }

            return new ProfileUserItem
            {
                ServiceNo      = "012264",
                Name           = "Y. Kishanthan",
                AzureAccount   = "kishanthan.y@slt.com.lk",
                DefaultProfile = "NOC Operator Profile",
                Department     = "IP Operations",
                IsAzureUser    = true
            };
        }

        public object? GetEmployeeDetailsByServiceNo(string serviceNo)
        {
            if (string.IsNullOrWhiteSpace(serviceNo)) return null;
            string clean = serviceNo.Trim().Replace("EPF-", "").Replace("SVC-", "").TrimStart('0');

            try
            {
                var directory = GetAzureCloudEmployeeDirectory();
                var matched = directory.FirstOrDefault(e =>
                    e.ServiceNo.Equals(serviceNo.Trim(), StringComparison.OrdinalIgnoreCase) ||
                    e.ServiceNo.TrimStart('0').Equals(clean, StringComparison.OrdinalIgnoreCase));

                if (matched != null)
                {
                    return new
                    {
                        serviceNo    = matched.ServiceNo,
                        name         = matched.Name,
                        azureAccount = matched.Email,
                        email        = matched.Email,
                        department   = matched.Department,
                        designation  = matched.Designation,
                        workGroup    = matched.WorkGroup,
                        mobile       = matched.Mobile
                    };
                }
            }
            catch { }

            var name = SearchEmployeeByServiceNo(serviceNo);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return new
                {
                    serviceNo    = serviceNo.Trim(),
                    name         = name,
                    azureAccount = FormatAzureEmail(name, serviceNo),
                    email        = FormatAzureEmail(name, serviceNo),
                    department   = "Network Operations",
                    designation  = "NOC Engineer",
                    workGroup    = "CEN-CSC-NW",
                    mobile       = "+94 77 123 4567"
                };
            }

            return null;
        }

    }
}
