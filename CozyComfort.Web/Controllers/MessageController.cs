using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using CozyComfort.Web.Helpers;

namespace CozyComfort.Web.Controllers
{
    public class MessageController : Controller
    {
        private readonly IConfiguration _config;
        private string ApiUrl => (_config["ApiBaseUrl"] ?? "http://localhost:5023").TrimEnd('/') + "/";

        public MessageController(IConfiguration config) => _config = config;

        [HttpPost]
        public async Task<IActionResult> Send(int toUserId, string content)
        {
            var token = HttpContext.Session.GetString("JWToken");
            if (string.IsNullOrEmpty(token)) return this.RedirectToLoginWithMessage("Session expired.");
            var client = ApiHelper.GetClient(token);
            var body = JsonSerializer.Serialize(new { ToUserId = toUserId, Content = content });
            var cnt = new StringContent(body, Encoding.UTF8, "application/json");
            var res = await client.PostAsync(ApiUrl + "api/Messages", cnt);
            var msg = await res.Content.ReadAsStringAsync();
            if (res.IsSuccessStatusCode)
            {
                TempData["Success"] = "Message sent.";
                var referer = Request.Headers["Referer"].FirstOrDefault();
                return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction("Index", "Dashboard");
            }
            if (ApiHelper.IsUnauthorized(res.StatusCode)) return this.RedirectToLoginWithMessage("Session expired. Please log in again.");
            TempData["Error"] = ApiHelper.GetFriendlyError(res.StatusCode, msg, "Failed to send message");
            var ref2 = Request.Headers["Referer"].FirstOrDefault();
            return !string.IsNullOrEmpty(ref2) ? Redirect(ref2) : RedirectToAction("Index", "Dashboard");
        }
    }
}
