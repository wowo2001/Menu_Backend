using Menu.Data;
using Menu.Models;


namespace Menu.Services
{
    public interface IMenuService
    {
        Task<List<Ingredient>> GetIngredient(string name, string username);
        Task<MenuList> GetMenu(string category, string username);

        Task<string> EditMenu(MenuDetails menu, string username);

        Task<string> DeleteMenu(string name, string username);

        Task<string> GetIngredientUnit(string ingredientName, string username);
    }
    public class MenuService : IMenuService
    {
        private readonly IMenuData _menuData;

        public MenuService(IMenuData menuData)
        {
            _menuData = menuData;
        }
        public async Task<List<Ingredient>> GetIngredient(string name, string username)
        {
            return await _menuData.GetIngredient(name, username);
        }
        public async Task<MenuList> GetMenu(string category, string username)
        {
            return await _menuData.GetMenu(category, username);
        }
        public async Task<string> EditMenu(MenuDetails menu, string username)
        {
            MenuList allMenu = await _menuData.GetMenu("All", username);
            if (allMenu.Name.Contains(menu.Name))
            {
                return await _menuData.EditMenu(menu, username);
            }
            else {
                return await _menuData.AddMenu(menu, username);
            }
        }

        public async Task<string> DeleteMenu(string name, string username)
        {
            MenuList allMenu = await _menuData.GetMenu("All", username);
            if (!allMenu.Name.Contains(name))
            {
                throw new ArgumentException("Menu does not exist");
            }
            return await _menuData.DeleteMenu(name, username);
        }

        public async Task<string> GetIngredientUnit(string ingredientName, string username)
        {
            MenuList allMenu = await _menuData.GetMenu("All", username);
            List<Ingredient> overlIngredientList = new List<Ingredient>(); 
            foreach (string menu in allMenu.Name)
            {
                List<Ingredient> menuIngredientList = await _menuData.GetIngredient(menu, username);
                foreach (Ingredient menuIngredient in menuIngredientList)
                {
                    overlIngredientList.Add(menuIngredient);
                }
                foreach (Ingredient ingredient in overlIngredientList)
                {
                    if (ingredient.Name.Equals(ingredientName))
                    {
                        return ingredient.Unit;
                    }
                }
            }
            
            return "";

        }

    }
}
