using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CozyComfort.Data.Context;
using CozyComfort.Data.Models;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context) => _context = context;

        private int? GetUserId() => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

        [HttpGet]
        [Authorize(Roles = "Distributor,Admin")]
        public IActionResult GetDistributorOrders()
        {
            var userId = GetUserId();
            var orders = _context.Orders
                .Where(x => x.OrderType == "SellerToDistributor" && x.ToUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return Ok(orders);
        }

        [HttpGet("manufacturer")]
        [Authorize(Roles = "Manufacturer,Admin")]
        public IActionResult GetManufacturerOrders()
        {
            var userId = GetUserId();
            var orders = _context.Orders
                .Where(x => x.OrderType == "DistributorToManufacturer" && x.ToUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return Ok(orders);
        }

        [HttpGet("seller")]
        [Authorize(Roles = "Seller,Admin")]
        public IActionResult GetSellerOrders()
        {
            var userId = GetUserId();
            var orders = _context.Orders
                .Where(x => (x.OrderType == "SellerToDistributor" && x.FromUserId == userId) || (x.OrderType == "CustomerToSeller" && x.ToUserId == userId))
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return Ok(orders);
        }

        [HttpGet("customer")]
        [Authorize(Roles = "Customer,Admin")]
        public IActionResult GetCustomerOrders()
        {
            var userId = GetUserId();
            var orders = _context.Orders
                .Where(x => x.OrderType == "CustomerToSeller" && x.FromUserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();
            return Ok(orders);
        }

        [HttpPost("distributor-to-manufacturer")]
        [Authorize(Roles = "Distributor")]
        public IActionResult PlaceOrderToManufacturer([FromBody] CreateOrderDto dto)
        {
            var fromId = GetUserId();
            var mfgStock = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Manufacturer" && x.ProductName == dto.ProductName);
            if (mfgStock == null || mfgStock.Quantity < dto.Quantity)
                return BadRequest(new { message = "Insufficient manufacturer stock" });
            var manufacturer = _context.Users.FirstOrDefault(x => x.Role == UserRole.Manufacturer);
            if (manufacturer == null) return BadRequest(new { message = "No manufacturer found" });

            var order = new Order
            {
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Status = "Pending",
                FromUserId = fromId,
                ToUserId = manufacturer.UserId,
                OrderType = "DistributorToManufacturer"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            _context.Notifications.Add(new Notification
            {
                UserId = manufacturer.UserId,
                Title = "New Order",
                Message = $"Distributor placed order: {dto.ProductName} x {dto.Quantity}",
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            });
            _context.SaveChanges();

            return Ok(order);
        }

        [HttpPost("seller-to-distributor")]
        [Authorize(Roles = "Seller")]
        public IActionResult PlaceOrderToDistributor([FromBody] CreateOrderDto dto)
        {
            var fromId = GetUserId();
            if (dto.ToUserId == null) return BadRequest(new { message = "ToUserId required" });
            var distStock = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Distributor" && x.OwnerUserId == dto.ToUserId && x.ProductName == dto.ProductName);
            if (distStock == null || distStock.Quantity < dto.Quantity)
                return BadRequest(new { message = "Insufficient distributor stock" });

            var order = new Order
            {
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Status = "Pending",
                FromUserId = fromId,
                ToUserId = dto.ToUserId,
                OrderType = "SellerToDistributor"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            _context.Notifications.Add(new Notification
            {
                UserId = dto.ToUserId.Value,
                Title = "New Order from Seller",
                Message = $"{dto.ProductName} x {dto.Quantity}",
                RelatedEntityType = "Order",
                RelatedEntityId = order.Id
            });
            _context.SaveChanges();

            return Ok(order);
        }

        [HttpPost("customer-to-seller")]
        [Authorize(Roles = "Customer")]
        public IActionResult PlaceOrderToSeller([FromBody] CreateOrderDto dto)
        {
            var fromId = GetUserId();
            if (dto.ToUserId == null) return BadRequest(new { message = "ToUserId required" });
            var sellerStock = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Seller" && x.OwnerUserId == dto.ToUserId && x.ProductName == dto.ProductName);
            if (sellerStock == null || sellerStock.Quantity < dto.Quantity) return BadRequest(new { message = "Insufficient stock" });
            sellerStock.Quantity -= dto.Quantity;

            var order = new Order
            {
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                UnitPrice = dto.UnitPrice,
                Status = "Completed",
                FromUserId = fromId,
                ToUserId = dto.ToUserId,
                OrderType = "CustomerToSeller"
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            return Ok(order);
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Manufacturer")]
        public IActionResult MarkManufacturerOrderComplete(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null || order.OrderType != "DistributorToManufacturer") return NotFound();
            order.Status = "Completed";
            var mfgStock = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Manufacturer" && x.ProductName == order.ProductName);
            if (mfgStock != null) mfgStock.Quantity = Math.Max(0, mfgStock.Quantity - order.Quantity);
            if (order.FromUserId.HasValue)
            {
                var existing = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Distributor" && x.OwnerUserId == order.FromUserId && x.ProductName == order.ProductName);
                if (existing != null) existing.Quantity += order.Quantity;
                else _context.Inventories.Add(new Inventory { ProductName = order.ProductName, Quantity = order.Quantity, Price = order.UnitPrice, OwnerUserId = order.FromUserId, OwnerRole = "Distributor" });
            }
            _context.SaveChanges();

            if (order.FromUserId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = order.FromUserId.Value,
                    Title = "Order Completed",
                    Message = $"Your order for {order.ProductName} x {order.Quantity} has been completed by the manufacturer.",
                    RelatedEntityType = "Order",
                    RelatedEntityId = order.Id
                });
                _context.SaveChanges();
            }
            return Ok(order);
        }

        [HttpPut("{id}/shipped")]
        [Authorize(Roles = "Distributor")]
        public IActionResult MarkDistributorOrderShipped(int id)
        {
            var order = _context.Orders.Find(id);
            if (order == null || order.OrderType != "SellerToDistributor") return NotFound();
            order.Status = "Shipped";
            var distStock = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Distributor" && x.OwnerUserId == order.ToUserId && x.ProductName == order.ProductName);
            if (distStock != null) distStock.Quantity = Math.Max(0, distStock.Quantity - order.Quantity);
            if (order.FromUserId.HasValue)
            {
                var existing = _context.Inventories.FirstOrDefault(x => x.OwnerRole == "Seller" && x.OwnerUserId == order.FromUserId && x.ProductName == order.ProductName);
                if (existing != null) existing.Quantity += order.Quantity;
                else _context.Inventories.Add(new Inventory { ProductName = order.ProductName, Quantity = order.Quantity, Price = order.UnitPrice, OwnerUserId = order.FromUserId, OwnerRole = "Seller" });
            }
            _context.SaveChanges();

            if (order.FromUserId.HasValue)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = order.FromUserId.Value,
                    Title = "Order Shipped",
                    Message = $"Your order for {order.ProductName} x {order.Quantity} has been shipped.",
                    RelatedEntityType = "Order",
                    RelatedEntityId = order.Id
                });
                _context.SaveChanges();
            }
            return Ok(order);
        }
    }

    public class CreateOrderDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? ToUserId { get; set; }
    }
}
