using Menu.Data;

namespace Menu.Services
{
    public interface IMenuService
    {
        Task<List<Dictionary<string, string>>> GetIngredient(string name);
        Task<List<string>> GetMenu(string category);

        Task<string> EditMenu(EditMenuRequest menu);

        Task<string> DeleteMenu(DeleteMenuRequest menu);
    }
    public class MenuService : IMenuService
    {
        private readonly IMenuData _menuData;

        public MenuService(IMenuData menuData)
        {
            _menuData = menuData;
        }
        public async Task<List<Dictionary<string, string>>> GetIngredient(string name)
        {
            return await _menuData.GetIngredient(name);
        }
        public async Task<List<string>> GetMenu(string category)
        {
            return await _menuData.GetMenu(category);
        }
        public async Task<string> EditMenu(EditMenuRequest menu)
        {
            List<string> allMenu = await _menuData.GetMenu("All");
            if (allMenu.Contains(menu.Name))
            {
                return await _menuData.EditMenu(menu);
            }
            else {
                return await _menuData.AddMenu(menu);
            }
        }

        public async Task<string> DeleteMenu(DeleteMenuRequest menu)
        {
            List<string> allMenu = await _menuData.GetMenu("All");
            if (!allMenu.Contains(menu.Name))
            {
                throw new ArgumentException("Menu does not exist");
            }
            return await _menuData.DeleteMenu(menu);
        }
    }
}
