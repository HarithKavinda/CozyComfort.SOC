using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CozyComfort.Web.Controllers
{
    public class AuthController : Controller
    {
        private readonly HttpClient _httpClient;

        public AuthController(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            var apiUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5023";
            if (!apiUrl.EndsWith("/")) apiUrl += "/";
            _httpClient.BaseAddress = new Uri(apiUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        // ================= REGISTER PAGE =================

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ================= REGISTER POST =================

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string? role)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields required";
                return View();
            }

            var data = new
            {
                email = email.Trim(),
                password = password,
                role = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim()
            };

            var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/register", content);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ViewBag.Error = "Register failed: " + error;
                    return View();
                }
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to API. Make sure CozyComfort.API is running on " + _httpClient.BaseAddress;
                return View();
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "API request timed out.";
                return View();
            }

            TempData["Success"] = "Register Success! Please login.";

            // ⭐ Register success → Login page ekata redirect
            return RedirectToAction("Login", "Auth");
        }

        // ================= LOGIN PAGE =================

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ================= LOGIN POST =================

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email & Password required";
                return View();
            }

            var data = new
            {
                email = email,
                password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                var response = await _httpClient.PostAsync("api/Auth/login", content);

                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Invalid login";
                    return View();
                }

                var result = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(result);
                var token = json.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;

                HttpContext.Session.SetString("UserData", result);
                if (!string.IsNullOrEmpty(token)) HttpContext.Session.SetString("JWToken", token);

                // ⭐ Login success → Role-based Dashboard ekata
                return RedirectToAction("Index", "Dashboard");
            }
            catch (HttpRequestException)
            {
                ViewBag.Error = "Cannot connect to API. Make sure CozyComfort.API is running.";
                return View();
            }
            catch (TaskCanceledException)
            {
                ViewBag.Error = "API request timed out.";
                return View();
            }
        }

        // ================= LOGOUT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
