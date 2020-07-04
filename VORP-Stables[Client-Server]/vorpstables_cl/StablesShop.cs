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
        public static int stableId;

        public static int CamHorse;
        public static int CamMyHorse;
        public static int CamCart;

        public static int HorsePed;
        public static int CartPed;
        public static bool horseIsLoaded = true;
        public static bool cartIsLoaded = true;
        public static bool MyhorseIsLoaded = false;
        public static bool MycartIsLoaded = false;

        public static int lIndex;
        public static int iIndex;

        public static int cIndex;

        #region HorseDataCache
        public static string horsemodel;
        public static double horsecost;

        public static int indexHorseSelected;
        public static int indexCartSelected;
        #endregion

        #region HorseCompsCache
        public static int indexCategory;
        public static int indexCategoryComp;
        public static int indexComp;
        #endregion

        public static async Task EnterBuyMode()
        {
            TriggerEvent("vorp:setInstancePlayer", true);
            int pId = PlayerPedId();

            FreezeEntityPosition(pId, true);

        }

        public static async Task ExitBuyMode()
        {
            TriggerEvent("vorp:setInstancePlayer", false);
            int pId = PlayerPedId();

            FreezeEntityPosition(pId, false);

        }

        public static async Task BuyHorseMode()
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

        public static async Task BuyCartMode()
        {
            await EnterBuyMode();

            float Camerax = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][0].ToString());
            float Cameray = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][1].ToString());
            float Cameraz = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][2].ToString());
            float CameraRotx = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][3].ToString());
            float CameraRoty = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][4].ToString());
            float CameraRotz = float.Parse(GetConfig.Config["Stables"][stableId]["CamCart"][5].ToString());

            CamHorse = CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", Camerax, Cameray, Cameraz, CameraRotx, CameraRoty, CameraRotz, 50.00f, false, 0);
            SetCamActive(CamHorse, true);
            RenderScriptCams(true, true, 1000, true, true, 0);
        }

        public static async Task MyHorseMode(int myHorseId)
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

        public static void DeleteMyHorse(int myHorseId)
        {
            TriggerEvent("vorpinputs:getInput", GetConfig.Langs["ButtonDeleteHorse"], GetConfig.Langs["DeleteConfirmation"], new Action<dynamic>(async (cb) =>
            {
                string horseName = cb;

                await Delay(1000);

                if (!horseName.Equals("close"))
                {
                    if (horseName.ToLower().Equals(HorseManagment.MyHorses[myHorseId].getHorseName().ToLower()))
                    {
                        TriggerServerEvent("vorpstables:RemoveHorse", HorseManagment.MyHorses[myHorseId].getHorseId());
                        if (HorseManagment.MyHorses[myHorseId].IsDefault())
                        {
                            HorseManagment.spawnedHorse = new Tuple<int, Horse>(-1, new Horse());
                        }
                        HorseManagment.MyHorses.RemoveAt(myHorseId);
                        TriggerEvent("vorp:TipRight", GetConfig.Langs["HorseDeleted"], 4000);
                    }
                    else
                    {
                        DeleteMyHorse(myHorseId);
                    }

                }
               
                
            }));

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

        public static async Task ExitMyCartMode()
        {
            if (!MycartIsLoaded)
            {
                await ExitBuyMode();
                DeleteVehicle(ref CartPed);
                SetCamActive(CamMyHorse, false);
                RenderScriptCams(false, true, 1000, true, true, 0);
                DestroyCam(CamMyHorse, true);
            }
        }

        public static async Task ConfirmBuyCarriage()
        {
            MenuController.CloseAllMenus();

            if (HorseManagment.MyCarts.Count >= int.Parse(GetConfig.Config["StableSlots"].ToString()))
            {
                TriggerEvent("vorp:TipRight", GetConfig.Langs["StableIsFull"], 4000);
            }
            else
            {
                TriggerEvent("vorpinputs:getInput", GetConfig.Langs["InputCartNameButton"], GetConfig.Langs["InputCartNamePlaceholder"], new Action<dynamic>(async (cb) =>
                {
                    string cartName = cb;

                    await Delay(1000);

                    if (cartName.Length < 3)
                    {
                        TriggerEvent("vorp:TipRight", "~e~" + GetConfig.Langs["PlaceHolderInputName"], 3000); // from client side
                        ConfirmBuyCarriage();
                    }
                    else
                    {
                        if (!cartName.Equals("close"))
                        {
                            TriggerServerEvent("vorpstables:BuyNewCart", cartName, GetConfig.CartLists.ElementAt(cIndex).Key, GetConfig.CartLists.ElementAt(cIndex).Value);
                        }
                    }

                }));
            }
        }

        public static async Task ConfirmBuyHorse(string tittle)
        {
            MenuController.CloseAllMenus();

            if (HorseManagment.MyHorses.Count >= int.Parse(GetConfig.Config["StableSlots"].ToString()))
            {
                TriggerEvent("vorp:TipRight", GetConfig.Langs["StableIsFull"], 4000);
            }
            else
            {
                TriggerEvent("vorpinputs:getInput", GetConfig.Langs["InputNameButton"], GetConfig.Langs["InputNamePlaceholder"], new Action<dynamic>(async (cb) =>
                {
                    string horseName = cb;

                    await Delay(1000);

                    if (horseName.Length < 3)
                    {

                         TriggerEvent("vorp:TipRight", "~e~" + GetConfig.Langs["PlaceHolderInputName"], 3000); // from client side
                         ConfirmBuyHorse(tittle);
                    }
                    else
                    {
                        if (!horseName.Equals("close"))
                        {
                            TriggerServerEvent("vorpstables:BuyNewHorse", horseName, tittle, horsemodel, horsecost);
                        }
                    }
                }));
            }
        }


        //public static async Task MenuStables(int sid)
        //{
        //    stableId = sid;
        //    Menus.MainMenu.GetMenu().OpenMenu();

        //    return;
        //    if (HorseManagment.spawnedHorse != null)
        //    {
        //        HorseManagment.DeleteDefaultHorse(HorseManagment.spawnedHorse.Item1);
        //    }
        //    MenuController.Menus.Clear();

        //    Menu menuStables = new Menu(GetConfig.Langs["TitleMenuStables"], GetConfig.Langs["SubTitleMenuStables"]);
        //    MenuController.AddMenu(menuStables);
        //    MenuController.MenuToggleKey = (Control)0;

        //    Debug.WriteLine($"{MenuController.MainMenu.MenuTitle}");
        //    #region SubMenuBuyHorses
        //    Menu subMenuBuyHorses = new Menu(GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
        //    MenuController.AddSubmenu(menuStables, subMenuBuyHorses);

        //    MenuItem menuButtonBuyHorses = new MenuItem(GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_RIGHT
        //    };

        //    menuStables.AddMenuItem(menuButtonBuyHorses);
        //    MenuController.BindMenuItem(menuStables, subMenuBuyHorses, menuButtonBuyHorses);


        //    Menu subMenuConfirmBuy = new Menu("Confirm Purcharse", "");
        //    MenuController.AddSubmenu(subMenuBuyHorses, subMenuConfirmBuy);

        //    MenuItem buttonConfirmYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuConfirmBuy.AddMenuItem(buttonConfirmYes);
        //    MenuItem buttonConfirmNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_LEFT
        //    };
        //    subMenuConfirmBuy.AddMenuItem(buttonConfirmNo);

        //    foreach (var cat in GetConfig.HorseLists)
        //    {
        //        List<string> hlist = new List<string>();

        //        foreach (var h in cat.Value)
        //        {
        //            hlist.Add(GetConfig.Langs[h.Key]);
        //        }

        //        MenuListItem horseCategories = new MenuListItem(cat.Key, hlist, 0, "Horses");
        //        subMenuBuyHorses.AddMenuItem(horseCategories);
        //        MenuController.BindMenuItem(subMenuBuyHorses, subMenuConfirmBuy, horseCategories);
        //    }



        //    #endregion

        //    #region SubMenuHorses
        //    Menu subMenuHorses = new Menu(GetConfig.Langs["TitleMenuHorses"], GetConfig.Langs["SubTitleMenuHorses"]);
        //    MenuController.AddSubmenu(menuStables, subMenuHorses);

        //    MenuItem menuButtonHorses = new MenuItem(GetConfig.Langs["TitleMenuHorses"], GetConfig.Langs["SubTitleMenuHorses"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_RIGHT
        //    };

        //    menuStables.AddMenuItem(menuButtonHorses);
        //    MenuController.BindMenuItem(menuStables, subMenuHorses, menuButtonHorses);

        //    Menu subMenuManagmentHorse = new Menu("Horse Name", "");
        //    MenuController.AddSubmenu(subMenuHorses, subMenuManagmentHorse);

        //    MenuItem buttonBuyComplements = new MenuItem(GetConfig.Langs["SubMenuBuyComplements"], GetConfig.Langs["SubMenuBuyComplements"])
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuManagmentHorse.AddMenuItem(buttonBuyComplements);

        //    MenuItem buttonSetDefaultHorse = new MenuItem(GetConfig.Langs["ButtonSetDefaultHorse"], GetConfig.Langs["ButtonSetDefaultHorse"])
        //    {
        //        RightIcon = MenuItem.Icon.TICK
        //    };
        //    subMenuManagmentHorse.AddMenuItem(buttonSetDefaultHorse);

        //    MenuItem buttonDeleteHorse = new MenuItem(GetConfig.Langs["ButtonDeletetHorse"], GetConfig.Langs["ButtonDeletetHorse"])
        //    {
        //        RightIcon = MenuItem.Icon.LOCK
        //    };
        //    subMenuManagmentHorse.AddMenuItem(buttonDeleteHorse);

        //    foreach (var mh in HorseManagment.MyHorses)
        //    {
        //        var Icon = MenuItem.Icon.SADDLE;

        //        if (mh.IsDefault())
        //        {
        //            Icon = MenuItem.Icon.TICK;
        //        }

        //        MenuItem buttonMyHorses = new MenuItem(mh.getHorseName(), GetConfig.Langs[mh.getHorseModel()])
        //        {

        //            RightIcon = Icon

        //        };
        //        subMenuHorses.AddMenuItem(buttonMyHorses);
        //        MenuController.BindMenuItem(subMenuHorses, subMenuManagmentHorse, buttonMyHorses);

        //    }

        //    Menu subMenuComplementsHorse = new Menu(GetConfig.Langs["SubMenuBuyComplements"], "");
        //    MenuController.AddSubmenu(subMenuManagmentHorse, subMenuComplementsHorse);

        //    MenuController.BindMenuItem(subMenuManagmentHorse, subMenuComplementsHorse, buttonBuyComplements);

        //    Menu subMenuConfirmBuyComp = new Menu("Confirm Purcharse", "");
        //    MenuController.AddSubmenu(subMenuManagmentHorse, subMenuConfirmBuyComp);

        //    MenuItem buttonConfirmCompYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuConfirmBuyComp.AddMenuItem(buttonConfirmCompYes);
        //    MenuItem buttonConfirmCompNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_LEFT
        //    };
        //    subMenuConfirmBuyComp.AddMenuItem(buttonConfirmCompNo);

        //    var compMantas = GetConfig.CompsLists.ElementAt(0);
        //    Menu subMenuCatComplementsHorseMantas = new Menu(compMantas.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseMantas);

        //    MenuItem buttonBuyComplementsCatMantas = new MenuItem(compMantas.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatMantas);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseMantas, buttonBuyComplementsCatMantas);

        //    foreach (var cat in compMantas.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesMantas = new MenuListItem(cat.Key, clist, 0, compMantas.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseMantas.AddMenuItem(compCategoriesMantas);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesMantas);
        //    }

        //    var compCuernos = GetConfig.CompsLists.ElementAt(1);
        //    Menu subMenuCatComplementsHorseCuernos = new Menu(compCuernos.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseCuernos);

        //    MenuItem buttonBuyComplementsCatCuernos = new MenuItem(compCuernos.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatCuernos);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseCuernos, buttonBuyComplementsCatCuernos);

        //    foreach (var cat in compCuernos.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesCuernos = new MenuListItem(cat.Key, clist, 0, compCuernos.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseCuernos.AddMenuItem(compCategoriesCuernos);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesCuernos);
        //    }

        //    var compAlforjas = GetConfig.CompsLists.ElementAt(2);
        //    Menu subMenuCatComplementsHorseAlforjas = new Menu(compAlforjas.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseAlforjas);

        //    MenuItem buttonBuyComplementsCatAlforjas = new MenuItem(compAlforjas.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatAlforjas);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseAlforjas, buttonBuyComplementsCatAlforjas);

        //    foreach (var cat in compAlforjas.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesAlforjas = new MenuListItem(cat.Key, clist, 0, compAlforjas.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseAlforjas.AddMenuItem(compCategoriesAlforjas);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesAlforjas);
        //    }


        //    var compColas = GetConfig.CompsLists.ElementAt(3);
        //    Menu subMenuCatComplementsHorseColas = new Menu(compColas.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseColas);

        //    MenuItem buttonBuyComplementsCatColas = new MenuItem(compColas.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatColas);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseColas, buttonBuyComplementsCatColas);

        //    foreach (var cat in compColas.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesColas = new MenuListItem(cat.Key, clist, 0, compColas.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseColas.AddMenuItem(compCategoriesColas);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesColas);
        //    }

        //    var compCrines = GetConfig.CompsLists.ElementAt(4);
        //    Menu subMenuCatComplementsHorseCrines = new Menu(compCrines.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseCrines);

        //    MenuItem buttonBuyComplementsCatCrines = new MenuItem(compCrines.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatCrines);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseCrines, buttonBuyComplementsCatCrines);

        //    foreach (var cat in compCrines.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesCrines = new MenuListItem(cat.Key, clist, 0, compCrines.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseCrines.AddMenuItem(compCategoriesCrines);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesCrines);
        //    }

        //    var compMonturas = GetConfig.CompsLists.ElementAt(5);
        //    Menu subMenuCatComplementsHorseMonturas = new Menu(compMonturas.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseMonturas);

        //    MenuItem buttonBuyComplementsCatMonturas = new MenuItem(compMonturas.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatMonturas);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseMonturas, buttonBuyComplementsCatMonturas);

        //    foreach (var cat in compMonturas.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesMonturas = new MenuListItem(cat.Key, clist, 0, compMonturas.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseMonturas.AddMenuItem(compCategoriesMonturas);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesMonturas);
        //    }

        //    var compEstribos = GetConfig.CompsLists.ElementAt(6);
        //    Menu subMenuCatComplementsHorseEstribos = new Menu(compEstribos.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorseEstribos);

        //    MenuItem buttonBuyComplementsCatEstribos = new MenuItem(compEstribos.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatEstribos);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorseEstribos, buttonBuyComplementsCatEstribos);

        //    foreach (var cat in compEstribos.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesEstribos = new MenuListItem(cat.Key, clist, 0, compEstribos.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorseEstribos.AddMenuItem(compCategoriesEstribos);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesEstribos);
        //    }



        //    // Repetir por cada categoria mañana cuando te levantes el pene
        //    var compPetates = GetConfig.CompsLists.ElementAt(7);
        //    Menu subMenuCatComplementsHorsePetates = new Menu(compPetates.Key, "");
        //    MenuController.AddSubmenu(subMenuComplementsHorse, subMenuCatComplementsHorsePetates);

        //    MenuItem buttonBuyComplementsCatPetates = new MenuItem(compPetates.Key, "")
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuComplementsHorse.AddMenuItem(buttonBuyComplementsCatPetates);
        //    MenuController.BindMenuItem(subMenuComplementsHorse, subMenuCatComplementsHorsePetates, buttonBuyComplementsCatPetates);

        //    foreach (var cat in compPetates.Value)
        //    {
        //        List<string> clist = new List<string>();

        //        for (int i = 0; i < cat.Value.Count(); i++)
        //        {
        //            clist.Add($"# {(i + 1).ToString()}");
        //        }

        //        MenuListItem compCategoriesPetates = new MenuListItem(cat.Key, clist, 0, compPetates.Key + " - " + cat.Key);
        //        subMenuCatComplementsHorsePetates.AddMenuItem(compCategoriesPetates);
        //        MenuController.BindMenuItem(subMenuComplementsHorse, subMenuConfirmBuyComp, compCategoriesPetates);
        //    }


        //    #endregion

        //    #region SubMenuBuyCarts
        //    Menu subMenuBuyCarts = new Menu(GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"]);
        //    MenuController.AddSubmenu(menuStables, subMenuBuyCarts);

        //    MenuItem menuButtonBuyCarts = new MenuItem(GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_RIGHT
        //    };

        //    menuStables.AddMenuItem(menuButtonBuyCarts);
        //    MenuController.BindMenuItem(menuStables, subMenuBuyCarts, menuButtonBuyCarts);

        //    Menu subMenuCartConfirmBuy = new Menu("Confirm Purcharse", "");
        //    MenuController.AddSubmenu(subMenuBuyHorses, subMenuCartConfirmBuy);

        //    MenuItem buttonCartConfirmYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.SADDLE
        //    };
        //    subMenuCartConfirmBuy.AddMenuItem(buttonCartConfirmYes);
        //    MenuItem buttonCartConfirmNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_LEFT
        //    };
        //    subMenuCartConfirmBuy.AddMenuItem(buttonCartConfirmNo);

        //    foreach (var cat in GetConfig.CartLists)
        //    {
        //        MenuItem _menuButton = new MenuItem(string.Format(GetConfig.Langs["ButtonCart"], GetConfig.Langs[cat.Key], cat.Value.ToString()), cat.Value.ToString())
        //        {
        //            RightIcon = MenuItem.Icon.ARROW_RIGHT
        //        };
        //        subMenuBuyCarts.AddMenuItem(_menuButton);
        //        MenuController.BindMenuItem(subMenuBuyCarts, subMenuCartConfirmBuy, _menuButton);
        //    }

        //    subMenuBuyCarts.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
        //    {
        //        Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
        //        if (cartIsLoaded)
        //        {
        //            await LoadCartPreview(stableId, _newIndex, CartPed);
        //        }
        //    };

        //    subMenuBuyCarts.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        buttonCartConfirmYes.Label = string.Format(GetConfig.Langs["ConfirmBuyButton"], GetConfig.CartLists.ElementAt(_index).Value.ToString());
        //        cIndex = _index;
        //    };

        //    subMenuCartConfirmBuy.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        if (_index == 0)
        //        {
        //            if (HorseManagment.MyCarts.Count >= int.Parse(GetConfig.Config["StableSlots"].ToString()))
        //            {
        //                MenuController.CloseAllMenus();
        //                TriggerEvent("vorp:Tip", GetConfig.Langs["StableIsFull"], 4000);
        //            }
        //            else
        //            {
        //                TriggerEvent("vorpinputs:getInput", GetConfig.Langs["InputCartNameButton"], GetConfig.Langs["InputCartNamePlaceholder"], new Action<dynamic>((cb) =>
        //                {
        //                    Debug.WriteLine(cb);
        //                    string cartName = cb;
        //                    TriggerServerEvent("vorpstables:BuyNewCart", cartName, GetConfig.CartLists.ElementAt(cIndex).Key, GetConfig.CartLists.ElementAt(cIndex).Value);
        //                    MenuController.CloseAllMenus();
        //                }));
        //            }
        //        }
        //        else
        //        {
        //            subMenuCartConfirmBuy.CloseMenu();
        //        }
        //    };

        //    subMenuBuyCarts.OnMenuOpen += (_menu) =>
        //    {
        //        BuyCartMode(stableId);
        //        LoadHorsePreview(stableId, 0, 0, HorsePed);
        //    };

        //    subMenuBuyCarts.OnMenuClose += (_menu) =>
        //    {
        //        ExitMyCartMode();
        //    };
        //    #endregion

        //    #region SubMenuCarts
        //    Menu subMenuCarts = new Menu(GetConfig.Langs["TitleMenuCarts"], GetConfig.Langs["SubTitleMenuCarts"]);
        //    MenuController.AddSubmenu(menuStables, subMenuCarts);

        //    MenuItem menuButtonCarts = new MenuItem(GetConfig.Langs["TitleMenuCarts"], GetConfig.Langs["SubTitleMenuCarts"])
        //    {
        //        RightIcon = MenuItem.Icon.ARROW_RIGHT
        //    };

        //    menuStables.AddMenuItem(menuButtonCarts);
        //    MenuController.BindMenuItem(menuStables, subMenuCarts, menuButtonCarts);

        //    Menu subMenuManagmentCarts = new Menu("Carts Name", "");
        //    MenuController.AddSubmenu(subMenuCarts, subMenuManagmentCarts);

        //    MenuItem buttonSetDefaultCarts = new MenuItem(GetConfig.Langs["ButtonSetDefaultHorse"], GetConfig.Langs["ButtonSetDefaultHorse"])
        //    {
        //        RightIcon = MenuItem.Icon.TICK
        //    };
        //    subMenuManagmentCarts.AddMenuItem(buttonSetDefaultCarts);

        //    foreach (var mh in HorseManagment.MyCarts)
        //    {
        //        var Icon = MenuItem.Icon.SADDLE;

        //        if (mh.IsDefault())
        //        {
        //            Icon = MenuItem.Icon.TICK;
        //        }

        //        MenuItem buttonMyCarts = new MenuItem(mh.getHorseName(), GetConfig.Langs[mh.getHorseModel()])
        //        {

        //            RightIcon = Icon

        //        };
        //        subMenuCarts.AddMenuItem(buttonMyCarts);
        //        MenuController.BindMenuItem(subMenuCarts, subMenuManagmentCarts, buttonMyCarts);
        //    }

        //    subMenuCarts.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        indexCartSelected = _index;
        //        subMenuManagmentCarts.MenuTitle = HorseManagment.MyCarts[_index].getHorseName();
        //    };

        //    subMenuManagmentCarts.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        switch (_index)
        //        {
        //            case 0:
        //                HorseManagment.MyCarts[indexCartSelected].setDefault(true);
        //                MenuController.CloseAllMenus();
        //                break;
        //        }
        //    };

        //    #endregion

        //    #region EventsBuyHorse
        //    subMenuBuyHorses.OnMenuOpen += (_menu) =>
        //    {
        //        BuyHorseMode(stableId);
        //        LoadHorsePreview(stableId, 0, 0, HorsePed);
        //    };

        //    subMenuBuyHorses.OnMenuClose += (_menu) =>
        //    {
        //        ExitBuyHorseMode();
        //    };

        //    subMenuConfirmBuy.OnItemSelect += async (_menu, _item, _index) =>
        //    {
        //        Debug.WriteLine($"OnItemSelect: [{_menu}, {_item}, {_index}]");

        //        if (_index == 0)
        //        {
        //            if (HorseManagment.MyHorses.Count >= int.Parse(GetConfig.Config["StableSlots"].ToString()))
        //            {
        //                MenuController.CloseAllMenus();
        //                TriggerEvent("vorp:Tip", GetConfig.Langs["StableIsFull"], 4000);
        //            }
        //            else
        //            {
        //                TriggerEvent("vorpinputs:getInput", GetConfig.Langs["InputNameButton"], GetConfig.Langs["InputNamePlaceholder"], new Action<dynamic>((cb) =>
        //                {
        //                    Debug.WriteLine(cb);
        //                    string horseName = cb;
        //                    TriggerServerEvent("vorpstables:BuyNewHorse", horseName, subMenuConfirmBuy.MenuTitle, horsemodel, horsecost);

        //                    MenuController.CloseAllMenus();
        //                }));
        //            }
        //        }
        //        else
        //        {
        //            subMenuConfirmBuy.CloseMenu();

        //        }

        //    };

        //    subMenuBuyHorses.OnListItemSelect += (_menu, _listItem, _listIndex, _itemIndex) =>
        //    {
        //        Debug.WriteLine($"OnListItemSelect: [{_menu}, {_listItem}, {_listIndex}, {_itemIndex}]");
        //        iIndex = _itemIndex;
        //        lIndex = _listIndex;
        //        subMenuConfirmBuy.MenuTitle = $"{GetConfig.HorseLists.ElementAt(_itemIndex).Key}";
        //        subMenuConfirmBuy.MenuSubtitle = string.Format(GetConfig.Langs["subTitleConfirmBuy"], GetConfig.Langs[GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Key], GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value.ToString());
        //        buttonConfirmYes.Label = string.Format(GetConfig.Langs["ConfirmBuyButton"], GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value.ToString());

        //        horsecost = GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Value;
        //        horsemodel = GetConfig.HorseLists.ElementAt(_itemIndex).Value.ElementAt(_listIndex).Key;
        //    };

        //    subMenuBuyHorses.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
        //    {
        //        Debug.WriteLine($"OnIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
        //        MenuListItem itemlist = (MenuListItem)_newItem;
        //        Debug.WriteLine(itemlist.ListIndex.ToString());
        //        if (horseIsLoaded)
        //        {
        //            await LoadHorsePreview(stableId, itemlist.Index, itemlist.ListIndex, HorsePed);
        //        }
        //    };

        //    subMenuBuyHorses.OnListIndexChange += async (_menu, _listItem, _oldIndex, _newIndex, _itemIndex) =>
        //    {
        //        Debug.WriteLine($"OnListIndexChange: [{_menu}, {_listItem}, {_oldIndex}, {_newIndex}, {_itemIndex}]");
        //        if (horseIsLoaded)
        //        {
        //            await LoadHorsePreview(stableId, _itemIndex, _newIndex, HorsePed);
        //        }
        //    };

        //    #endregion

        //    #region EventsManagHorses

        //    #endregion

        //    #region EventsBuyComplementsHorses

        //    subMenuComplementsHorse.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        indexCategory = _index;
        //        Debug.WriteLine(_index.ToString());
        //    };



        //    subMenuConfirmBuyComp.OnItemSelect += (_menu, _item, _index) =>
        //    {
        //        if(_index == 0)
        //        {
        //            BuyComp();
        //            Debug.WriteLine($"Key: {GetConfig.CompsLists.ElementAt(indexCategory).Value.ElementAt(indexCategoryComp).Key} | Price: {GetConfig.CompsPrices.ElementAt(indexCategory).Value.ElementAt(indexCategoryComp).Value.ToString()} | UintID: {GetConfig.CompsLists.ElementAt(indexCategory).Value.ElementAt(indexCategoryComp).Value[indexComp].ToString()}");
        //        }
        //    };

        //    #endregion
        //    menuStables.OpenMenu();
        //}

        public static async Task BuyComp()
        {
            JObject newGear = HorseManagment.MyHorses[indexHorseSelected].getGear();
            uint hashItem = GetConfig.CompsLists.ElementAt(indexCategory).Value.ElementAt(indexCategoryComp).Value[indexComp];
            
                    newGear["blanket"] = blanketsComp;

                    newGear["horn"] = hornsComp;

                    newGear["bag"] = saddlebagsComp;

                    newGear["tail"] = tailsComp;

                    newGear["mane"] = manesComp;

                    newGear["saddle"] = saddlesComp;

                    newGear["stirrups"] = stirrupsComp;

                    newGear["bedroll"] = bedrollsComp;
                    Debug.WriteLine(newGear.ToString());

            
            
            if (costComps != 0)
            {
                if (!HorseManagment.MyComps.Contains(blanketsComp) && blanketsComp != 0)
                {
                    HorseManagment.MyComps.Add(blanketsComp);
                }
                if (!HorseManagment.MyComps.Contains(hornsComp) && hornsComp != 0)
                {
                    HorseManagment.MyComps.Add(hornsComp);
                }
                if (!HorseManagment.MyComps.Contains(saddlebagsComp) && saddlebagsComp != 0)
                {
                    HorseManagment.MyComps.Add(saddlebagsComp);
                }
                if (!HorseManagment.MyComps.Contains(tailsComp) && tailsComp != 1 && tailsComp != 0)
                {
                    HorseManagment.MyComps.Add(tailsComp);
                }
                if (!HorseManagment.MyComps.Contains(manesComp) && manesComp != 1 && manesComp != 0)
                {
                    HorseManagment.MyComps.Add(manesComp);
                }
                if (!HorseManagment.MyComps.Contains(saddlesComp) && saddlesComp != 0)
                {
                    HorseManagment.MyComps.Add(saddlesComp);
                }
                if (!HorseManagment.MyComps.Contains(stirrupsComp) && stirrupsComp != 0)
                {
                    HorseManagment.MyComps.Add(stirrupsComp);
                }
                if (!HorseManagment.MyComps.Contains(bedrollsComp) && bedrollsComp != 0)
                {
                    HorseManagment.MyComps.Add(bedrollsComp);
                }
                
                string array = Newtonsoft.Json.JsonConvert.SerializeObject(HorseManagment.MyComps.ToArray());
                TriggerServerEvent("vorpstables:BuyNewComp", array, costComps, newGear.ToString(), HorseManagment.MyHorses[indexHorseSelected].getHorseId());
            }
            else
            {
                TriggerServerEvent("vorpstables:UpdateComp", newGear.ToString(), HorseManagment.MyHorses[indexHorseSelected].getHorseId());
            }

            ExitMyHorseMode();
            MenuController.CloseAllMenus();
        }

        public static async Task LoadHorsePreview(int cat, int index, int ped2Delete)
        {
            horseIsLoaded = false;
            DeletePed(ref ped2Delete);
            float x = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnHorse"][0].ToString());
            float y = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnHorse"][1].ToString());
            float z = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnHorse"][2].ToString());
            float heading = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnHorse"][3].ToString());

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

        public static async Task LoadCartPreview(int index, int veh2Delete)
        {
            cartIsLoaded = false;
            DeleteVehicle(ref veh2Delete);
            float x = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnCart"][0].ToString());
            float y = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnCart"][1].ToString());
            float z = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnCart"][2].ToString());
            float heading = float.Parse(GetConfig.Config["Stables"][stableId]["SpawnCart"][3].ToString());
            uint hashVeh = (uint)GetHashKey(GetConfig.CartLists.ElementAt(index).Key);

            await InitStables.LoadModel(hashVeh);

            CartPed = CreateVehicle(hashVeh, x, y, z, heading, false, true, true, true);
            Function.Call((Hash)0xAF35D0D2583051B0, CartPed, true);
            SetEntityCanBeDamaged(CartPed, false);
            SetEntityInvincible(CartPed, true);
            FreezeEntityPosition(CartPed, true);
            SetModelAsNoLongerNeeded(hashVeh);

            cartIsLoaded = true;
        }

        public static uint blanketsComp = 0;
        public static int blanketsCat = 0;
        public static uint hornsComp = 0;
        public static int hornsCat = 0;
        public static uint saddlebagsComp = 0;
        public static int saddlebagsCat = 0;
        public static uint tailsComp = 0;
        public static int tailsCat = 0;
        public static uint manesComp = 0;
        public static int manesCat = 0;
        public static uint saddlesComp = 0;
        public static int saddlesCat = 0;
        public static uint stirrupsComp = 0;
        public static int stirrupsCat = 0;
        public static uint bedrollsComp = 0;
        public static int bedrollsCat = 0;


        public static async Task LoadHorseCompsPreview(int cat, int subcat, int index)
        {
            Function.Call((Hash)0xC8A9481A01E63C28, HorsePed, 1);

            if (index == 0)
            {
                switch (cat)
                {
                    case 0:
                        blanketsComp = 0;
                        break;
                    case 1:
                        hornsComp = 0;
                        break;
                    case 2:
                        saddlebagsComp = 0;
                        break;
                    case 3:
                        tailsComp = 0;
                        break;
                    case 4:
                        manesComp = 0;
                        break;
                    case 5:
                        saddlesComp = 0;
                        break;
                    case 6:
                        stirrupsComp = 0;
                        break;
                    case 7:
                        bedrollsComp = 0;
                        break;
                }
                
            }
            else
            {
                
                switch (cat)
                {
                    case 0:
                        blanketsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        blanketsCat = subcat;
                        break;
                    case 1:
                        hornsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        hornsCat = subcat;
                        break;
                    case 2:
                        saddlebagsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        saddlebagsCat = subcat;
                        break;
                    case 3:
                        if(index == 1)
                        {
                            tailsComp = 1;
                            tailsCat = -1;
                        }
                        else
                        {
                            tailsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 2];
                            tailsCat = subcat;
                        }
                        break;
                    case 4:
                        if (index == 1)
                        {
                            manesComp = 1;
                            manesCat = -1;
                        }
                        else
                        {
                            manesComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 2];
                            manesCat = subcat;
                        }
                        break;
                    case 5:
                        saddlesComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        saddlesCat = subcat;
                        break;
                    case 6:
                        stirrupsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        stirrupsCat = subcat;
                        break;
                    case 7:
                        bedrollsComp = GetConfig.CompsLists.ElementAt(cat).Value.ElementAt(subcat).Value[index - 1];
                        bedrollsCat = subcat;
                        break;
                }
            }
            CalcPrice();
            await ReloadComps();
        }

        public static async Task ReloadComps() 
        {
            if (blanketsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, blanketsComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0x17CEB41A, 0);
            }

            if (hornsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, hornsComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0x5447332, 0);
            }

            if (saddlebagsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, saddlebagsComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0x80451C25, 0);
            }
            
            if (tailsComp == 1)
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0xA63CAE10, 0);
            }
            else if (tailsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, tailsComp, true, true, true);
            }

            if (manesComp == 1)
            {
                
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0xAA0217AB, 0);
            }
            else if (manesComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, manesComp, true, true, true);
            }

            if (saddlesComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, saddlesComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0xBAA7E618, 0);
            }
            
            if (stirrupsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, stirrupsComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0xDA6DADCA, 0);
            }

            if (bedrollsComp != 0)
            {
                Function.Call((Hash)0xD3A7B003ED343FD9, HorsePed, bedrollsComp, true, true, true);
            }
            else
            {
                Function.Call((Hash)0xD710A5007C2AC539, HorsePed, 0xEFB31921, 0);
            }            
            Function.Call((Hash)0xCC8CA3E88256E58F, HorsePed, 0, 1, 1, 1, 0);

        }

        public static double costComps;

        public static async Task CalcPrice()
        {
            costComps = 0;
            if (!HorseManagment.MyComps.Contains(blanketsComp) && blanketsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(0).Value.ElementAt(blanketsCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(hornsComp) && hornsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(1).Value.ElementAt(hornsCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(saddlebagsComp) && saddlebagsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(2).Value.ElementAt(saddlebagsCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(tailsComp) && tailsComp != 1 && tailsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(3).Value.ElementAt(tailsCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(manesComp) && manesComp != 1 && manesComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(4).Value.ElementAt(manesCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(saddlesComp) && saddlesComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(5).Value.ElementAt(saddlesCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(stirrupsComp) && stirrupsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(6).Value.ElementAt(stirrupsCat).Value;
            }
            if (!HorseManagment.MyComps.Contains(bedrollsComp) && bedrollsComp != 0)
            {
                costComps += GetConfig.CompsPrices.ElementAt(7).Value.ElementAt(bedrollsCat).Value;
            }
            Menus.BuyHorseCompsMenu.SetPriceButton(costComps);
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
