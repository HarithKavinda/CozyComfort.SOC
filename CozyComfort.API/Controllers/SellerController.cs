using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Seller,Admin")]
    public class SellerController : ControllerBase
    {
        [HttpGet("orders")]
        public IActionResult GetOrders()
        {
            return Ok("Seller orders");
        }
    }
}
