using Amazon.Runtime.Internal;
using Menu.Models;
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
        public async Task<ActionResult<string>> EditMenu([FromBody] MenuDetails request)
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

    [ApiController]
    [Route("ShopList")]
    public class ShopListController : ControllerBase
    {

        private readonly IShopListService _shopListService;
        public ShopListController(IShopListService shopListService)
        {
            _shopListService = shopListService;
        }
        [HttpPost("UpdateShopList")]
        public async Task<ActionResult<string>> UpdateShopList([FromBody] Choice request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            var message = await _shopListService.UpdateShopList(request);
            return Ok(message);
        }

        [HttpGet("GetShopList")]
        public async Task<ActionResult<Choice>> GetShopList([FromQuery] string Id)
        {

            var message = await _shopListService.GetShopList(Id);
            return Ok(message);
        }

        [HttpPost("DeleteShopList")]
        public async Task<ActionResult<Choice>> DeleteShopList([FromBody] DeleteShopList request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            try
            {
                var message = await _shopListService.DeleteShopList(request);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  // Return a 400 BadRequest with the error message
            }
        }

        [HttpGet("AggregateList")]
        public async Task<ActionResult<AggregateList>> AggregateShopList([FromQuery] string Id)
        {
            var message = await _shopListService.AggregateShopList(Id);
            return Ok(message);
        }
    }
}
