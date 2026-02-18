using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.Web.Controllers
{
    /// <summary>
    /// Legacy inventory route. The real inventory views now live inside
    /// the role-based dashboards, so this simply redirects.
    /// </summary>
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            // Distributor and Manufacturer see inventory inside their dashboards.
            return RedirectToAction("Distributor", "Dashboard");
        }
    }
}
