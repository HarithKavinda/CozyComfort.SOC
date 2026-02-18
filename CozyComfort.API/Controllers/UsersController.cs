using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context) => _context = context;

        [HttpGet("distributors")]
        [Authorize(Roles = "Seller,Manufacturer,Admin")]
        public IActionResult GetDistributors()
        {
            var list = _context.Users
                .Where(x => x.Role == UserRole.Distributor)
                .Select(x => new { x.UserId, x.Email })
                .ToList();
            return Ok(list);
        }

        [HttpGet("sellers")]
        [Authorize(Roles = "Customer,Distributor,Admin")]
        public IActionResult GetSellers()
        {
            var list = _context.Users
                .Where(x => x.Role == UserRole.Seller)
                .Select(x => new { x.UserId, x.Email })
                .ToList();
            return Ok(list);
        }
    }
}
