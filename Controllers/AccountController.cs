using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using AlarmSystem.Services;

namespace AlarmSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public AccountController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        private bool IsDevBypassAllowed =>
            _env.IsDevelopment()
            || _configuration.GetValue<bool>("DevAuth:BypassEnabled", false)
            || _configuration.GetValue<bool>("Auth:AllowDevBypass", false);

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["IsDevelopment"] = IsDevBypassAllowed;
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password, [FromForm] string? returnUrl = null)
        {
            // If dev credentials posted in development mode, route to bypass
            if (IsDevBypassAllowed && !string.IsNullOrWhiteSpace(username))
            {
                return await HandleDevBypassLogin(username, returnUrl);
            }

            // Normal login is handled via Azure AD SSO
            // Redirect to SignIn to start the Azure AD flow
            return RedirectToAction("SignIn", new { returnUrl });
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> DevBypassLogin([FromForm] string? username = null, string? returnUrl = null)
        {
            if (!IsDevBypassAllowed)
            {
                return Unauthorized();
            }

            return await HandleDevBypassLogin(username, returnUrl);
        }

        private async Task<IActionResult> HandleDevBypassLogin(string? username, string? returnUrl)
        {
            var defaultName = _configuration.GetValue<string>("DevAuth:DisplayName") ?? "Dev User";
            var defaultEmpId = _configuration.GetValue<string>("DevAuth:EmployeeId") ?? "012264";

            var devUser = string.IsNullOrWhiteSpace(username) ? defaultName : username.Trim();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-id"),
                new Claim(ClaimTypes.Name, devUser),
                new Claim("name", devUser),
                new Claim(ClaimTypes.Email, "dev@slt.com.lk"),
                new Claim("email", "dev@slt.com.lk"),
                new Claim(ClaimTypes.Role, AppRoles.SuperAdmin),
                new Claim(ClaimTypes.Role, AppRoles.Admin),
                new Claim(ClaimTypes.Role, "Developer"),
                new Claim("preferred_username", "testuser008@intranet.slt.com.lk"),
                new Claim("employeeId", defaultEmpId),
                new Claim("oid", "00000000-0000-0000-0000-000000000001")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home")
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> SignIn(string? returnUrl = null)
        {
            var redirectUrl = !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "Home");

            var properties = new AuthenticationProperties
            {
                RedirectUri = redirectUrl
            };

            return Challenge(properties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [Authorize]
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
            Response.Headers["Pragma"] = "no-cache";
            Response.Headers["Expires"] = "0";

            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return RedirectToAction("Login");
        }
    }
}
