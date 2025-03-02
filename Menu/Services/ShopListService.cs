using Menu.Data;
using Menu.Models;

namespace Menu.Services
{
    public interface IShopListService
    {
        Task<string> UpdateShopList(WeeklyChoice choice, string username);

        Task<WeeklyChoice> GetShopList(string Id, string username);

        Task<string> DeleteShopList(string id, string username);

        Task<AggregateList> AggregateShopList(string Id, string username);

        Task<AggregateList> GetPurchaseList(string Id, string username);

        Task<string> UpdatePurchaseList(AggregateList aggregateList, string username);

        Task<List<string>> GetAllPurchaseList(string username);

        Task<string> DeletePurchaseList(string id, string username);
    }

    public class ShopListService : IShopListService
    {
        private readonly IShopListData _shopListData;
        private readonly IMenuData _menuData;
        private readonly IPurchaseListData _purchaseListData;
        private readonly ILocationService _locationService;

        public ShopListService(IShopListData shopListData, IMenuData menuData, IPurchaseListData purchaseListData, ILocationService locationService)
        {
            _shopListData = shopListData;
            _menuData = menuData;
            _purchaseListData = purchaseListData;
            _locationService = locationService;
        }
        public async Task<string> UpdateShopList(WeeklyChoice choice, string username)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(choice.Id, username);
            if (existingChoice.Id == null)
            {
                return await _shopListData.AddShopList(choice, username);
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
                return await _shopListData.UpdateShopList(existingChoice, username);
            }
        }

        public async Task<WeeklyChoice> GetShopList(string Id, string username)
        {
            return await _shopListData.GetShopList(Id, username);
        }

        public async Task<string> DeleteShopList(string id, string username)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(id, username);
            if (existingChoice.Id != null)
            {
                await _shopListData.DeleteShopList(id, username);
            }
            return "Shop list menu does not exist";
        }
        public async Task<AggregateList> AggregateShopList(string Id, string username)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(Id, username);
            AggregateList aggregateList = new AggregateList();
            aggregateList.Id = Id;
            foreach (var dailyChoice in existingChoice.MyChoice)
            {
                foreach(var eachDish in dailyChoice.Dish)
                {
                    var dishIngredientList = await _menuData.GetIngredient(eachDish, username);
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
                            List<NameLocation> nameLocationList = await _locationService.GetLocation(new List<string> { dishIngredient.Name }, username);
                            IngredientPurchase ingredientPurchase = new IngredientPurchase()
                            {
                                Name = dishIngredient.Name,
                                Unit = dishIngredient.Unit,
                                Amount = dishIngredient.Amount,
                                purchased = false,
                                location = nameLocationList[0].Location,
                                source = "menu"
                            };
                            aggregateList.AllIngredientList.Add(ingredientPurchase);
                        }
                        else {
                            for(int i=0; i< aggregateList.AllIngredientList.Count; i++)
                            {
                                if (dishIngredient.Name == aggregateList.AllIngredientList[i].Name && dishIngredient.Unit == aggregateList.AllIngredientList[i].Unit)
                                {
                                    aggregateList.AllIngredientList[i].Amount = aggregateList.AllIngredientList[i].Amount + dishIngredient.Amount;
                                }
                            }


                        }
                    }
                    
                }
            }
            AggregateList existingAggreateList = await _purchaseListData.GetPurchaseList(Id, username);
            if (existingAggreateList.Id != null)
            {
                await _purchaseListData.UpdatePurchaseList(aggregateList, username);
            }
            else
            {
                await _purchaseListData.AddPurchaseList(aggregateList, username);
            }
            return aggregateList;
        }

        public async Task<AggregateList> GetPurchaseList(string Id, string username)
        {
            return await _purchaseListData.GetPurchaseList(Id, username);
        }

        public async Task<string> UpdatePurchaseList(AggregateList aggregateList, string username)
        {
            return await _purchaseListData.UpdatePurchaseList(aggregateList, username);
        }

        public async Task<List<string>> GetAllPurchaseList(string username)
        {
            return await _purchaseListData.GetAllPurchaseList(username);
        }

        public async Task<string> DeletePurchaseList(string id, string username)
        {
            AggregateList existingAggregateList = await _purchaseListData.GetPurchaseList(id, username);
            if (existingAggregateList.Id == null)
            {
                throw new ArgumentException("Purchase list does not exist");
            }
            return await _purchaseListData.DeletePurchaseList(id, username);
        }

    }

    
}
