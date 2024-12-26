using Menu.Data;
using Menu.Models;

namespace Menu.Services
{
    public interface IShopListService
    {
        Task<string> UpdateShopList(WeeklyChoice choice);

        Task<WeeklyChoice> GetShopList(string Id);

        Task<string> DeleteShopList(string id);

        Task<AggregateList> AggregateShopList(string Id);

        Task<AggregateList> GetPurchaseList(string Id);

        Task<string> UpdatePurchaseList(AggregateList aggregateList);

        Task<List<string>> GetAllPurchaseList();

        Task<string> DeletePurchaseList(string id);
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
        public async Task<string> UpdateShopList(WeeklyChoice choice)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(choice.Id);
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

        public async Task<WeeklyChoice> GetShopList(string Id)
        {
            return await _shopListData.GetShopList(Id);
        }

        public async Task<string> DeleteShopList(string id)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(id);
            if (existingChoice.Id == null)
            {
                throw new ArgumentException("Menu does not exist");
            }
            return await _shopListData.DeleteShopList(id);
        }
        public async Task<AggregateList> AggregateShopList(string Id)
        {
            WeeklyChoice existingChoice = await _shopListData.GetShopList(Id);
            AggregateList aggregateList = new AggregateList();
            aggregateList.Id = Id;
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
                            List<NameLocation> nameLocationList = await _locationService.GetLocation(new List<string> { dishIngredient.Name });
                            IngredientPurchase ingredientPurchase = new IngredientPurchase()
                            {
                                Name = dishIngredient.Name,
                                Unit = dishIngredient.Unit,
                                Amount = dishIngredient.Amount,
                                purchased = false,
                                location = nameLocationList[0].Location
                            };
                            aggregateList.AllIngredientList.Add(ingredientPurchase);
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
            AggregateList existingAggreateList = await _purchaseListData.GetPurchaseList(Id);
            if (existingAggreateList.Id != null)
            {
                await _purchaseListData.UpdatePurchaseList(aggregateList);
            }
            else
            {
                await _purchaseListData.AddPurchaseList(aggregateList);
            }
            return aggregateList;
        }

        public async Task<AggregateList> GetPurchaseList(string Id)
        {
            return await _purchaseListData.GetPurchaseList(Id);
        }

        public async Task<string> UpdatePurchaseList(AggregateList aggregateList)
        {
            return await _purchaseListData.UpdatePurchaseList(aggregateList);
        }

        public async Task<List<string>> GetAllPurchaseList()
        {
            return await _purchaseListData.GetAllPurchaseList();
        }

        public async Task<string> DeletePurchaseList(string id)
        {
            AggregateList existingAggregateList = await _purchaseListData.GetPurchaseList(id);
            if (existingAggregateList.Id == null)
            {
                throw new ArgumentException("Purchase list does not exist");
            }
            return await _purchaseListData.DeletePurchaseList(id);
        }

    }

    
}
