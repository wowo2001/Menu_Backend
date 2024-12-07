namespace Menu.Models
{
    public class AggregateList
    {

        public string Id { get; set; }
        public List<IngredientPurchase> AllIngredientList { get; set; }

        public AggregateList() {
            AllIngredientList = new List<IngredientPurchase>();
        }
    }
}
