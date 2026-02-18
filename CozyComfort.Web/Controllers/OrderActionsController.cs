using Microsoft.AspNetCore.Mvc;
using System.Text;
using CozyComfort.Web.Helpers;

namespace CozyComfort.Web.Controllers
{
    public class OrderActionsController : Controller
    {
        private readonly IConfiguration _config;
        private string ApiUrl => (_config["ApiBaseUrl"] ?? "http://localhost:5023").TrimEnd('/') + "/";

        public OrderActionsController(IConfiguration config) => _config = config;

        private HttpClient GetClient()
        {
            var token = HttpContext.Session.GetString("JWToken");
            return ApiHelper.GetClient(token ?? "");
        }

        [HttpPost]
        public async Task<IActionResult> MarkComplete(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return this.RedirectToLoginWithMessage("Session expired.");
                var client = GetClient();
                var res = await client.PutAsync(ApiUrl + $"api/Orders/{id}/complete", null);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) { TempData["Success"] = "Order marked as completed."; return RedirectToAction("Manufacturer", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to update order"); return RedirectToAction("Manufacturer", "Dashboard");
            }
            catch (Exception ex) { TempData["Error"] = "Cannot reach API. " + ex.Message; return RedirectToAction("Manufacturer", "Dashboard"); }
        }

        [HttpPost]
        public async Task<IActionResult> MarkShipped(int id)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return this.RedirectToLoginWithMessage("Session expired.");
                var client = GetClient();
                var res = await client.PutAsync(ApiUrl + $"api/Orders/{id}/shipped", null);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) { TempData["Success"] = "Order marked as shipped."; return RedirectToAction("Distributor", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to update order"); return RedirectToAction("Distributor", "Dashboard");
            }
            catch (Exception ex) { TempData["Error"] = "Cannot reach API. " + ex.Message; return RedirectToAction("Distributor", "Dashboard"); }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrderToManufacturer(string productName, int quantity, decimal unitPrice)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return this.RedirectToLoginWithMessage("Session expired.");
                var client = GetClient();
                var body = System.Text.Json.JsonSerializer.Serialize(new { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(ApiUrl + "api/Orders/distributor-to-manufacturer", content);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) { TempData["Success"] = "Order placed successfully."; return RedirectToAction("Distributor", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to place order"); return RedirectToAction("Distributor", "Dashboard");
            }
            catch (Exception ex) { TempData["Error"] = "Cannot reach API. " + ex.Message; return RedirectToAction("Distributor", "Dashboard"); }
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrderToDistributor(string productName, int quantity, decimal unitPrice, int toUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return this.RedirectToLoginWithMessage("Session expired.");
                var client = GetClient();
                var body = System.Text.Json.JsonSerializer.Serialize(new { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, ToUserId = toUserId });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(ApiUrl + "api/Orders/seller-to-distributor", content);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) { TempData["Success"] = "Order placed successfully."; return RedirectToAction("Seller", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to place order"); return RedirectToAction("Seller", "Dashboard");
            }
            catch (Exception ex) { TempData["Error"] = "Cannot reach API. " + ex.Message; return RedirectToAction("Seller", "Dashboard"); }
        }

        [HttpPost]
        public async Task<IActionResult> Purchase(string productName, int quantity, decimal unitPrice, int toUserId)
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken"))) return this.RedirectToLoginWithMessage("Session expired.");
                var client = GetClient();
                var body = System.Text.Json.JsonSerializer.Serialize(new { ProductName = productName, Quantity = quantity, UnitPrice = unitPrice, ToUserId = toUserId });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var res = await client.PostAsync(ApiUrl + "api/Orders/customer-to-seller", content);
                var msg = await res.Content.ReadAsStringAsync();
                if (res.IsSuccessStatusCode) { TempData["Success"] = "Purchase completed successfully."; return RedirectToAction("Customer", "Dashboard"); }
                if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
                TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to complete purchase"); return RedirectToAction("Customer", "Dashboard");
            }
            catch (Exception ex) { TempData["Error"] = "Cannot reach API. " + ex.Message; return RedirectToAction("Customer", "Dashboard"); }
        }
    }
}
