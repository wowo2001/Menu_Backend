namespace Menu.Models
{
    public class MenuDetails
    {
        public string Name { get; set; }
        public List<Ingredient> Ingredient { get; set; }
        public string Category { get; set; }

        public MenuDetails()
        {
            Ingredient = new List<Ingredient>();
        }
    }

    public class Ingredient
    {
        public string Unit { get; set; }
        public string Name { get; set; }
        public float Amount { get; set; }
    }
}
