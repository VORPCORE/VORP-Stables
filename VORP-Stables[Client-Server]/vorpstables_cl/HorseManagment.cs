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
        public static List<Cart> MyCarts = new List<Cart>();
        public static List<uint> MyComps = new List<uint>();
        public static Tuple<int, Horse> spawnedHorse;
        public static Tuple<int, Cart> spawnedCart;
        public static int MPTagHorse = 0;
        private int BrushPrompt;

        public HorseManagment()
        {
            EventHandlers["vorpstables:GetMyStables"] += new Action<List<dynamic>>(GetMyStables);
            EventHandlers["vorpstables:GetMyComplements"] += new Action<string>(GetMyComplements);
            TriggerServerEvent("vorpstables:LoadMyStables");
            Tick += onCallHorse;
            Tick += onHorseDead;
            Tick += timeToRespawn;
        }

        [Tick]
        private async Task timeToRespawn()
        {
            for (int h = 0; h < MyHorses.Count(); h++)
            {
                if (MyHorses[h].getHorseDeadTime() > 0)
                {
                    MyHorses[h].setHorseDead(MyHorses[h].getHorseDeadTime() - 1);
                }
            }
            await Delay(1000);
        }

        [Tick]
        private async Task onHorseDead()
        {

            if (spawnedHorse == null || spawnedHorse.Item1 == -1) 
            {

            }
            else 
            {
                if (API.IsEntityDead(spawnedHorse.Item1) && spawnedHorse.Item2.getHorseDeadTime() == 0 && API.DoesEntityExist(spawnedHorse.Item1))
                {
                    int indexHorse = MyHorses.IndexOf(spawnedHorse.Item2);
                    spawnedHorse.Item2.setHorseDead(GetConfig.Config["SecondsToRespawn"].ToObject<int>());
                    MyHorses[indexHorse].setHorseDead(GetConfig.Config["SecondsToRespawn"].ToObject<int>());
                    TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["HorseDead"], spawnedHorse.Item2.getHorseName(), spawnedHorse.Item2.getHorseDeadTime()), 5000);
                    int pedHorse = spawnedHorse.Item1;
                    API.DeletePed(ref pedHorse);
                }
            }

        }

        public static void SetDefaultCartDB(int id)
        {
            TriggerServerEvent("vorpstables:SetDefaultCart", id);
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
            int pedAiming = 0;

            if (API.IsControlJustPressed(0, 0x24978A28))
            {
                CallHorse();
                await Delay(5000); // Anti Flood
            }

            if (API.IsControlJustPressed(0, 0xF3830D8E))
            {
                CallCart();
                await Delay(5000); // Anti Flood
            }

            if (API.GetEntityPlayerIsFreeAimingAt(API.PlayerId(), ref pedAiming) && API.IsDisabledControlJustPressed(0, 0x4216AF06))
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

        [Tick]
        private async Task onTagHorse()
        {
            await Delay(1000);
            if (spawnedHorse != null && API.DoesEntityExist(spawnedHorse.Item1))
            {
                if (getHorseDistance(spawnedHorse.Item1) < 15.0f && Function.Call<bool>((Hash)0xAAB0FE202E9FC9F0, spawnedHorse.Item1, -1))
                {
                    Function.Call((Hash)0x93171DDDAB274EB8, MPTagHorse, 2);
                }
                else
                {
                    if (API.IsMpGamerTagActive(MPTagHorse))
                    {
                        Function.Call((Hash)0x93171DDDAB274EB8, MPTagHorse, 0);
                    }
                }
            }

        }

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
                    if (spawnedHorse.Item2.getHorseDeadTime() > 0)
                    {
                        TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["HorseIsDead"], spawnedHorse.Item2.getHorseName(), spawnedHorse.Item2.getHorseDeadTime()), 5000);
                    }
                    else
                    {
                        await SpawnHorseDefault();
                    }
                    
                }
            }
            else
            {
                TriggerEvent("vorp:Tip", GetConfig.Langs["NoDefaultHorses"], 2000);
            }
        }

        private async Task CallCart()
        {
            if (spawnedCart != null)
            {
                if (API.DoesEntityExist(spawnedCart.Item1) && spawnedCart.Item1 != -1) // if spawned
                {
                    Debug.WriteLine(Function.Call<bool>((Hash)0xE052C1B1CAA4ECE4, spawnedCart.Item1, -1).ToString());

                    if (Function.Call<bool>((Hash)0xE052C1B1CAA4ECE4, spawnedCart.Item1, -1))
                    {
                        if (getHorseDistance(spawnedCart.Item1) < 30.0f)
                        {
                            int pPID = API.PlayerPedId();
                            API.NetworkRequestControlOfEntity(spawnedCart.Item1);
                            API.SetEntityAsMissionEntity(spawnedCart.Item1, true, true);

                            Function.Call((Hash)0x6A071245EB0D1882, spawnedCart.Item1, pPID, -1, 5.0F, 2.0F, 0F, 0);
                        }
                        else
                        {
                            API.NetworkRequestControlOfEntity(spawnedCart.Item1);
                            API.SetEntityAsMissionEntity(spawnedCart.Item1, true, true);
                            int hped = spawnedCart.Item1;
                            API.DeleteVehicle(ref hped);
                            await SpawnCartDefault();
                        }
                    }
                    else
                    {
                        TriggerEvent("vorp:Tip", GetConfig.Langs["CartIsOcuppied"], 2000);
                    }

                }
                else
                {
                    if (spawnedCart.Item2.getHorseDeadTime() > 0)
                    {
                        TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["CartIsDead"], spawnedCart.Item2.getHorseName(), spawnedCart.Item2.getHorseDeadTime().ToString()), 5000);
                    }
                    else
                    {
                        await SpawnCartDefault();
                    }

                }
            }
            else
            {
                TriggerEvent("vorp:Tip", GetConfig.Langs["NoDefaultCarts"], 2000);
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
            Vector3 spawnPos2 = Vector3.Zero;
            float spawnHeading = 0.0F;
            int unk1 = 0;

            //API.GetNthClosestVehicleNodeWithHeading(playerPos.X, playerPos.Y, playerPos.Z, 25, ref spawnPos, ref spawnHeading, ref unk1, 0, 0f, 0f);

            API.GetClosestRoad(playerPos.X + 10.0f, playerPos.Y + 10.0f, playerPos.Z, 0.0f, 25, ref spawnPos, ref spawnPos2, ref unk1, ref unk1, ref spawnHeading, true);

            int spawnedh = API.CreatePed(hashHorse, spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, true, true, false, false);

            Function.Call((Hash)0x283978A15512B2FE, spawnedh, true);
            Function.Call((Hash)0x23F74C2FDA6E7C61, -1230993421, spawnedh);
            Function.Call((Hash)0x6A071245EB0D1882, spawnedh, pPID, 4000, 5.0F, 2.0F, 0F, 0);
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

            //Function.Call((Hash)0x931B241409216C1F, API.PlayerPedId(), spawnedh, 0); //Brush

            API.SetModelAsNoLongerNeeded(hashHorse);
        }

        private async Task SpawnCartDefault()
        {

            Cart def = spawnedCart.Item2;

            uint hashCart = (uint)API.GetHashKey(def.getHorseModel());
            await InitStables.LoadModel(hashCart);
            int pPID = API.PlayerPedId();
            Vector3 playerPos = API.GetEntityCoords(pPID, true, true);


            Vector3 spawnPos = Vector3.Zero;
            Vector3 spawnPos2 = Vector3.Zero;
            float spawnHeading = 0.0F;
            int unk1 = 0;

            API.GetClosestRoad(playerPos.X + 10.0f, playerPos.Y + 10.0f, playerPos.Z, 0.0f, 25, ref spawnPos, ref spawnPos2, ref unk1, ref unk1, ref spawnHeading, true);

            int spawnedh = Function.Call<int>((Hash)0xAF35D0D2583051B0, hashCart, spawnPos.X, spawnPos.Y, spawnPos.Z, spawnHeading, true, true, false, true);

            SET_PED_DEFAULT_OUTFIT(spawnedh);

            //Function.Call((Hash)0x283978A15512B2FE, spawnedh, true);

            int attachedPed = Function.Call<int>((Hash)0x56D713888A566481, spawnedh);
            await Delay(1000);
            Debug.WriteLine(attachedPed.ToString());

            Function.Call((Hash)0x23F74C2FDA6E7C61, -1236452613, spawnedh);
            Function.Call((Hash)0x6A071245EB0D1882, spawnedh, pPID, 4000, 5.0F, 2.0F, 0F, 0);
            Function.Call((Hash)0x98EFA132A4117BE1, spawnedh, def.getHorseName());
            Function.Call((Hash)0x4A48B6E03BABB4AC, spawnedh, def.getHorseName());

            uint hashP = (uint)API.GetHashKey("PLAYER");
            Function.Call((Hash)0xADB3F206518799E8, spawnedh, hashP);
            Function.Call((Hash)0xCC97B29285B1DC3B, spawnedh, 1);

            spawnedCart = new Tuple<int, Cart>(spawnedh, def);

            //Function.Call((Hash)0x931B241409216C1F, API.PlayerPedId(), spawnedh, 0); //Brush

            API.SetModelAsNoLongerNeeded(hashCart);
        }

        public int SET_PED_DEFAULT_OUTFIT(int coach)
        {
            return Function.Call<int>((Hash)0xAF35D0D2583051B0, coach, true);
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
                int eresante = int.Parse(s);
                result = (uint)eresante;
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
            MyCarts.Clear();

            foreach (dynamic horses in stableDB)
            {
                if (horses.type == "horse")
                {
                    Debug.WriteLine(horses.name);

                    bool isdefault = Convert.ToBoolean(horses.isDefault);

                    Horse _h = new Horse(horses.id, horses.name, horses.modelname, horses.xp, horses.status, horses.gear, isdefault, false);

                    MyHorses.Add(_h);

                    if (isdefault)
                    {
                        spawnedHorse = new Tuple<int, Horse>(-1, _h);
                    }

                }
                else
                {
                    Debug.WriteLine(horses.name);

                    bool isdefault = Convert.ToBoolean(horses.isDefault);

                    Cart _h = new Cart(horses.id, horses.name, horses.modelname, horses.xp, horses.status, horses.gear, isdefault, false);

                    MyCarts.Add(_h);

                    if (isdefault)
                    {
                        spawnedCart = new Tuple<int, Cart>(-1, _h);
                    }
                }
            }
        }
    }
}
