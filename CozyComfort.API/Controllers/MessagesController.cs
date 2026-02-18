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
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MessagesController(AppDbContext context) => _context = context;

        private int? GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet]
        public IActionResult GetMyMessages()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();
            var list = _context.Messages
                .Where(x => x.ToUserId == userId || x.FromUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Take(50)
                .ToList();
            return Ok(list);
        }

        [HttpPost]
        public IActionResult Send([FromBody] SendMessageDto dto)
        {
            var fromId = GetUserId();
            if (fromId == null) return Unauthorized();
            var msg = new Message
            {
                FromUserId = fromId.Value,
                ToUserId = dto.ToUserId,
                Content = dto.Content
            };
            _context.Messages.Add(msg);
            _context.SaveChanges();

            var toUser = _context.Users.Find(dto.ToUserId);
            _context.Notifications.Add(new Notification
            {
                UserId = dto.ToUserId,
                Title = "New Message",
                Message = dto.Content.Length > 80 ? dto.Content[..80] + "..." : dto.Content,
                RelatedEntityType = "Message",
                RelatedEntityId = msg.Id
            });
            _context.SaveChanges();

            return Ok(new { message = "Sent", msg });
        }
    }

    public class SendMessageDto
    {
        public int ToUserId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
