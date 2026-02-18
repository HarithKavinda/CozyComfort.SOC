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
    public class StockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StockController(AppDbContext context) => _context = context;

        private int? GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet("manufacturer")]
        [Authorize(Roles = "Distributor,Manufacturer,Admin")]
        public IActionResult GetManufacturerStock()
        {
            var list = _context.Inventories
                .Where(x => x.OwnerRole == "Manufacturer")
                .ToList();
            return Ok(list);
        }

        [HttpGet("distributor")]
        [Authorize(Roles = "Seller,Distributor,Admin")]
        public IActionResult GetDistributorStock()
        {
            var list = _context.Inventories
                .Where(x => x.OwnerRole == "Distributor")
                .ToList();
            return Ok(list);
        }

        [HttpGet("distributor/mine")]
        [Authorize(Roles = "Distributor,Admin")]
        public IActionResult GetMyDistributorStock()
        {
            var userId = GetUserId();
            var list = _context.Inventories
                .Where(x => x.OwnerRole == "Distributor" && x.OwnerUserId == userId)
                .ToList();
            return Ok(list);
        }

        [HttpGet("seller")]
        [Authorize(Roles = "Customer,Seller,Admin")]
        public IActionResult GetSellerStock()
        {
            var list = _context.Inventories
                .Where(x => x.OwnerRole == "Seller")
                .ToList();
            return Ok(list);
        }

        [HttpGet("seller/mine")]
        [Authorize(Roles = "Seller,Admin")]
        public IActionResult GetMySellerStock()
        {
            var userId = GetUserId();
            var list = _context.Inventories
                .Where(x => x.OwnerRole == "Seller" && x.OwnerUserId == userId)
                .ToList();
            return Ok(list);
        }
    }
}
