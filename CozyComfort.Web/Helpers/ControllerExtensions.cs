using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.Web.Helpers
{
    public static class ControllerExtensions
    {
        public static IActionResult RedirectToLoginWithMessage(this Controller controller, string message)
        {
            controller.HttpContext.Session.Clear();
            controller.TempData["Error"] = message;
            return controller.RedirectToAction("Login", "Auth");
        }
    }
}
