using System.Xml.Linq;

namespace Menu.Models
{
    public class Choice
    {
        public string Id { get; set; }
        public List<MyChoice> MyChoice { get; set; }

    }

    public class MyChoice
    {
        public string Day { get; set; }
        public List<string> Dish { get; set; }
    }
}

