using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using CozyComfort.Web.Helpers;

namespace CozyComfort.Web.Controllers
{
    public class ManufacturerStockController : Controller
    {
        private readonly IConfiguration _config;
        private string ApiUrl => (_config["ApiBaseUrl"] ?? "http://localhost:5023").TrimEnd('/') + "/";

        public ManufacturerStockController(IConfiguration config) => _config = config;

        private HttpClient GetClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            return ApiHelper.GetClient(token ?? "");
        }

        [HttpPost]
        public async Task<IActionResult> Add(string productName, int quantity, decimal price)
        {
            try
            {
                var token = HttpContext.Session.GetString("JWToken");
                if (string.IsNullOrEmpty(token)) return RedirectToLogin("Session expired.");
                var client = GetClient();
                var body = JsonSerializer.Serialize(new { ProductName = productName?.Trim() ?? "", Quantity = quantity, Price = price });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(ApiUrl + "api/ManufacturerStock", content);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode)
                { TempData["Success"] = "Product added successfully."; return RedirectToAction("Manufacturer", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return RedirectToLogin("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to add product");
                return RedirectToAction("Manufacturer", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot reach API. Ensure API is running. " + ex.Message;
                return RedirectToAction("Manufacturer", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, string productName, int quantity, decimal price)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return RedirectToLogin("Session expired.");
                var client = GetClient();
                var body = JsonSerializer.Serialize(new { ProductName = productName?.Trim() ?? "", Quantity = quantity, Price = price });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PutAsync(ApiUrl + $"api/ManufacturerStock/{id}", content);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode)
                { TempData["Success"] = "Product updated successfully."; return RedirectToAction("Manufacturer", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return RedirectToLogin("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to update product");
                return RedirectToAction("Manufacturer", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot reach API. " + ex.Message;
                return RedirectToAction("Manufacturer", "Dashboard");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return RedirectToLogin("Session expired.");
                var client = GetClient();
                var res = await client.DeleteAsync(ApiUrl + $"api/ManufacturerStock/{id}");
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode)
                { TempData["Success"] = "Product deleted."; return RedirectToAction("Manufacturer", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return RedirectToLogin("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to delete product");
                return RedirectToAction("Manufacturer", "Dashboard");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Cannot reach API. " + ex.Message;
                return RedirectToAction("Manufacturer", "Dashboard");
            }
        }

        private IActionResult RedirectToLogin(string message) => this.RedirectToLoginWithMessage(message);
    }
}
