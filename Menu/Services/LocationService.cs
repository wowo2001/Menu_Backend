using Menu.Data;
using Menu.Models;
using System.Linq;
using System.Xml.Linq;

namespace Menu.Services
{
    public interface ILocationService
    {
        Task<List<NameLocation>> GetLocation(List<string> nameList);

        Task<string> EditLocation(List<NameLocation> namelocationList);

        Task<string> DeleteLocation(string name);

        Task<List<NameLocation>> GetAllIngredientLocationList();
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
        public async Task<List<NameLocation>> GetLocation(List<string> nameList)
        {
            var nameLocationList = new List<NameLocation>();
            foreach (string name in nameList)
            {
                NameLocation nameLication = new NameLocation();
                nameLication.Name = name;
                nameLication.Location = await _locationData.GetLocationData(name);
                nameLocationList.Add(nameLication);
            }
            return nameLocationList;
        }

        public async Task<string> EditLocation(List<NameLocation> namelocationList)
        {
            foreach(NameLocation namelocation in namelocationList)
            {
                if (await _locationData.GetLocationData(namelocation.Name) == null)
                {
                    await _locationData.AddLocationData(namelocation);
                }
                else
                {
                    await _locationData.UpdateLocationData(namelocation);
                }
            }
            return "location information has been added/updated";

        }

        public async Task<string> DeleteLocation(string name)
        {
            return await _locationData.DeleteLocationData(name);
        }

        public async Task<List<NameLocation>> GetAllIngredientLocationList()
        {
            List<string> ingredientNameList = new List<string>();
            MenuList allMenu = await _menuData.GetMenu("All");
            foreach (string menu in allMenu.Name)
            {
                List<Ingredient> menuIngredientList = await _menuData.GetIngredient(menu);
                foreach (Ingredient menuIngredient in menuIngredientList)
                {
                    if (!ingredientNameList.Contains(menuIngredient.Name))
                    {
                        ingredientNameList.Add(menuIngredient.Name);
                    }
                }
            }
            return await GetLocation(ingredientNameList);
        }
    }
}
