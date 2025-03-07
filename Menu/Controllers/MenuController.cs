using Amazon.Runtime.Internal;
using Menu.Models;
using Menu.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
            var username = HttpContext.Items["Username"].ToString();
            var message = await _menuService.GetIngredient(name, username);
            return Ok(message);
        }
        [HttpGet("GetMenu")]
        public async Task<ActionResult<string>> GetMenu([FromQuery] string category)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _menuService.GetMenu(category, username);
            return Ok(message);
        }
        [HttpPost("EditMenu")]
        public async Task<ActionResult<string>> EditMenu([FromBody] MenuDetails request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            var username = HttpContext.Items["Username"].ToString();
            var message = await _menuService.EditMenu(request, username);
            return Ok(message);
        }
        [HttpPost("DeleteMenu")]
        public async Task<ActionResult<string>> DeleteMenu([FromBody] object request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            try
            {
                var requestData = JsonConvert.DeserializeObject<dynamic>(request.ToString());
                string name = requestData.Name;
                var username = HttpContext.Items["Username"].ToString();
                var message = await _menuService.DeleteMenu(name, username);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  // Return a 400 BadRequest with the error message
            }
        }

        [HttpGet("GetIngredientUnit")]
        public async Task<ActionResult<string>> GetIngredientUnit([FromQuery] string ingredientName)
        {
            if (string.IsNullOrEmpty(ingredientName))
            {
                return BadRequest("Ingredient name must be provided.");
            }
            var username = HttpContext.Items["Username"].ToString();
            var message = await _menuService.GetIngredientUnit(ingredientName, username);
            return Ok(message);
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
        public async Task<ActionResult<string>> UpdateShopList([FromBody] WeeklyChoice request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.UpdateShopList(request, username);
            return Ok(message);
        }

        [HttpGet("GetShopList")]
        public async Task<ActionResult<WeeklyChoice>> GetShopList([FromQuery] string Id)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.GetShopList(Id, username);
            return Ok(message);
        }

        [HttpPost("DeleteShopList")]
        public async Task<ActionResult<WeeklyChoice>> DeleteShopList([FromBody] object request)
        {
            if (request == null)
            {
                return BadRequest("Invalid input.");
            }
            try
            {
                var username = HttpContext.Items["Username"].ToString();
                var requestData = JsonConvert.DeserializeObject<dynamic>(request.ToString());
                string id = requestData.Id;
                string message1 = await _shopListService.DeleteShopList(id, username);
                string message2 = await _shopListService.DeletePurchaseList(id, username);
                return Ok(message1 + "\n" + message2);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  // Return a 400 BadRequest with the error message
            }
        }

        [HttpGet("AggregateList")]
        public async Task<ActionResult<AggregateList>> AggregateShopList([FromQuery] string id)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.AggregateShopList(id, username);
            return Ok(message);
        }

        [HttpGet("GetPurchaseList")]
        public async Task<ActionResult<AggregateList>> GetPurchaseList([FromQuery] string id)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.GetPurchaseList(id, username);
            return Ok(message);
        }

        [HttpPost("UpdatePurchaseList")]
        public async Task<ActionResult<AggregateList>> UpdatePurchaseList([FromBody] AggregateList aggregateList)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.UpdatePurchaseList(aggregateList, username);
            return Ok(message);
        }

        [HttpGet("GetAllPurchaseList")]
        public async Task<ActionResult<List<string>>> GetAllPurchaseList()
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _shopListService.GetAllPurchaseList(username);
            return Ok(message);
        }


    }

    [ApiController]
    [Route("Location")]
    public class LocationController : ControllerBase
    {

        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }


        [HttpGet("GetLocation")]
        public async Task<ActionResult<NameLocation>> GetLocation([FromQuery] List<string> nameList)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _locationService.GetLocation(nameList, username);
            return Ok(message);
        }

        [HttpPost("EditLocation")]
        public async Task<ActionResult<string>> EditLocation([FromBody] List<NameLocation> namelocationList)
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _locationService.EditLocation(namelocationList, username);
            return Ok(message);
        }

        [HttpPost("DeleteLocation")]
        public async Task<ActionResult<string>> DeleteLocation([FromBody] object request)
        {
            try
            {
                var username = HttpContext.Items["Username"].ToString();
                var requestData = JsonConvert.DeserializeObject<dynamic>(request.ToString());
                string name = requestData.Name;
                var message = await _locationService.DeleteLocation(name, username);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);  // Return a 400 BadRequest with the error message
            }
        }

        [HttpGet("GetAllIngredientLocationList")]
        public async Task<ActionResult<NameLocation>> GetAllIngredientLocationList()
        {
            var username = HttpContext.Items["Username"].ToString();
            var message = await _locationService.GetAllIngredientLocationList(username);
            return Ok(message);
        }

    }

}
