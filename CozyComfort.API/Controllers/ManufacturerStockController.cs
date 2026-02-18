using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manufacturer,Admin")]
    public class ManufacturerStockController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ManufacturerStockController(AppDbContext context) => _context = context;

        private int? GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet]
        public IActionResult GetMyStock()
        {
            var userId = GetUserId();
            var stock = _context.Inventories
                .Where(x => x.OwnerRole == "Manufacturer" && (x.OwnerUserId == null || x.OwnerUserId == userId))
                .ToList();
            return Ok(stock);
        }

        [HttpPost]
        public IActionResult Add([FromBody] InventoryItemDto dto)
        {
            var userId = GetUserId();
            var item = new Inventory
            {
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Price = dto.Price,
                OwnerUserId = userId,
                OwnerRole = "Manufacturer"
            };
            _context.Inventories.Add(item);
            _context.SaveChanges();
            return Ok(new { message = "Product added", item });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] InventoryItemDto dto)
        {
            var item = _context.Inventories.Find(id);
            if (item == null) return NotFound();
            item.ProductName = dto.ProductName;
            item.Quantity = dto.Quantity;
            item.Price = dto.Price;
            _context.SaveChanges();
            return Ok(new { message = "Updated", item });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var item = _context.Inventories.Find(id);
            if (item == null) return NotFound();
            _context.Inventories.Remove(item);
            _context.SaveChanges();
            return Ok(new { message = "Deleted" });
        }
    }

    public class InventoryItemDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
