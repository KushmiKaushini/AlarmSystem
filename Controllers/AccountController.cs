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
            ViewData["IsDevelopment"] = _env.IsDevelopment();
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] string? username, [FromForm] string? password, [FromForm] string? returnUrl = null)
        {
            // Ignore any posted credentials - normal login is handled via Azure AD SSO
            // Redirect to SignIn to start the Azure AD flow
            return RedirectToAction("SignIn", new { returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult DevBypassLogin(string? returnUrl = null)
        {
            // Security check: only allow in development environment
            if (!_env.IsDevelopment())
            {
                return Unauthorized();
            }

            // Mock claims for development bypass
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "dev-user-id"),
                new Claim(ClaimTypes.Name, "Dev User"),
                new Claim(ClaimTypes.Email, "dev@example.com"),
                new Claim(ClaimTypes.Role, "Developer"),
                new Claim("preferred_username", "dev@example.com")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = false,
                RedirectUri = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home")
            };

            return SignIn(new ClaimsPrincipal(claimsIdentity), authProperties);
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