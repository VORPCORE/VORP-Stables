using CitizenFX.Core;
using MenuAPI;
using System;

namespace VORP.Stables.Client.Menus
{
    class MainMenu
    {
        private static void SetupMenu(Menu mainMenu, string CharJob)
        {
            MenuController.AddMenu(mainMenu);

            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.MenuToggleKey = (Control)0;

            #region Buy Horses Menu
            if (CharJob != null)
            {
                if ((CharJob == Convert.ToString(GetConfig.Config["JobForHorseDealer"]) || CharJob == Convert.ToString(GetConfig.Config["JobForHorseAndCarriagesDealer"]))
                    && !Convert.ToBoolean(GetConfig.Config["DisableBuyOptions"]))
                {
                    int MenuItemDesign = Convert.ToInt16(GetConfig.Config["MenuItemDesign"]);
                    switch (MenuItemDesign)
                    {
                        case 0:
                            AddSubMenu(mainMenu, BuyHorsesMenu.GetMenu(), GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
                            break;

                        case 1:
                            AddSubMenu(mainMenu, BuyHorsesMenu_MenuItem.GetMenu(), GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
                            break;
                    }
                }
            }

            else
            {
                int MenuItemDesign = Convert.ToInt16(GetConfig.Config["MenuItemDesign"]);
                switch (MenuItemDesign)
                {
                    case 0:
                        AddSubMenu(mainMenu, BuyHorsesMenu.GetMenu(), GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
                        break;

                    case 1:
                        AddSubMenu(mainMenu, BuyHorsesMenu_MenuItem.GetMenu(), GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
                        break;
                }
            }
            #endregion

            #region My Horses Menu
            AddSubMenu(mainMenu, MyHorsesMenu.GetMenu(), GetConfig.Langs["TitleMenuHorses"], GetConfig.Langs["SubTitleMenuHorses"]);
            #endregion

            #region Buy Carriages Menu

            if (CharJob != null)
            {
                if ((CharJob == Convert.ToString(GetConfig.Config["JobForCarriagesDealer"]) || CharJob == Convert.ToString(GetConfig.Config["JobForHorseAndCarriagesDealer"])) 
                    && !Convert.ToBoolean(GetConfig.Config["DisableBuyOptions"]))
                {
                    AddSubMenu(mainMenu, BuyCarriagesMenu.GetMenu(), GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"]);
                }
            }

            else
            {
                AddSubMenu(mainMenu, BuyCarriagesMenu.GetMenu(), GetConfig.Langs["TitleMenuBuyCarts"], GetConfig.Langs["SubTitleMenuBuyCarts"]);
            }
            #endregion

            #region My Carriages Menu
            AddSubMenu(mainMenu, MyCarriagesMenu.GetMenu(), GetConfig.Langs["TitleMenuCarts"], GetConfig.Langs["SubTitleMenuCarts"]);
            #endregion

            #region OnMenuOpen
            mainMenu.OnMenuOpen += (_menu) =>
            {

            };
            #endregion

            #region OnMenuClose
            mainMenu.OnMenuClose += (_menu) =>
            {

            };
            #endregion

        }

        private static void AddSubMenu(Menu mainMenu, Menu GetMenu, string Title, string SubTitle)
        {
            MenuController.AddSubmenu(mainMenu, GetMenu);

            MenuItem subMenuBuyHorses = new MenuItem(Title, SubTitle)
            {
                RightIcon = MenuItem.Icon.ARROW_RIGHT
            };

            mainMenu.AddMenuItem(subMenuBuyHorses);
            MenuController.BindMenuItem(mainMenu, GetMenu, subMenuBuyHorses);
        }

        public static Menu GetMenu(string CharJob)
        {
            Menu mainMenu = new Menu(GetConfig.Langs["TitleMenuStables"], GetConfig.Langs["SubTitleMenuStables"]);

            SetupMenu(mainMenu, CharJob);
            return mainMenu;
        }
    }
}
