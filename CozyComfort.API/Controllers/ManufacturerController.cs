using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Manufacturer,Admin")]
    public class ManufacturerController : ControllerBase
    {
        [HttpGet("production")]
        public IActionResult GetProduction()
        {
            return Ok("Production details");
        }
    }
}
