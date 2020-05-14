using CitizenFX.Core;
using CitizenFX.Core.Native;
using MenuAPI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace vorpstables_cl
{
    public class StablesShop: BaseScript
    {
        static int CamHorse;
        static int CamCart;

        static int HorsePed;
        static bool horseIsLoaded = true;

        static int lIndex;
        static int iIndex;


        #region HorseDataCache
        static string horsemodel;
        static double horsecost;

        static int indexHorseSelected;
        #endregion

        public static async Task EnterBuyMode()
        {
            foreach (var v in GetActivePlayers())
            {
                int id = v + 1;
                int player = GetPlayerFromServerId(id);
                SetEntityAlpha(GetPlayerPed(player), 0, false);
                SetEntityNoCollisionEntity(PlayerPedId(), GetPlayerPed(player), false);
                Debug.WriteLine(id.ToString());
            }
            int pId = PlayerPedId();

            FreezeEntityPosition(pId, true);

        }

        public static async Task ExitBuyMode()
        {
            foreach (var v in GetActivePlayers())
            {
                int id = v + 1;
                int player = GetPlayerFromServerId(id);
                SetEntityAlpha(GetPlayerPed(player), 255, false);
                SetEntityNoCollisionEntity(PlayerPedId(), GetPlayerPed(player), true);
                Debug.WriteLine(id.ToString());
            }
            int pId = PlayerPedId();

            FreezeEntityPosition(pId, false);

        }

        public static async Task BuyHorseMode(int stableId)
        {
            await EnterBuyMode();

            float Camerax = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][0].ToString());
            float Cameray = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][1].ToString());
            float Cameraz = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][2].ToString());
            float CameraRotx = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][3].ToString());
            float CameraRoty = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][4].ToString());
            float CameraRotz = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorse"][5].ToString());

            CamHorse = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", Camerax, Cameray, Cameraz, CameraRotx, CameraRoty, CameraRotz, 50.00f, false, 0);
            SetCamActive(CamHorse, true);
            RenderScriptCams(true, true, 1000, true, true, 0);
        }

        public static async Task ExitBuyHorseMode()
        {
            await ExitBuyMode();
            DeletePed(ref HorsePed);
            SetCamActive(CamHorse, false);
            RenderScriptCams(false, true, 1000, true, true, 0);
            DestroyCam(CamHorse, true);
        }

        public static async Task MenuStables(int stableId)
        {
            
            Menu menuStables = new Menu(GetConfig.Langs["TitleMenuStables"], GetConfig.Langs["SubTitleMenuStables"]);
            MenuController.AddMenu(menuStables);

            #region SubMenuBuyHorses
            Menu subMenuBuyHorses = new Menu(GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
            MenuController.AddSubmenu(menuStables, subMenuBuyHorses);

            MenuItem menuButtonBuyHorses = new MenuItem(GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"])
            {
                RightIcon = MenuItem.Icon.ARROW_RIGHT
            };

            menuStables.AddMenuItem(menuButtonBuyHorses);
            MenuController.BindMenuItem(menuStables, subMenuBuyHorses, menuButtonBuyHorses);


            Menu subMenuConfirmBuy = new Menu("Confirm Purcharse", "");
            MenuController.AddSubmenu(subMenuBuyHorses, subMenuConfirmBuy);

            MenuItem buttonConfirmYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuConfirmBuy.AddMenuItem(buttonConfirmYes);
            MenuItem buttonConfirmNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.ARROW_LEFT
            };
            subMenuConfirmBuy.AddMenuItem(buttonConfirmNo);

            foreach (var cat in GetConfig.HorseLists)
            {
                List<string> hlist = new List<string>();

                foreach (var h in cat.Value)
                {
                    Debug.WriteLine(h.Key);
                    hlist.Add(GetConfig.Langs[h.Key]);
                }

                MenuListItem horseCategories = new MenuListItem(cat.Key, hlist, 0, "Horses");
                subMenuBuyHorses.AddMenuItem(horseCategories);
                MenuController.BindMenuItem(subMenuBuyHorses, subMenuConfirmBuy, horseCategories);
            }



            #endregion

            #region SubMenuHorses
            Menu subMenuHorses = new Menu(GetConfig.Langs["TitleMenuHorses"], GetConfig.Langs["SubTitleMenuHorses"]);
            MenuController.AddSubmenu(menuStables, subMenuHorses);

            MenuItem menuButtonHorses = new MenuItem(GetConfig.Langs["TitleMenuHorses"], GetConfig.Langs["SubTitleMenuHorses"])
            {
                RightIcon = MenuItem.Icon.ARROW_RIGHT
            };

            menuStables.AddMenuItem(menuButtonHorses);
            MenuController.BindMenuItem(menuStables, subMenuHorses, menuButtonHorses);

            Menu subMenuManagmentHorse = new Menu("Horse Name", "");
            MenuController.AddSubmenu(subMenuHorses, subMenuManagmentHorse);

            MenuItem buttonBuyComplements = new MenuItem(GetConfig.Langs["SubMenuBuyComplements"], GetConfig.Langs["SubMenuBuyComplements"])
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuManagmentHorse.AddMenuItem(buttonBuyComplements);

            MenuItem buttonSetDefaultHorse = new MenuItem(GetConfig.Langs["ButtonSetDefaultHorse"], GetConfig.Langs["ButtonSetDefaultHorse"])
            {
                RightIcon = MenuItem.Icon.TICK
            };
            subMenuManagmentHorse.AddMenuItem(buttonSetDefaultHorse);

            foreach (var mh in HorseManagment.MyHorses)
            {
                var Icon = MenuItem.Icon.SADDLE;

                if (mh.IsDefault())
                { 
                    Icon = MenuItem.Icon.TICK; 
                }

                MenuItem buttonMyHorses = new MenuItem(mh.getHorseName(), GetConfig.Langs[mh.getHorseModel()])
                {

                    RightIcon = Icon

                };
                subMenuHorses.AddMenuItem(buttonMyHorses);
                MenuController.BindMenuItem(subMenuHorses, subMenuManagmentHorse, buttonMyHorses);

            }

            #endregion

            #region SubMenuBuyCarts
            Menu subMenuBuyCarts = new Menu(GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"]);
            MenuController.AddSubmenu(menuStables, subMenuBuyCarts);

            MenuItem menuButtonBuyCarts = new MenuItem(GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"])
            {
                RightIcon = MenuItem.Icon.ARROW_RIGHT
            };

            menuStables.AddMenuItem(menuButtonBuyCarts);
            MenuController.BindMenuItem(menuStables, subMenuBuyCarts, menuButtonBuyCarts);
            #endregion

            #region SubMenuCarts
            Menu subMenuCarts = new Menu(GetConfig.Langs["TitleMenuCarts"], GetConfig.Langs["SubTitleMenuCarts"]);
            MenuController.AddSubmenu(menuStables, subMenuCarts);

            MenuItem menuButtonCarts = new MenuItem(GetConfig.Langs["TitleMenuCarts"], GetConfig.Langs["SubTitleMenuCarts"])
            {
                RightIcon = MenuItem.Icon.ARROW_RIGHT
            };

            menuStables.AddMenuItem(menuButtonCarts);
            MenuController.BindMenuItem(menuStables, subMenuCarts, menuButtonCarts);
            #endregion

            #region EventsBuyHorse
            subMenuBuyHorses.OnMenuOpen += (_menu) =>
            {
                BuyHorseMode(stableId);
                LoadHorsePreview(stableId, 0, 0, HorsePed);
            };

            subMenuBuyHorses.OnMenuClose += (_menu) =>
            {
                ExitBuyHorseMode();
            };

            subMenuConfirmBuy.OnItemSelect += async (_menu, _item, _index) =>
            {
                Debug.WriteLine($"OnItemSelect: [{_menu}, {_item}, {_index}]");

                if (_index == 0)
                {
                    if(HorseManagment.MyHorses.Count >= int.Parse(GetConfig.Config["StableSlots"].ToString()))
                    {
                        MenuController.CloseAllMenus();
                        TriggerEvent("vorp:Tip", GetConfig.Langs["StableIsFull"], 4000);
                    }
                    else
                    {
                        TriggerEvent("vorpinputs:getInput", GetConfig.Langs["InputNameButton"], GetConfig.Langs["InputNamePlaceholder"], new Action<dynamic>((cb) =>
                        {
                            Debug.WriteLine(cb);
                            string horseName = cb;
                            TriggerServerEvent("vorpstables:BuyNewHorse", horseName, subMenuConfirmBuy.MenuTitle, horsemodel, horsecost);

                            MenuController.CloseAllMenus();
                        }));
                    }
                }
                else
                {
                    subMenuConfirmBuy.CloseMenu();

                }

            };

            subMenuBuyHorses.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
                iIndex = _itemIndex;
                lIndex = _listIndex;
                subMenuConfirmBuy.MenuTitle = $"{GetConfig.HorseLists.ElementAt(_itemIndex).Key}";
                subMenuConfirmBuy.MenuSubtitle = string.Format(GetConfig.Langs["subTitleConfirmBuy"], GetConfig.Langs[GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Key],  GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value.ToString());
                buttonConfirmYes.Label = string.Format(GetConfig.Langs["ConfirmBuyButton"], GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value.ToString());

                horsecost = GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value;
                horsemodel = GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Key;
            };

            subMenuBuyHorses.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());
                if (horseIsLoaded)
                {
                    await LoadHorsePreview(stableId, itemlist.Index, itemlist.ListIndex, HorsePed);
                }
            };

            subMenuBuyHorses.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
                if (horseIsLoaded)
                {
                    await LoadHorsePreview(stableId, _itemIndex, _newIndex, HorsePed);
                }
            };

            #endregion

            #region EventsBuyCart
            subMenuBuyCarts.OnMenuOpen += (_menu) =>
            {

            };

            subMenuBuyCarts.OnMenuClose += (_menu) =>
            {

            };
            #endregion

            #region EventsManagHorses
            subMenuHorses.OnItemSelect += (_menu, _item, _index) =>
            {
                indexHorseSelected = _index;
                Debug.WriteLine(HorseManagment.MyHorses[_index].getHorseName());
                if (HorseManagment.MyHorses[_index].IsDefault())
                {
                    buttonSetDefaultHorse.Enabled = false;
                }
                else{
                    buttonSetDefaultHorse.Enabled = true;
                }
            };

            subMenuManagmentHorse.OnItemSelect += (_menu, _item, _index) =>
            {

                switch (_index)
                {
                    case 0:
                        
                        break;
                    case 1:
                        HorseManagment.MyHorses[indexHorseSelected].setDefault(true);
                        MenuController.CloseAllMenus();
                        break;
                }
            };

            #endregion

            menuStables.OpenMenu();
        }

        public static async Task LoadHorsePreview(int stID, int cat, int index, int ped2Delete)
        {
            horseIsLoaded = false;
            DeletePed(ref ped2Delete);
            float x = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][0].ToString());
            float y = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][1].ToString());
            float z = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][2].ToString());
            float heading = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][3].ToString());

            uint hashPed = (uint)GetHashKey(GetConfig.HorseLists.ElementAt(cat).Value.ElementAt(index).Key);

            await InitStables.LoadModel(hashPed);

            HorsePed = CreatePed(hashPed, x, y, z, heading, false, true, true, true);
            Function.Call((Hash)0x283978A15512B2FE, HorsePed, true);
            SetEntityCanBeDamaged(HorsePed, false);
            SetEntityInvincible(HorsePed, true);
            FreezeEntityPosition(HorsePed, true);
            SetModelAsNoLongerNeeded(hashPed);

            horseIsLoaded = true;
        }

    }
}
