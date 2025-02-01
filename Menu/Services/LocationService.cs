using Menu.Data;
using Menu.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Menu.Services
{
    public interface ILocationService
    {
        Task<List<NameLocation>> GetLocation(List<string> nameList, string username);

        Task<string> EditLocation(List<NameLocation> namelocationList, string username);

        Task<string> DeleteLocation(string name, string username);

        Task<List<NameLocation>> GetAllIngredientLocationList(string username);
    }
    public class LocationService : ILocationService
    {
        private readonly ILocationData _locationData;
        private readonly IMenuData _menuData;

        public LocationService(ILocationData locationData, IMenuData menuData)
        {
            _locationData = locationData;
            _menuData = menuData;
        }
        public async Task<List<NameLocation>> GetLocation(List<string> nameList, string username)
        {
            var nameLocationList = new List<NameLocation>();
            foreach (string name in nameList)
            {
                NameLocation nameLication = new NameLocation();
                nameLication.Name = name;
                nameLication.Location = await _locationData.GetLocationData(name, username);
                nameLocationList.Add(nameLication);
            }
            return nameLocationList;
        }

        public async Task<string> EditLocation(List<NameLocation> namelocationList, string username)
        {
            foreach(NameLocation namelocation in namelocationList)
            {
                if (await _locationData.GetLocationData(namelocation.Name, username) == "")
                {
                    await _locationData.AddLocationData(namelocation, username);
                }
                else
                {
                    await _locationData.UpdateLocationData(namelocation, username);
                }
            }
            return "location information has been added/updated";

        }

        public async Task<string> DeleteLocation(string name, string username)
        {
            return await _locationData.DeleteLocationData(name, username);
        }

        public async Task<List<NameLocation>> GetAllIngredientLocationList(string username)
        {
            List<string> ingredientNameList = new List<string>();
            MenuList allMenu = await _menuData.GetMenu("All", username);
            foreach (string menu in allMenu.Name)
            {
                List<Ingredient> menuIngredientList = await _menuData.GetIngredient(menu, username);
                foreach (Ingredient menuIngredient in menuIngredientList)
                {
                    if (!ingredientNameList.Contains(menuIngredient.Name))
                    {
                        ingredientNameList.Add(menuIngredient.Name);
                    }
                }
            }
            List<NameLocation> ingredientLocationList = await GetLocation(ingredientNameList, username);
            var sortedIngredients = ingredientLocationList.OrderBy(i => i.Location).ToList();
            return sortedIngredients;
        }
    }
}
