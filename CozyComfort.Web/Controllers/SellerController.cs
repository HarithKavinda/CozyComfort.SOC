using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.Web.Controllers
{
    /// <summary>
    /// Legacy route kept only to avoid 404s.
    /// All seller operations now live inside the main Seller dashboard.
    /// Hitting /Seller/CreateOrder simply redirects to the new dashboard flow.
    /// </summary>
    public class SellerController : Controller
    {
        [HttpGet]
        public IActionResult CreateOrder()
        {
            // Seller order creation is handled from the Seller dashboard tables/forms.
            return RedirectToAction("Seller", "Dashboard");
        }

        [HttpPost]
        public IActionResult CreateOrder(string productName, int quantity)
        {
            // Old POST endpoint is no longer used – send user to the proper dashboard.
            return RedirectToAction("Seller", "Dashboard");
        }
    }
}
