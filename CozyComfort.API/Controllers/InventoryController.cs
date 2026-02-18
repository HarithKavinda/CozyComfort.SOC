using Microsoft.AspNetCore.Mvc;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;
using Microsoft.AspNetCore.Authorization; // ✅ add for [Authorize]

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public InventoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/inventory
        [HttpGet]
        [Authorize(Roles = "Distributor,Admin")]
        public IActionResult GetAll()
        {
            return Ok(_context.Inventories.ToList());
        }

        // POST: api/inventory
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult Add([FromBody] Inventory item)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _context.Inventories.Add(item);
            _context.SaveChanges();

            return Ok(new { message = "Item added successfully", item });
        }

        // Optional: GET by id
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public IActionResult GetById(int id)
        {
            var item = _context.Inventories.FirstOrDefault(x => x.Id == id); // ✅ FIX HERE
            if (item == null) return NotFound(new { message = "Item not found" });

            return Ok(item);
        }

        // Optional: DELETE by id
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var item = _context.Inventories.FirstOrDefault(x => x.Id == id); // ✅ FIX HERE
            if (item == null) return NotFound(new { message = "Item not found" });

            _context.Inventories.Remove(item);
            _context.SaveChanges();

            return Ok(new { message = "Item deleted successfully" });
        }
    }
}
