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
        static int CamMyHorse;
        static int CamCart;

        static int HorsePed;
        static bool horseIsLoaded = true;
        static bool MyhorseIsLoaded = false;

        static int lIndex;
        static int iIndex;

        static List<Menu> CompsMenus = new List<Menu>();

        #region HorseDataCache
        static string horsemodel;
        static double horsecost;

        static int indexHorseSelected;
        #endregion

        #region HorseCompsCache
        static int indexCategory;
        static int indexSubCategory;

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

        public static async Task MyHorseMode(int stableId, int myHorseId)
        {
            await EnterBuyMode();

            float Camerax = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][0].ToString());
            float Cameray = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][1].ToString());
            float Cameraz = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][2].ToString());
            float CameraRotx = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][3].ToString());
            float CameraRoty = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][4].ToString());
            float CameraRotz = float.Parse(GetConfig.Config["Stables"][stableId]["CamHorseGear"][5].ToString());

            await LoadMyHorsePreview(stableId, myHorseId);

            CamMyHorse = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", Camerax, Cameray, Cameraz, CameraRotx, CameraRoty, CameraRotz, 70.00f, false, 0);
            SetCamActive(CamMyHorse, true);
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

        public static async Task ExitMyHorseMode()
        {
            if (!MyhorseIsLoaded)
            {
                await ExitBuyMode();
                DeletePed(ref HorsePed);
                SetCamActive(CamMyHorse, false);
                RenderScriptCams(false, true, 1000, true, true, 0);
                DestroyCam(CamMyHorse, true);
            }
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

            Menu subMenuComplementsHorse = new Menu(GetConfig.Langs["SubMenuBuyComplements"], "");
            MenuController.AddSubmenu(subMenuManagmentHorse, subMenuComplementsHorse);

            MenuController.BindMenuItem(subMenuManagmentHorse, subMenuComplementsHorse, buttonBuyComplements);


            //Menu Confirmar
            Menu subMenuConfirmBuyComp = new Menu("Confirm Purcharse", "");
            MenuController.AddSubmenu(subMenuManagmentHorse, subMenuConfirmBuyComp);

            MenuItem buttonConfirmCompYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuConfirmBuyComp.AddMenuItem(buttonConfirmCompYes);
            MenuItem buttonConfirmCompNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.ARROW_LEFT
            };
            subMenuConfirmBuyComp.AddMenuItem(buttonConfirmCompNo);





            // Repetir por cada categoria mañana cuando te levantes el pene
            var compMantas = GetConfig.CompsLists.ElementAt(0);
            Menu subMenuCatComplementsHorseMantas = new Menu(compMantas.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseMantas);

            MenuItem buttonBuyComplementsCatMantas = new MenuItem(compMantas.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatMantas);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseMantas, buttonBuyComplementsCatMantas);

            foreach (var cat in compMantas.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesMantas = new MenuListItem(cat.Key, clist, 0, compMantas.Key + " - " + cat.Key);
                subMenuCatComplementsHorseMantas.AddMenuItem(compCategoriesMantas);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesMantas);
            }

            // Repetir por cada categoria mañana cuando te levantes el pene
            var compCuernos = GetConfig.CompsLists.ElementAt(1);
            Menu subMenuCatComplementsHorseCuernos = new Menu(compCuernos.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseCuernos);

            MenuItem buttonBuyComplementsCatCuernos = new MenuItem(compCuernos.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatCuernos);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseCuernos, buttonBuyComplementsCatCuernos);

            foreach (var cat in compCuernos.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesCuernos = new MenuListItem(cat.Key, clist, 0, compCuernos.Key + " - " + cat.Key);
                subMenuCatComplementsHorseCuernos.AddMenuItem(compCategoriesCuernos);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesCuernos);
            }


            // Repetir por cada categoria mañana cuando te levantes el pene
            var compAlforjas = GetConfig.CompsLists.ElementAt(2);
            Menu subMenuCatComplementsHorseAlforjas = new Menu(compAlforjas.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseAlforjas);

            MenuItem buttonBuyComplementsCatAlforjas = new MenuItem(compAlforjas.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatAlforjas);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseAlforjas, buttonBuyComplementsCatAlforjas);

            foreach (var cat in compAlforjas.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesAlforjas = new MenuListItem(cat.Key, clist, 0, compAlforjas.Key + " - " + cat.Key);
                subMenuCatComplementsHorseAlforjas.AddMenuItem(compCategoriesAlforjas);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesAlforjas);
            }



            // Repetir por cada categoria mañana cuando te levantes el pene
            var compColas = GetConfig.CompsLists.ElementAt(3);
            Menu subMenuCatComplementsHorseColas = new Menu(compColas.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseColas);

            MenuItem buttonBuyComplementsCatColas = new MenuItem(compColas.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatColas);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseColas, buttonBuyComplementsCatColas);

            foreach (var cat in compColas.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesColas = new MenuListItem(cat.Key, clist, 0, compColas.Key + " - " + cat.Key);
                subMenuCatComplementsHorseColas.AddMenuItem(compCategoriesColas);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesColas);
            }



            // Repetir por cada categoria mañana cuando te levantes el pene
            var compCrines = GetConfig.CompsLists.ElementAt(4);
            Menu subMenuCatComplementsHorseCrines = new Menu(compCrines.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseCrines);

            MenuItem buttonBuyComplementsCatCrines = new MenuItem(compCrines.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatCrines);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseCrines, buttonBuyComplementsCatCrines);

            foreach (var cat in compCrines.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesCrines = new MenuListItem(cat.Key, clist, 0, compCrines.Key + " - " + cat.Key);
                subMenuCatComplementsHorseCrines.AddMenuItem(compCategoriesCrines);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesCrines);
            }



            // Repetir por cada categoria mañana cuando te levantes el pene
            var compMonturas = GetConfig.CompsLists.ElementAt(4);
            Menu subMenuCatComplementsHorseMonturas = new Menu(compMonturas.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseMonturas);

            MenuItem buttonBuyComplementsCatMonturas = new MenuItem(compMonturas.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatMonturas);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseMonturas, buttonBuyComplementsCatMonturas);

            foreach (var cat in compMonturas.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesMonturas = new MenuListItem(cat.Key, clist, 0, compMonturas.Key + " - " + cat.Key);
                subMenuCatComplementsHorseMonturas.AddMenuItem(compCategoriesMonturas);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesMonturas);
            }



            // Repetir por cada categoria mañana cuando te levantes el pene
            var compEstribos = GetConfig.CompsLists.ElementAt(5);
            Menu subMenuCatComplementsHorseEstribos = new Menu(compEstribos.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseEstribos);

            MenuItem buttonBuyComplementsCatEstribos = new MenuItem(compEstribos.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatEstribos);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseEstribos, buttonBuyComplementsCatEstribos);

            foreach (var cat in compEstribos.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesEstribos = new MenuListItem(cat.Key, clist, 0, compEstribos.Key + " - " + cat.Key);
                subMenuCatComplementsHorseEstribos.AddMenuItem(compCategoriesEstribos);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesEstribos);
            }



            // Repetir por cada categoria mañana cuando te levantes el pene
            var compPetates = GetConfig.CompsLists.ElementAt(6);
            Menu subMenuCatComplementsHorsePetates = new Menu(compPetates.Key, "");
            MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorsePetates);

            MenuItem buttonBuyComplementsCatPetates = new MenuItem(compPetates.Key, "")
            {
                RightIcon = MenuItem.Icon.SADDLE
            };
            subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatPetates);
            MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorsePetates, buttonBuyComplementsCatPetates);

            foreach (var cat in compPetates.Value)
            {
                List<string> clist = new List<string>();

                for (int i = 0; i < cat.Value.Count(); i++)
                {
                    clist.Add($"# {(i + 1).ToString()}");
                }

                MenuListItem compCategoriesPetates = new MenuListItem(cat.Key, clist, 0, compPetates.Key + " - " + cat.Key);
                subMenuCatComplementsHorsePetates.AddMenuItem(compCategoriesPetates);
                MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesPetates);
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

            subMenuManagmentHorse.OnMenuClose += (_menu) =>
            {
                ExitMyHorseMode();
            };

            subMenuComplementsHorse.OnMenuClose += (_menu) =>
            {
                MyhorseIsLoaded = false;
            };

            subMenuHorses.OnItemSelect += (_menu, _item, _index) =>
            {
                indexHorseSelected = _index;
                MyHorseMode(stableId, _index);
                subMenuManagmentHorse.MenuTitle = HorseManagment.MyHorses[_index].getHorseName();

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
                        MyhorseIsLoaded = true;
                        break;
                    case 1:
                        HorseManagment.MyHorses[indexHorseSelected].setDefault(true);
                        MenuController.CloseAllMenus();
                        break;
                }
            };

            #endregion

            #region EventsBuyComplementsHorses

            subMenuComplementsHorse.OnItemSelect += (_menu, _item, _index) =>
            {
                indexCategory = _index;
                Debug.WriteLine(_index.ToString());
            };

            subMenuCatComplementsHorseMantas.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseMantas.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseMantas.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };

            subMenuCatComplementsHorseCuernos.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseCuernos.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseCuernos.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };


            subMenuCatComplementsHorseAlforjas.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseAlforjas.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseAlforjas.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };


            subMenuCatComplementsHorseColas.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseColas.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseColas.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };


            subMenuCatComplementsHorseCrines.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseCrines.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseCrines.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };

            subMenuCatComplementsHorseMonturas.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseMonturas.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseMonturas.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };



            subMenuCatComplementsHorseEstribos.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorseEstribos.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorseEstribos.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
            };


            subMenuCatComplementsHorsePetates.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
            };

            subMenuCatComplementsHorsePetates.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                MenuListItem itemlist = (MenuListItem)_newItem;
                Debug.WriteLine(itemlist.ListIndex.ToString());

                await LoadHorseCompsPreview(indexCategory, itemlist.Index, itemlist.ListIndex);
            };

            subMenuCatComplementsHorsePetates.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");

                await LoadHorseCompsPreview(indexCategory, _itemIndex, _newIndex);
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

        public static async Task LoadHorseCompsPreview(int cat, int subcat, int index)
        {
            uint hcomp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index];

            Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, hcomp, true, true, true);
        }

        public static async Task LoadMyHorsePreview(int stID, int index)
        {
            if (!MyhorseIsLoaded) {
                float x = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][0].ToString());
                float y = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][1].ToString());
                float z = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][2].ToString());
                float heading = float.Parse(GetConfig.Config["Stables"][stID]["SpawnHorse"][3].ToString());

                uint hashPed = (uint)GetHashKey(HorseManagment.MyHorses[index].getHorseModel());

                await InitStables.LoadModel(hashPed);

                HorsePed = CreatePed(hashPed, x, y, z, heading, false, true, true, true);
                Function.Call((Hash)0x283978A15512B2FE, HorsePed, true);
                SetEntityCanBeDamaged(HorsePed, false);
                SetEntityInvincible(HorsePed, true);
                FreezeEntityPosition(HorsePed, true);
                SetModelAsNoLongerNeeded(hashPed);
            }


        }

    }
}
