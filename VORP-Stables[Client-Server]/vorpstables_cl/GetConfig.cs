using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_cl
{
    class GetConfig : BaseScript
    {
        public static JObject Config = new JObject();
        public static Dictionary<string, string> Langs = new Dictionary<string, string>();
        public static Dictionary<string, Dictionary<string, double>> HorseLists = new Dictionary<string, Dictionary<string, double>>();

        public GetConfig()
        {
            EventHandlers[$"{API.GetCurrentResourceName()}:SendConfig"] += new Action<string, ExpandoObject>(LoadDefaultConfig);
            TriggerServerEvent($"{API.GetCurrentResourceName()}:getConfig");
        }

        private void LoadDefaultConfig(string dc, ExpandoObject dl)
        {

            Config = JObject.Parse(dc);

            foreach (var l in dl)
            {
                Langs[l.Key] = l.Value.ToString();
            }


            foreach (JObject categories in GetConfig.Config["Horses"].Children<JObject>())
            {
                foreach (JProperty cat in categories.Properties())
                {
                    Debug.WriteLine(cat.Name); //Titulo

                    Dictionary<string, double> hlist = new Dictionary<string, double>();

                    foreach (JObject horse in cat.Value.Children<JObject>())
                    {

                        foreach (JProperty h in horse.Properties())
                        {
                            hlist.Add(h.Name, double.Parse(h.Value.ToString()));
                        }

                    }

                    HorseLists.Add(cat.Name, hlist);
                }
            }

            //Start When finish Loading
            InitStables.StartInit();
        }
    }
}
