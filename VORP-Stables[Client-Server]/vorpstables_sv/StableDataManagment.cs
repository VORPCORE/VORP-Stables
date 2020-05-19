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
            EventHandlers["vorpstables:BuyNewComp"] += new Action<Player, string, double, string, int>(BuyNewComp);
            EventHandlers["vorpstables:UpdateComp"] += new Action<Player, string, int>(UpdateComp);
            EventHandlers["vorpstables:SetDefaultHorse"] += new Action<Player, int>(SetDefaultHorse);
        }

        private void UpdateComp([FromSource]Player source, string jgear, int horseId)
        {
            string sid = "steam:" + source.Identifiers["steam"];
            Exports["ghmattimysql"].execute("UPDATE stables SET gear=? WHERE identifier=? AND id=?", new object[] { jgear, sid, horseId });
            Delay(2200);
            Debug.WriteLine("UpdateComp");
            InitStables_Server IS = new InitStables_Server();
            IS.LoadStablesDB(source);

        }

        private void BuyNewComp([FromSource]Player source, string jcomps, double cost, string jgear, int horseId)
        {
            string sid = "steam:" + source.Identifiers["steam"];
            int _source = int.Parse(source.Handle);
            Debug.WriteLine("BuyNewComp");
            TriggerEvent("vorp:getCharacter", _source, new Action<dynamic>((user) =>
            {
                double money = user.money;


                if (cost <= money)
                {
                    TriggerEvent("vorp:removeMoney", _source, 0, cost);
                    Exports["ghmattimysql"].execute("SELECT * FROM horse_complements WHERE identifier=?", new[] { sid }, new Action<dynamic>((result) =>
                    {
                        if (result.Count == 0)
                        {
                            Exports["ghmattimysql"].execute("INSERT INTO horse_complements (`identifier`, `complements`) VALUES (?, ?)", new object[] { sid, jcomps });
                            Exports["ghmattimysql"].execute("UPDATE stables SET gear=? WHERE identifier=? AND id=?", new object[] { jgear, sid, horseId });
                        }
                        else
                        {
                            Exports["ghmattimysql"].execute("UPDATE horse_complements SET complements=? WHERE identifier=?", new object[] { jcomps, sid });
                            Exports["ghmattimysql"].execute("UPDATE stables SET gear=? WHERE identifier=? AND id=?", new object[] { jgear, sid, horseId });
                        }

                    }));
                    source.TriggerEvent("vorp:Tip", string.Format(LoadConfig.Langs["SuccessfulBuy"], cost), 4000);
                    ReLoadStables(source);
                }
                else
                {
                    source.TriggerEvent("vorp:Tip", LoadConfig.Langs["NoMoney"], 4000);

                }

            }));


        }

        private async Task ReLoadStables(Player source)
        {
            string sid = "steam:" + source.Identifiers["steam"];
            await Delay(4000);
            Exports["ghmattimysql"].execute("SELECT * FROM stables WHERE identifier=?", new[] { sid }, new Action<dynamic>((result) =>
            {
                if (result.Count == 0)
                {
                    Debug.WriteLine($"{source.Name} has no horses");
                }
                else
                {
                    List<dynamic> stable = new List<dynamic>();
                    foreach (var h in result)
                    {
                        stable.Add(h);
                    }

                    source.TriggerEvent("vorpstables:GetMyStables", stable);
                    Debug.WriteLine($"Loaded {result.Count} horses of {source.Name}");
                }

            }));

            Exports["ghmattimysql"].execute("SELECT * FROM horse_complements WHERE identifier=?", new[] { sid }, new Action<dynamic>((result) =>
            {
                if (result.Count == 0)
                {
                    Debug.WriteLine($"{source.Name} has no complements");
                }
                else
                {
                    string complements = result[0].complements;
                    source.TriggerEvent("vorpstables:GetMyComplements", complements);
                }
            }));
        }

        private void SetDefaultHorse([FromSource]Player source, int horseId)
        {
            string sid = "steam:" + source.Identifiers["steam"];

            Exports["ghmattimysql"].execute("UPDATE stables SET isDefault=0 WHERE identifier=? AND NOT id=?", new object[] { sid, horseId });
            Exports["ghmattimysql"].execute("UPDATE stables SET isDefault=1 WHERE identifier=? AND id=?", new object[] { sid, horseId });
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
                    Delay(2200);
                    InitStables_Server IS = new InitStables_Server();
                    IS.LoadStablesDB(source);
                }
                else
                {
                    source.TriggerEvent("vorp:Tip", LoadConfig.Langs["NoMoney"], 4000);

                }

            }));
        }
    }
}
