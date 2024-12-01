using Menu.Data;
using Menu.Models;
using System.Xml.Linq;

namespace Menu.Services
{
    public interface IShopListService
    {
        Task<string> UpdateShopList(Choice choice);

        Task<Choice> GetShopList(string Id);

        Task<string> DeleteShopList(DeleteShopList shopList);

        Task<AggregateList> AggregateShopList(string Id);
    }

    public class ShopListService : IShopListService
    {
        private readonly IShopListData _shopListData;
        private readonly IMenuData _menuData;

        public ShopListService(IShopListData shopListData, IMenuData menuData)
        {
            _shopListData = shopListData;
            _menuData = menuData;
        }
        public async Task<string> UpdateShopList(Choice choice)
        {
            Choice existingChoice = await _shopListData.GetShopList(choice.Id);
            if (existingChoice.Id == null)
            {
                return await _shopListData.AddShopList(choice);
            }
            else {
                bool inExistingChoice = false;
                var existingDate = new List<string>();
                for(int i=0; i < existingChoice.MyChoice.Count; i++)
                {
                    if (existingChoice.MyChoice[i].Day == choice.MyChoice[0].Day)
                    {
                        existingChoice.MyChoice[i] = choice.MyChoice[0];
                        inExistingChoice = true;
                    }
                }
                if(!inExistingChoice)
                {
                    existingChoice.MyChoice.AddRange(choice.MyChoice);
                    
                }
                return await _shopListData.UpdateShopList(existingChoice);
            }
        }

        public async Task<Choice> GetShopList(string Id)
        {
            return await _shopListData.GetShopList(Id);
        }

        public async Task<string> DeleteShopList(DeleteShopList shopList)
        {
            Choice existingChoice = await _shopListData.GetShopList(shopList.Id);
            if (existingChoice.Id == null)
            {
                throw new ArgumentException("Menu does not exist");
            }
            return await _shopListData.DeleteShopList(shopList.Id);
        }
        public async Task<AggregateList> AggregateShopList(string Id)
        {
            Choice existingChoice = await _shopListData.GetShopList(Id);
            AggregateList aggregateList = new AggregateList()
            {
                AllIngredientList = new List<Ingredient>()
            };
            foreach (var dailyChoice in existingChoice.MyChoice)
            {
                foreach(var eachDish in dailyChoice.Dish)
                {
                    var dishIngredientList = await _menuData.GetIngredient(eachDish);
                    foreach (var dishIngredient in dishIngredientList)
                    {
                        bool inAggregateList = false;
                        foreach (var ingredient in aggregateList.AllIngredientList)
                        {
                            if (dishIngredient.Name== ingredient.Name)
                            {
                                inAggregateList = true;
                            }
                        }
                        if (!inAggregateList)
                        {
                            aggregateList.AllIngredientList.Add(dishIngredient);
                        }
                        else {
                            for(int i=0; i< aggregateList.AllIngredientList.Count; i++)
                            {
                                if (dishIngredient.Name == aggregateList.AllIngredientList[i].Name)
                                {
                                    aggregateList.AllIngredientList[i].Amount = aggregateList.AllIngredientList[i].Amount + dishIngredient.Amount;
                                }
                            }


                        }
                    }
                    
                }
            }
            return aggregateList;
        }
    }

    
}
