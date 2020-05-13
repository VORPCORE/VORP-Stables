using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_cl
{
    public class Horse
    {
        private string Name;
        private string HorseModel;

        private int XP;

        private JObject Status;

        private JObject Gear;

        private bool isDefault;

        public Horse(string name, string horseModel, int xP, string status, string jsonGear, bool isdefault)
        {
            Name = name;
            HorseModel = horseModel;
            XP = xP;
            Status = JObject.Parse(status);
            Gear = JObject.Parse(jsonGear);
            isDefault = isdefault;
        }

        public Horse() { }

        public string getHorseModel()
        {
            return HorseModel;
        }
        public string getHorseName()
        {
            return Name;
        }
        public JObject getGear()
        {
            return Gear;
        }
    }
}
