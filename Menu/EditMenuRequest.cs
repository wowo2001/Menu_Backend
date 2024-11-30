namespace Menu
{
    public class EditMenuRequest
    {
        public string Name { get; set; }
        public List<Ingredient> Ingredient { get; set; }
        public string Category { get; set; }
    }

    public class Ingredient
    {
        public string Unit { get; set; }
        public string Name { get; set; }
        public string Amount { get; set; }
    }
}
