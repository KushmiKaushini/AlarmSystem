using System.Security.Claims;

namespace AlarmSystem.Services
{
    public static class AppRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";
        public const string AdminOrSuperAdmin = Admin + "," + SuperAdmin;
    }

    public static class AppRoleExtensions
    {
        public static bool IsSuperAdmin(this ClaimsPrincipal? principal)
        {
            return principal?.IsInRole(AppRoles.SuperAdmin) == true;
        }

        public static bool IsAdminOrSuperAdmin(this ClaimsPrincipal? principal)
        {
            return principal.IsSuperAdmin() || principal?.IsInRole(AppRoles.Admin) == true;
        }
    }

    public static class HardCodedSuperAdminAccess
    {
        private static readonly HashSet<string> AzureAccounts = new(StringComparer.OrdinalIgnoreCase)
        {
            "testuser008@intranet.slt.com.lk"
        };

        private static readonly HashSet<string> ServiceIds = new(StringComparer.OrdinalIgnoreCase)
        {
            "012264"
        };

        public static bool IsMatch(ClaimsPrincipal? principal, string? azureAccount, string? serviceId)
        {
            var normalizedAccounts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            AddIfPresent(normalizedAccounts, azureAccount);

            if (principal is not null)
            {
                AddIfPresent(normalizedAccounts, principal.FindFirst("preferred_username")?.Value);
                AddIfPresent(normalizedAccounts, principal.FindFirst("email")?.Value);
                AddIfPresent(normalizedAccounts, principal.FindFirst(ClaimTypes.Email)?.Value);
                AddIfPresent(normalizedAccounts, principal.FindFirst("upn")?.Value);
                AddIfPresent(normalizedAccounts, principal.FindFirst(ClaimTypes.Upn)?.Value);
                AddIfPresent(normalizedAccounts, principal.FindFirst("unique_name")?.Value);
                AddIfPresent(normalizedAccounts, principal.Identity?.Name);
            }

            if (normalizedAccounts.Overlaps(AzureAccounts))
                return true;

            var normalizedServiceId = Normalize(serviceId);
            if (!string.IsNullOrWhiteSpace(normalizedServiceId) && ServiceIds.Contains(normalizedServiceId))
                return true;

            return false;
        }

        private static void AddIfPresent(ISet<string> set, string? value)
        {
            var normalized = Normalize(value);
            if (string.IsNullOrWhiteSpace(normalized))
                return;

            set.Add(normalized);

            if (TryConvertGuestUpnToEmail(normalized, out var guestEmail))
                set.Add(guestEmail);
        }

        private static string? Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim().ToLowerInvariant();
        }

        private static bool TryConvertGuestUpnToEmail(string normalizedValue, out string email)
        {
            email = string.Empty;

            const string guestMarker = "#ext#@";
            var markerIndex = normalizedValue.IndexOf(guestMarker, StringComparison.Ordinal);
            if (markerIndex <= 0)
                return false;

            var guestPart = normalizedValue[..markerIndex];
            var separatorIndex = guestPart.LastIndexOf('_');
            if (separatorIndex <= 0 || separatorIndex >= guestPart.Length - 1)
                return false;

            email = guestPart[..separatorIndex] + "@" + guestPart[(separatorIndex + 1)..];
            return true;
        }
    }
}