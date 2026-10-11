using AlarmSystem.Models;
using AlarmSystem.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace AlarmSystem.Services
{
    public class RpbSummaryService : IRpbSummaryService
    {
        private readonly IConfiguration _configuration;

        public RpbSummaryService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection Connection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("TMS"));
        }

        public async Task<RpbSummaryViewModel> GetSummaryAsync()
        {
            RpbSummaryViewModel vm = new RpbSummaryViewModel();

            using SqlConnection con = Connection();

            await con.OpenAsync();

            //--------------------------------------------------
            // Total Distributed & Installed
            //--------------------------------------------------

       SqlCommand totalCmd = new SqlCommand(
    @"SELECT
        ISNULL(SUM(distributedunits),0) AS TotalDistributed,
        ISNULL(SUM(installed),0) AS TotalInstalled
      FROM summary", con);

            SqlDataReader totalReader = await totalCmd.ExecuteReaderAsync();

            if (await totalReader.ReadAsync())
            {
               vm.TotalDistributed =
    Convert.ToInt32(totalReader["TotalDistributed"]);

vm.TotalInstalled =
    Convert.ToInt32(totalReader["TotalInstalled"]);
            }

            await totalReader.CloseAsync();

            //--------------------------------------------------
            // Summary Grid
            //--------------------------------------------------

            SqlCommand gridCmd = new SqlCommand(
                @"SELECT *
                  FROM summary
                  ORDER BY NWEng", con);

            SqlDataReader gridReader =
                await gridCmd.ExecuteReaderAsync();

            while (await gridReader.ReadAsync())
            {
                vm.Summary.Add(new RpbSummary
                {
                    NWEng = gridReader["NWEng"].ToString() ?? "",
                    Designation = gridReader["designation"].ToString() ?? "",
                    DistributedUnits = Convert.ToInt32(gridReader["distributedunits"]),
                    Installed = Convert.ToInt32(gridReader["Installed"]),
                    CompletedP = Convert.ToDecimal(gridReader["completedp"])
                });
            }

            await gridReader.CloseAsync();

            //--------------------------------------------------
            // Map Locations
            //--------------------------------------------------

            SqlCommand mapCmd = new SqlCommand(
                @"SELECT
                    Location,
                    latitude,
                    longitude,
                    NWENG,
                    [Site code],
                    Status
                  FROM rpbcordinates", con);

            SqlDataReader mapReader =
                await mapCmd.ExecuteReaderAsync();

            while (await mapReader.ReadAsync())
            {
                vm.Locations.Add(new RpbLocation
                {
                    Location = mapReader["Location"].ToString() ?? "",
                    Latitude = Convert.ToDouble(mapReader["latitude"]),
                    Longitude = Convert.ToDouble(mapReader["longitude"]),
                    NWENG = mapReader["NWENG"].ToString() ?? "",
                    SiteCode = mapReader["Site code"].ToString() ?? "",
                    Status = Convert.ToInt32(mapReader["Status"])
                });
            }

            await mapReader.CloseAsync();

            return vm;
        }
    }
}