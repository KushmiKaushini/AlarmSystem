using AlarmSystem.Models.UserManagement;
using AlarmSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net.Mail;

namespace AlarmSystem.Controllers
{
    [ApiController]
    [Route("api/usermanagement")]
    public class UserManagementApiController : ControllerBase
    {
        private readonly SqlConnection _con;

        public UserManagementApiController(SqlConnection con)
        {
            _con = con;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] string? search, [FromQuery] int top = 200)
        {
            if (top < 1) top = 50;
            if (top > 1000) top = 1000;

            var searchLike = string.IsNullOrWhiteSpace(search)
                ? string.Empty
                : $"%{search.Trim().ToLowerInvariant()}%";

            try
            {
                await _con.OpenAsync();
                await EnsureUsersTableAsync(_con);

                using var cmd = new SqlCommand("dbo.usp_UserManagement_GetUsers", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@Top", SqlDbType.Int).Value = top;
                cmd.Parameters.Add("@SearchLike", SqlDbType.NVarChar, 2000).Value = searchLike;

                using var reader = await cmd.ExecuteReaderAsync();
                var rows = new List<UserRecordDto>();

                while (await reader.ReadAsync())
                {
                    rows.Add(new UserRecordDto
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        ServiceId = ReadString(reader, "ServiceId") ?? string.Empty,
                        Name = ReadString(reader, "Name") ?? string.Empty,
                        AzureAccount = ReadString(reader, "AzureAccount") ?? string.Empty,
                        RoleName = ReadString(reader, "RoleName") ?? string.Empty,
                        Department = ReadString(reader, "Department"),
                        Phone = ReadString(reader, "Phone"),
                        IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                        CreatedAtUtc = reader["CreatedAtUtc"] == DBNull.Value
                            ? DateTime.MinValue
                            : Convert.ToDateTime(reader["CreatedAtUtc"]),
                        CreatedBy = ReadString(reader, "CreatedBy")
                    });
                }

                return Ok(new
                {
                    count = rows.Count,
                    rows
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error loading users.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpGet("users/{id:int}")]
        public async Task<IActionResult> GetUserById([FromRoute] int id)
        {
            if (id < 1) return BadRequest(new { message = "Invalid user id." });

            try
            {
                await _con.OpenAsync();
                await EnsureUsersTableAsync(_con);

                using var cmd = new SqlCommand("dbo.usp_UserManagement_GetUserById", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return NotFound(new { message = "User not found." });

                var row = new UserRecordDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ServiceId = ReadString(reader, "ServiceId") ?? string.Empty,
                    Name = ReadString(reader, "Name") ?? string.Empty,
                    AzureAccount = ReadString(reader, "AzureAccount") ?? string.Empty,
                    RoleName = ReadString(reader, "RoleName") ?? string.Empty,
                    Department = ReadString(reader, "Department"),
                    Phone = ReadString(reader, "Phone"),
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    CreatedAtUtc = reader["CreatedAtUtc"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["CreatedAtUtc"]),
                    CreatedBy = ReadString(reader, "CreatedBy")
                };

                return Ok(row);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error loading user.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpPost("users")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest? request)
        {
            if (request is null)
                return BadRequest(new { message = "Request body is required." });

            var name = Normalize(request.Name, maxLength: 120);
            var azureAccount = Normalize(request.AzureAccount, maxLength: 200);
            var roleName = Normalize(request.RoleName, maxLength: 40);
            var department = Normalize(request.Department, maxLength: 120);
            var phone = Normalize(request.Phone, maxLength: 40);

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Name is required." });

            if (string.IsNullOrWhiteSpace(azureAccount))
                return BadRequest(new { message = "Azure Account is required." });

            if (!IsValidEmail(azureAccount))
                return BadRequest(new { message = "A valid Azure Account is required." });

            var serviceId = ExtractServiceId(azureAccount);
            if (string.IsNullOrWhiteSpace(serviceId))
                return BadRequest(new { message = "Service ID could not be derived. Azure Account must have at least 6 characters before @." });

            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new { message = "Role is required." });

            var normalizedRole = NormalizeRoleName(roleName);
            if (normalizedRole is null)
                return BadRequest(new { message = "Role must be Admin or Viewer." });

            var createdBy = "System";

            try
            {
                await _con.OpenAsync();
                await EnsureUsersTableAsync(_con);

                using var cmd = new SqlCommand("dbo.usp_UserManagement_CreateUser", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@ServiceId", SqlDbType.NVarChar, 6).Value = serviceId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = name;
                cmd.Parameters.Add("@AzureAccount", SqlDbType.NVarChar, 200).Value = azureAccount;
                cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 40).Value = normalizedRole;
                cmd.Parameters.Add("@Department", SqlDbType.NVarChar, 120).Value = (object?)department ?? DBNull.Value;
                cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 40).Value = (object?)phone ?? DBNull.Value;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = request.IsActive;
                cmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 200).Value = createdBy;

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return StatusCode(500, new { message = "User was inserted but no row was returned." });

                var row = new UserRecordDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ServiceId = ReadString(reader, "ServiceId") ?? string.Empty,
                    Name = ReadString(reader, "Name") ?? string.Empty,
                    AzureAccount = ReadString(reader, "AzureAccount") ?? string.Empty,
                    RoleName = ReadString(reader, "RoleName") ?? string.Empty,
                    Department = ReadString(reader, "Department"),
                    Phone = ReadString(reader, "Phone"),
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    CreatedAtUtc = reader["CreatedAtUtc"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["CreatedAtUtc"]),
                    CreatedBy = ReadString(reader, "CreatedBy")
                };

                return Ok(new
                {
                    message = "User created successfully.",
                    row
                });
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new { message = "A user with the same Service ID or Azure Account already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error creating user.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpPut("users/{id:int}")]
        public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] UpdateUserRequest? request)
        {
            if (id < 1) return BadRequest(new { message = "Invalid user id." });
            if (request is null)
                return BadRequest(new { message = "Request body is required." });

            var name = Normalize(request.Name, maxLength: 120);
            var azureAccount = Normalize(request.AzureAccount, maxLength: 200);
            var roleName = Normalize(request.RoleName, maxLength: 40);
            var department = Normalize(request.Department, maxLength: 120);
            var phone = Normalize(request.Phone, maxLength: 40);

            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "Name is required." });

