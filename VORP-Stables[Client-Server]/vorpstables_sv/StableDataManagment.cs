using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_sv
{
    public class StableDataManagment : BaseScript
    {
        public StableDataManagment()
        {
            EventHandlers["vorpstables:BuyNewHorse"] += new Action<Player, string, string, string, double>(BuyNewHorse);
        }

        private void BuyNewHorse([FromSource]Player source, string name, string race, string model, double cost)
        {
            int _source = int.Parse(source.Handle);

            string sid = "steam:" + source.Identifiers["steam"];

            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;


                if (cost <= money)
                {
                    TriggerEvent("vorp:removeMoney", _source, 0, cost);
                    Exports["ghmattimysql"].execute("INSERT INTO stables (`identifier`, `name`, `type`, `modelname`) VALUES (?, ?, ?, ?)", new object[] { sid, name, "horse", model });
                    source.TriggerEvent("vorp:Tip", string.Format(LoadConfig.Langs["SuccessfulBuy"], race, LoadConfig.Langs[model], cost.ToString()), 4000);
                }
                else
                {
                    source.TriggerEvent("vorp:Tip", LoadConfig.Langs["NoMoney"], 4000);

                }

            }));
        }
    }
}
