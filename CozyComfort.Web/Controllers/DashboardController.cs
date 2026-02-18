using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using CozyComfort.Web.Helpers;

namespace CozyComfort.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;
        private string ApiUrl => (_config["ApiBaseUrl"] ?? "http://localhost:5023").TrimEnd('/') + "/";

        public DashboardController(IConfiguration config) => _config = config;

        private async Task<(T? Data, bool Unauthorized)> GetAsyncWithStatus<T>(string path)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return (default, true);
            var client = ApiHelper.GetClient(token);
            var res = await client.GetAsync(ApiUrl + path);
            if (res.StatusCode == System.Net.HttpStatusCode.Unauthorized) return (default, true);
            if (!res.IsSuccessStatusCode) return (default, false);
            var json = await res.Content.ReadAsStringAsync();
            return (JsonSerializer.Deserialize<T>(json), false);
        }

        private async Task<T?> GetAsync<T>(string path)
        {
            var (data, _) = await GetAsyncWithStatus<T>(path);
            return data;
        }
        // ⭐ Home page eken Dashboard ekata - role ekata anuwa redirect
        public IActionResult Index()
        {
            var userData = HttpContext.Session.GetString("UserData");
            if (string.IsNullOrEmpty(userData))
                return RedirectToAction("Login", "Auth");

            if (userData.Contains("role\":\"Admin")) return RedirectToAction("Admin");
            if (userData.Contains("role\":\"Manufacturer")) return RedirectToAction("Manufacturer");
            if (userData.Contains("role\":\"Distributor")) return RedirectToAction("Distributor");
            if (userData.Contains("role\":\"Seller")) return RedirectToAction("Seller");
            if (userData.Contains("role\":\"Customer") || userData.Contains("role\":\"User")) return RedirectToAction("Customer");

            return RedirectToAction("AccessDenied");
        }

        public IActionResult Admin()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                return RedirectToAction("Login", "Auth");
            return View();
        }

        public async Task<IActionResult> Manufacturer()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                return RedirectToAction("Login", "Auth");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
                return RedirectToAction("Login", "Auth");
            var (stock, u1) = await GetAsyncWithStatus<List<JsonElement>>("api/ManufacturerStock");
            if (u1) { HttpContext.Session.Clear(); TempData["Error"] = "Session expired."; return RedirectToAction("Login", "Auth"); }
            ViewBag.Stock = stock ?? new List<JsonElement>();
            ViewBag.Notifications = await GetAsync<List<JsonElement>>("api/Notifications") ?? new List<JsonElement>();
            ViewBag.Orders = await GetAsync<List<JsonElement>>("api/Orders/manufacturer") ?? new List<JsonElement>();
            ViewBag.Distributors = await GetAsync<List<JsonElement>>("api/Users/distributors") ?? new List<JsonElement>();
            return View();
        }

        public async Task<IActionResult> Distributor()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                return RedirectToAction("Login", "Auth");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
                return RedirectToAction("Login", "Auth");
            var (mfgStock, u1) = await GetAsyncWithStatus<List<JsonElement>>("api/Stock/manufacturer");
            if (u1) { HttpContext.Session.Clear(); TempData["Error"] = "Session expired."; return RedirectToAction("Login", "Auth"); }
            ViewBag.ManufacturerStock = mfgStock ?? new List<JsonElement>();
            ViewBag.MyStock = await GetAsync<List<JsonElement>>("api/Stock/distributor/mine") ?? new List<JsonElement>();
            ViewBag.Notifications = await GetAsync<List<JsonElement>>("api/Notifications") ?? new List<JsonElement>();
            ViewBag.Orders = await GetAsync<List<JsonElement>>("api/Orders") ?? new List<JsonElement>();
            ViewBag.Messages = await GetAsync<List<JsonElement>>("api/Messages") ?? new List<JsonElement>();
            ViewBag.Sellers = await GetAsync<List<JsonElement>>("api/Users/sellers") ?? new List<JsonElement>();
            return View();
        }

        public async Task<IActionResult> Seller()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                return RedirectToAction("Login", "Auth");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
                return RedirectToAction("Login", "Auth");
            var (distStock, u1) = await GetAsyncWithStatus<List<JsonElement>>("api/Stock/distributor");
            if (u1) { HttpContext.Session.Clear(); TempData["Error"] = "Session expired."; return RedirectToAction("Login", "Auth"); }
            ViewBag.DistributorStock = distStock ?? new List<JsonElement>();
            ViewBag.MyStock = await GetAsync<List<JsonElement>>("api/Stock/seller/mine") ?? new List<JsonElement>();
            ViewBag.Notifications = await GetAsync<List<JsonElement>>("api/Notifications") ?? new List<JsonElement>();
            ViewBag.Messages = await GetAsync<List<JsonElement>>("api/Messages") ?? new List<JsonElement>();
            ViewBag.Distributors = await GetAsync<List<JsonElement>>("api/Users/distributors") ?? new List<JsonElement>();
            return View();
        }

        public async Task<IActionResult> Customer()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserData")))
                return RedirectToAction("Login", "Auth");
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
                return RedirectToAction("Login", "Auth");
            var (sellerStock, u1) = await GetAsyncWithStatus<List<JsonElement>>("api/Stock/seller");
            if (u1) { HttpContext.Session.Clear(); TempData["Error"] = "Session expired."; return RedirectToAction("Login", "Auth"); }
            ViewBag.SellerStock = sellerStock ?? new List<JsonElement>();
            ViewBag.Sellers = await GetAsync<List<JsonElement>>("api/Users/sellers") ?? new List<JsonElement>();
            ViewBag.MyOrders = await GetAsync<List<JsonElement>>("api/Orders/customer") ?? new List<JsonElement>();
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
