using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CozyComfort.Web.Helpers
{
    public static class ApiHelper
    {
        public static HttpClient GetClient(string token)
        {
            var client = new HttpClient();
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static bool IsUnauthorized(HttpStatusCode status) => status == HttpStatusCode.Unauthorized;

        public static string GetFriendlyError(HttpStatusCode status, string apiMsg, string fallback)
        {
            if (status == HttpStatusCode.Unauthorized) return "Session expired. Please log in again.";
            if (status == HttpStatusCode.Forbidden) return "You don't have permission for this action.";
            try
            {
                var j = JsonSerializer.Deserialize<JsonElement>(apiMsg);
                if (j.TryGetProperty("message", out var m)) return m.GetString() ?? fallback;
            }
            catch { }
            return string.IsNullOrWhiteSpace(apiMsg) ? fallback : (apiMsg.Length > 150 ? apiMsg[..150] + "..." : apiMsg);
        }
    }
}
