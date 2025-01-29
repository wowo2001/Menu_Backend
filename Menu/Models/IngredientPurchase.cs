namespace Menu.Models
{
    public class IngredientPurchase : Ingredient
    {
        public bool purchased { get; set; }

        public string location { get; set; }

        public string source { get; set; }
    }
}
