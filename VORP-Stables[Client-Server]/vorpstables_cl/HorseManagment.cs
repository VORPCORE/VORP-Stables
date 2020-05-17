using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorpstables_cl
{
    public class HorseManagment : BaseScript
    {
        public static List<Horse> MyHorses = new List<Horse>();
        public static List<uint> MyComps = new List<uint>();
        public static Tuple<int, Horse> spawnedHorse;
        public static int MPTagHorse = 0;
        private int BrushPrompt;

        public HorseManagment()
        {
            EventHandlers["vorpstables:GetMyStables"] += new Action<List<dynamic>>(GetMyStables);
            EventHandlers["vorpstables:GetMyComplements"] += new Action<string>(GetMyComplements);
            TriggerServerEvent("vorpstables:LoadMyStables");
            //SetupBrushPrompt();
            Tick += onCallHorse;
            //Tick += onTagHorse; FEATURE WHEN RemoveMpGamerTag Works
        }

        private void GetMyComplements(string comps)
        {
            MyComps.Clear();
            JArray jComps = JArray.Parse(comps);
            foreach(var jc in jComps)
            {
                MyComps.Add(ConvertValue(jc.ToString()));
            }
        }

        [Tick]
        private async Task onCallHorse()
        {
            int pedAiming = new int();

            if (API.IsControlJustPressed(0, 0x24978A28))
            {
                CallHorse();
                await Delay(5000); // Anti Flood
            }

            if(API.GetEntityPlayerIsFreeAimingAt(API.PlayerId(), ref pedAiming) && API.IsDisabledControlJustPressed(0, 0x4216AF06))
            {
                FleeHorse(pedAiming);
                await Delay(1000);
            }

            if (API.GetEntityPlayerIsFreeAimingAt(API.PlayerId(), ref pedAiming) && API.IsDisabledControlJustPressed(0, 0x63A38F2C))
            {
                Function.Call((Hash)0x524B54361229154F, API.PlayerPedId(), API.GetHashKey("WORLD_HUMAN_HORSE_TEND_BRUSH_LINK"), 7000, true, 0, 0, false);
                await Delay(7000);
                API.ClearPedEnvDirt(pedAiming);
            }

        }

        private async Task FleeHorse(int ped)
        {

            if (ped == spawnedHorse.Item1)
            {
                Vector3 pedCoords = API.GetEntityCoords(API.PlayerPedId(), true, true);
                //Function.Call((Hash)0x94587F17E9C365D5, pedAiming, pedCoords.X + 10.0f, pedCoords.Y + 10.0f, pedCoords.Z, 1.0f, 10000, false, true); FEATURE
                Function.Call((Hash)0xA899B61C66F09134, ped, 0, 0);
                API.TaskGoToCoordAnyMeans(ped, pedCoords.X + 20.0f, pedCoords.Y + 20.0f, pedCoords.Z, 3.0F, 0, true, 0, 1.0f);
                await Delay(4000);
                API.DeletePed(ref ped);
            }
        }

        public void SetupBrushPrompt()
        {
            BrushPrompt = Function.Call<int>((Hash)0x04F97DE45A519419);
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", "Limpiar");
            Function.Call((Hash)0x5DD02A8318420DD7, BrushPrompt, str);
            Function.Call((Hash)0xB5352B7494A08258, BrushPrompt, 0x63A38F2C);
            Function.Call((Hash)0x8A0FB4D03A630D21, BrushPrompt, true);
            Function.Call((Hash)0x71215ACCFDE075EE, BrushPrompt, false);
            Function.Call((Hash)0x94073D5CA3F16B7B, BrushPrompt, true);
            Function.Call((Hash)0xF7AA2696A22AD8B9, BrushPrompt);
        }

        //FEATURE WHEN RemoveMpGamerTag Works
        //[Tick]
        //private async Task onTagHorse()
        //{
        //    await Delay(1000);
        //    if (spawnedHorse != null && API.DoesEntityExist(spawnedHorse.Item1))
        //    {
        //        if (getHorseDistance(spawnedHorse.Item1) < 15.0f && Function.Call<bool>((Hash)0xAAB0FE202E9FC9F0, spawnedHorse.Item1, -1))
        //        {
        //            if (API.IsMpGamerTagActive(MPTagHorse))
        //            {
        //                //Nada
        //            }
        //            else
        //            {
        //                MPTagHorse = Function.Call<int>((Hash)0x53CB4B502E1C57EA, spawnedHorse.Item1, spawnedHorse.Item2.getHorseName(), true, true);
        //            }
        //        }
        //        else
        //        {
        //            if (API.IsMpGamerTagActive(MPTagHorse))
        //            {
        //                Function.Call((Hash)0x839BFD7D7E49FE09, MPTagHorse);
        //            }
        //        }
        //    }

        //}

        private async Task CallHorse()
        {
            if (spawnedHorse != null)
            {
                if (API.DoesEntityExist(spawnedHorse.Item1) && spawnedHorse.Item1 != -1) // if spawned
                {
                    Debug.WriteLine(Function.Call<bool>((Hash)0xAAB0FE202E9FC9F0, spawnedHorse.Item1, -1).ToString());

                    if (Function.Call<bool>((Hash)0xAAB0FE202E9FC9F0, spawnedHorse.Item1, -1))
                    {
                        if (getHorseDistance(spawnedHorse.Item1) < 30.0f)
                        {
                            int pPID = API.PlayerPedId();
                            API.NetworkRequestControlOfEntity(spawnedHorse.Item1);
                            API.SetEntityAsMissionEntity(spawnedHorse.Item1, true, true);

                            Function.Call((Hash)0x6A071245EB0D1882, spawnedHorse.Item1, pPID, -1, 5.0F, 2.0F, 0F, 0);
                        }
                        else
                        {
                            API.NetworkRequestControlOfEntity(spawnedHorse.Item1);
                            API.SetEntityAsMissionEntity(spawnedHorse.Item1, true, true);
                            int hped = spawnedHorse.Item1;
                            API.DeletePed(ref hped);
                            await SpawnHorseDefault();
                        }
                    }
                    else
                    {
                        TriggerEvent("vorp:Tip", GetConfig.Langs["HorseIsOcuppied"], 2000);
                    }
                
                }
                else
                {
                    await SpawnHorseDefault();
                }
            }
            else
            {
                TriggerEvent("vorp:Tip", GetConfig.Langs["NoDefaultHorses"], 2000);
            }
        }

        private async Task SpawnHorseDefault()
        {
            Horse def = spawnedHorse.Item2;

            uint hashHorse = (uint)API.GetHashKey(def.getHorseModel());
            await InitStables.LoadModel(hashHorse);
            int pPID = API.PlayerPedId();
            Vector3 playerPos = API.GetEntityCoords(pPID, true, true);


            Vector3 spawnPos = Vector3.Zero;
            float spawnHeading = 0.0F;
            int unk1 = 0;

            API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, 25, ref spawnPos, ref spawnHeading, ref unk1, 0, 0f, 0f);

            int spawnedh = API.CreatePed(hashHorse, spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, true, true, false, false);

            Function.Call((Hash)0x283978A15512B2FE, spawnedh, true);
            Function.Call((Hash)0x23F74C2FDA6E7C61, -1230993421, spawnedh);
            Function.Call((Hash)0x6A071245EB0D1882, spawnedh, pPID, -1, 5.0F, 2.0F, 0F, 0);
            Function.Call((Hash)0x98EFA132A4117BE1, spawnedh, def.getHorseName());
            Function.Call((Hash)0x4A48B6E03BABB4AC, spawnedh, def.getHorseName());

            uint hashP = (uint)API.GetHashKey("PLAYER");
            Function.Call((Hash)0xADB3F206518799E8, spawnedh, hashP);
            Function.Call((Hash)0xCC97B29285B1DC3B, spawnedh, 1);

            JObject gear = def.getGear();
            uint blanket = ConvertValue(gear["blanket"].ToString());
            uint saddle = ConvertValue(gear["saddle"].ToString());
            uint mane = ConvertValue(gear["mane"].ToString());
            uint tail = ConvertValue(gear["tail"].ToString());
            uint bag = ConvertValue(gear["bag"].ToString());
            uint bedroll = ConvertValue(gear["bedroll"].ToString());
            uint stirrups = ConvertValue(gear["stirrups"].ToString());
            uint horn = ConvertValue(gear["horn"].ToString());

            if (blanket != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, blanket, true, true, true);
            if (saddle != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, saddle, true, true, true);
            if (mane != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, mane, true, true, true);
            if (tail != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, tail, true, true, true);
            if (bag != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, bag, true, true, true);
            if (bedroll != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, bedroll, true, true, true);
            if (stirrups != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, stirrups, true, true, true);
            if (horn != -1)
                Function.Call((Hash)0xD3A7B003ED343FD9, spawnedh, horn, true, true, true);


            //Create Tag
            MPTagHorse = Function.Call<int>((Hash)0x53CB4B502E1C57EA, spawnedh, def.getHorseName(), false, false, "", 0);

            spawnedHorse = new Tuple<int, Horse>(spawnedh, def);

            API.SetModelAsNoLongerNeeded(hashHorse);
        }

        public uint ConvertValue(string s)
        {
            uint result;

            if (uint.TryParse(s, out result))
            {
                return result;
            }
            else
            {
                int interesante = int.Parse(s);
                result = (uint)interesante;
                return result;
            }
        }

        public static void SetDefaultHorseDB(int id)
        {
            TriggerServerEvent("vorpstables:SetDefaultHorse", id);
        }

        public static void DeleteDefaultHorse(int ped)
        {
            API.DeletePed(ref ped);
        }

        private float getHorseDistance(int horsePed)
        {
            Vector3 playerCoords = API.GetEntityCoords(API.PlayerPedId(), true, true);
            Vector3 horseCoords = API.GetEntityCoords(horsePed, true, true);
            return API.GetDistanceBetweenCoords(playerCoords.X, playerCoords.Y, playerCoords.Z, horseCoords.X, horseCoords.Y, horseCoords.Z, false);
        }

        private void GetMyStables(List<dynamic> stableDB)
        {
            Debug.WriteLine("Establos " + stableDB.Count.ToString());

            MyHorses.Clear();

            foreach (dynamic horses in stableDB)
            {
                if (horses.type == "horse")
                {
                    Debug.WriteLine(horses.name);

                    bool isdefault = Convert.ToBoolean(horses.isDefault);

                    Horse _h = new Horse(horses.id, horses.name, horses.modelname, horses.xp, horses.status, horses.gear, isdefault);

                    MyHorses.Add(_h);

                    if (isdefault)
                    {
                        spawnedHorse = new Tuple<int, Horse>(-1, _h);
                    }

                }
            }
        }
    }
}
