using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CozyComfort.Web.Controllers
{
    public class RegisterController : Controller
    {
        private readonly IConfiguration _configuration;

        public RegisterController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, int role)
        {
            var client = new HttpClient();
            var apiUrl = _configuration["ApiBaseUrl"] ?? "http://localhost:5023";
            if (!apiUrl.EndsWith("/")) apiUrl += "/";

            var data = new
            {
                Email = email,
                Password = password,
                Role = role
            };

            var content = new StringContent(
                JsonSerializer.Serialize(data),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync(apiUrl + "api/Auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Register failed";
                return View("Index");
            }

            // register success → login page
            return RedirectToAction("Index", "Login");
        }
    }
}
