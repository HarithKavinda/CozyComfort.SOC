using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CozyComfort.Web.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;

        public LoginController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = new HttpClient();
            var apiUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5023";
            if (!apiUrl.EndsWith("/")) apiUrl += "/";

            var data = new
            {
                Email = email,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(apiUrl + "api/Auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Invalid login";
                return View("Index");
            }

            var result = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(result);

            var token = json.RootElement.GetProperty("token").GetString();
            var role = json.RootElement.GetProperty("role").GetString();

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
            {
                ViewBag.Error = "Invalid response from API";
                return View("Index");
            }

            HttpContext.Session.SetString("JWToken", token);
            HttpContext.Session.SetString("Role", role);

            // role redirect
            if (role == "Admin")
                return RedirectToAction("Admin", "Dashboard");

            if (role == "Manufacturer")
                return RedirectToAction("Manufacturer", "Dashboard");

            if (role == "Distributor")
                return RedirectToAction("Distributor", "Dashboard");

            return RedirectToAction("Seller", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }
    }
}
