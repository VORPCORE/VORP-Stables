using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_sv
{
    public class InitStables_Server : BaseScript
    {
        public InitStables_Server()
        {
            EventHandlers["vorpstables:LoadMyStables"] += new Action<Player>(LoadStablesDB);
        }

        public void LoadStablesDB([FromSource]Player source)
        {
            string sid = "steam:" + source.Identifiers["steam"];

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


    }
}