            if (string.IsNullOrWhiteSpace(azureAccount))
                return BadRequest(new { message = "Azure Account is required." });

            if (!IsValidEmail(azureAccount))
                return BadRequest(new { message = "A valid Azure Account is required." });

            var serviceId = ExtractServiceId(azureAccount);
            if (string.IsNullOrWhiteSpace(serviceId))
                return BadRequest(new { message = "Service ID could not be derived. Azure Account must have at least 6 characters before @." });

            if (string.IsNullOrWhiteSpace(roleName))
                return BadRequest(new { message = "Role is required." });

            var normalizedRole = NormalizeRoleName(roleName);
            if (normalizedRole is null)
                return BadRequest(new { message = "Role must be Admin or Viewer." });

            try
            {
                await _con.OpenAsync();
                await EnsureUsersTableAsync(_con);

                var existingRole = await GetUserRoleNameAsync(id);
                if (existingRole is null)
                    return NotFound(new { message = "User not found." });



                using var cmd = new SqlCommand("dbo.usp_UserManagement_UpdateUser", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@ServiceId", SqlDbType.NVarChar, 6).Value = serviceId;
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 120).Value = name;
                cmd.Parameters.Add("@AzureAccount", SqlDbType.NVarChar, 200).Value = azureAccount;
                cmd.Parameters.Add("@RoleName", SqlDbType.NVarChar, 40).Value = normalizedRole;
                cmd.Parameters.Add("@Department", SqlDbType.NVarChar, 120).Value = (object?)department ?? DBNull.Value;
                cmd.Parameters.Add("@Phone", SqlDbType.NVarChar, 40).Value = (object?)phone ?? DBNull.Value;
                cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = request.IsActive;

                using var reader = await cmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                    return NotFound(new { message = "User not found." });

                var row = new UserRecordDto
                {
                    Id = Convert.ToInt32(reader["Id"]),
                    ServiceId = ReadString(reader, "ServiceId") ?? string.Empty,
                    Name = ReadString(reader, "Name") ?? string.Empty,
                    AzureAccount = ReadString(reader, "AzureAccount") ?? string.Empty,
                    RoleName = ReadString(reader, "RoleName") ?? string.Empty,
                    Department = ReadString(reader, "Department"),
                    Phone = ReadString(reader, "Phone"),
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                    CreatedAtUtc = reader["CreatedAtUtc"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["CreatedAtUtc"]),
                    CreatedBy = ReadString(reader, "CreatedBy")
                };

                return Ok(new
                {
                    message = "User updated successfully.",
                    row
                });
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                return Conflict(new { message = "A user with the same Service ID or Azure Account already exists." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error updating user.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser([FromRoute] int id)
        {
            if (id < 1) return BadRequest(new { message = "Invalid user id." });

            try
            {
                await _con.OpenAsync();
                await EnsureUsersTableAsync(_con);

                var existingRole = await GetUserRoleNameAsync(id);
                if (existingRole is null)
                    return NotFound(new { message = "User not found." });


                using var cmd = new SqlCommand("dbo.usp_UserManagement_DeleteUser", _con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                var affectedValue = await cmd.ExecuteScalarAsync();
                var affected = affectedValue == null || affectedValue == DBNull.Value
                    ? 0
                    : Convert.ToInt32(affectedValue);
                if (affected == 0)
                    return NotFound(new { message = "User not found." });

                return Ok(new { message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error deleting user.",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
            finally
            {
                try { if (_con.State == ConnectionState.Open) await _con.CloseAsync(); } catch { }
            }
        }

        private static async Task EnsureUsersTableAsync(SqlConnection con)
        {
            using var cmd = new SqlCommand("dbo.usp_AppUsers_EnsureSchema", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            await cmd.ExecuteNonQueryAsync();
        }

        private static string? Normalize(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var mail = new MailAddress(email);
                return string.Equals(mail.Address, email, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string? NormalizeRoleName(string? roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName)) return null;

            var normalized = roleName.Trim().ToLowerInvariant();
            return normalized switch
            {
                "admin" => AppRoles.Admin,
                "viewer" => AppRoles.Viewer,
                _ => null
            };
        }

        private async Task<string?> GetUserRoleNameAsync(int id)
        {
            using var cmd = new SqlCommand("dbo.usp_UserManagement_GetRoleName", _con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            var value = await cmd.ExecuteScalarAsync();
            return value?.ToString();
        }

        private static bool IsAdminRole(string? roleName)
        {
            return string.Equals(roleName?.Trim(), AppRoles.Admin, StringComparison.OrdinalIgnoreCase);
        }

        private static string? ExtractServiceId(string? azureAccount)
        {
            var value = Normalize(azureAccount, 200);
            if (string.IsNullOrWhiteSpace(value)) return null;

            var lower = value.Trim().ToLowerInvariant();
            var atIndex = lower.IndexOf('@');
            var localPart = atIndex > 0 ? lower[..atIndex] : lower;
            localPart = localPart.Trim();

            return localPart.Length >= 6 ? localPart[..6] : null;
        }

        private static string? ReadString(IDataRecord record, string columnName)
        {
            for (var i = 0; i < record.FieldCount; i++)
            {
                if (!string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (record.IsDBNull(i)) return null;
                return record.GetValue(i)?.ToString();
            }

            return null;
        }
    }
}
