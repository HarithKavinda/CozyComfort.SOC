using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If already logged in, redirect to role dashboard
            var userData = HttpContext.Session.GetString("UserData");
            if (!string.IsNullOrEmpty(userData))
                return RedirectToAction("Index", "Dashboard");

            return View();
        }
    }
}
