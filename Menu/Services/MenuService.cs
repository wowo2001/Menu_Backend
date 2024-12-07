﻿using Menu.Data;
using Menu.Models;


namespace Menu.Services
{
    public interface IMenuService
    {
        Task<List<Ingredient>> GetIngredient(string name);
        Task<MenuList> GetMenu(string category);

        Task<string> EditMenu(MenuDetails menu);

        Task<string> DeleteMenu(string name);
    }
    public class MenuService : IMenuService
    {
        private readonly IMenuData _menuData;

        public MenuService(IMenuData menuData)
        {
            _menuData = menuData;
        }
        public async Task<List<Ingredient>> GetIngredient(string name)
        {
            return await _menuData.GetIngredient(name);
        }
        public async Task<MenuList> GetMenu(string category)
        {
            return await _menuData.GetMenu(category);
        }
        public async Task<string> EditMenu(MenuDetails menu)
        {
            MenuList allMenu = await _menuData.GetMenu("All");
            if (allMenu.Name.Contains(menu.Name))
            {
                return await _menuData.EditMenu(menu);
            }
            else {
                return await _menuData.AddMenu(menu);
            }
        }

        public async Task<string> DeleteMenu(string name)
        {
            MenuList allMenu = await _menuData.GetMenu("All");
            if (!allMenu.Name.Contains(name))
            {
                throw new ArgumentException("Menu does not exist");
            }
            return await _menuData.DeleteMenu(name);
        }

    }
}
