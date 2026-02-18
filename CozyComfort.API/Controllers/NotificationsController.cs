using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context) => _context = context;

        private int? GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet]
        public IActionResult GetMyNotifications()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var list = _context.Notifications
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToList();
            return Ok(list);
        }

        [HttpPut("{id}/read")]
        public IActionResult MarkAsRead(int id)
        {
            var userId = GetUserId();
            var n = _context.Notifications.FirstOrDefault(x => x.Id == id && x.UserId == userId);
            if (n == null) return NotFound();
            n.IsRead = true;
            _context.SaveChanges();
            return Ok();
        }
    }
}
