using Menu.Services;
using Microsoft.AspNetCore.Mvc;

namespace Menu.Controllers
{
    [ApiController]
    [Route("Menu")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }
        [HttpGet("GetIngredient")]
        public async Task<ActionResult<string>> GetIngredient([FromQuery] string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Name must be provided.");
            }

            var message = await _menuService.GetIngredient(name);
            return Ok(message);
        }
        [HttpGet("GetMenu")]
        public async Task<ActionResult<string>> GetMenu([FromQuery] string category)
        {
            var message = await _menuService.GetMenu(category);
            return Ok(message);
        }
        [HttpPost("EditMenu")]
        public async Task<ActionResult<string>> EditMenu([FromBody] EditMenuRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            var message = await _menuService.EditMenu(request);
            return Ok(message);
        }
        [HttpPost("DeleteMenu")]
        public async Task<ActionResult<string>> DeleteMenu([FromBody] DeleteMenuRequest request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            try
            {
                var message = await _menuService.DeleteMenu(request);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  // Return a 400 BadRequest with the error message
            }
        }
    }
}
