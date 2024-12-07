using System.Xml.Linq;

namespace Menu.Models
{
    public class WeeklyChoice
    {
        public string Id { get; set; }
        public List<DailyChoice> MyChoice { get; set; }

        public WeeklyChoice()
        {
            MyChoice = new List<DailyChoice>();
        }

    }

    public class DailyChoice
    {
        public string Day { get; set; }
        public List<string> Dish { get; set; }

        public DailyChoice()
        {
            Dish = new List<string>();
        }
    }
}

