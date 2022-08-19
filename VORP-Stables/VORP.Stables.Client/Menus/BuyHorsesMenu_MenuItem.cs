using CitizenFX.Core;
using MenuAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VORP.Stables.Client.Menus
{
    class BuyHorsesMenu_MenuItem
    {
        private static Menu buyHorsesMenu = new Menu(GetConfig.Langs["TitleMenuBuyHorses"], GetConfig.Langs["SubTitleMenuBuyHorses"]);
        private static Menu subMenuConfirmBuy = new Menu(GetConfig.Langs["SubMenuConfirmBuy"], "");
        private static bool setupDone = false;
        private static bool subMenuConfirmBuyCreated = false;
        private static bool subMenuConfirmBuyFinished = false;
        private static void SetupMenu()
        {
            if (setupDone) return;
            setupDone = true;

            
            MenuController.AddMenu(buyHorsesMenu);

            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.MenuToggleKey = (Control)0;

            foreach (var cat in GetConfig.HorseLists)
            {
                AddSubMenu(buyHorsesMenu, GetSubMenu(cat), cat.Key, GetConfig.Langs["SubMenuDescription"]);
            }
        }

        private static void SetupSubMenu(Menu mainMenu, KeyValuePair<string, Dictionary<string, double>> horse_cat)
        {
            MenuController.AddMenu(mainMenu);

            MenuController.EnableMenuToggleKeyOnController = false;
            MenuController.MenuToggleKey = (Control)0;

            #region MenuConfirm
            MenuItem buttonConfirmYes = new MenuItem("", GetConfig.Langs["ConfirmBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.SADDLE,
                ItemData = "buttonConfirmYes"
            };

            MenuItem buttonConfirmNo = new MenuItem(GetConfig.Langs["CancelBuyButton"], GetConfig.Langs["CancelBuyButtonDesc"])
            {
                RightIcon = MenuItem.Icon.ARROW_LEFT,
                ItemData = "buttonConfirmNo"
            };

            if (!subMenuConfirmBuyCreated)
            {
                MenuController.AddSubmenu(mainMenu, subMenuConfirmBuy);

                
                subMenuConfirmBuy.AddMenuItem(buttonConfirmYes);
                
                subMenuConfirmBuy.AddMenuItem(buttonConfirmNo);

                subMenuConfirmBuyCreated = true;
            }
            #endregion

            foreach (var h in horse_cat.Value)
            {
                string price = string.Format(GetConfig.Langs["HorseItemPrice"], h.Value);

                MenuItem horseItem = new MenuItem(GetConfig.Langs[h.Key], price)
                {
                    RightIcon = MenuItem.Icon.ARROW_RIGHT
                };

                mainMenu.AddMenuItem(horseItem);
                MenuController.BindMenuItem(mainMenu, subMenuConfirmBuy, horseItem);
            }

            //Events
            #region OnMenuOpen
            mainMenu.OnMenuOpen += (_menu) =>
            {
                StablesShop.BuyHorseMode();
                StablesShop.LoadHorsePreview(_menu.ParentMenu.GetCurrentMenuItem().Index, _menu.CurrentIndex, StablesShop.HorsePed);
            };
            #endregion

            #region OnMenuClose
            mainMenu.OnMenuClose += (_menu) =>
            {
                StablesShop.ExitBuyHorseMode();
            };
            #endregion

            #region subMenu OnItemSelect
            subMenuConfirmBuy.OnItemSelect += async (_menu, _item, _index) =>
            {
                if (subMenuConfirmBuyFinished) return;
                subMenuConfirmBuyFinished = true;

                Debug.WriteLine($"OnItemSelect: [{_menu}, {_item}, {_index}]");

                if (_index == 0)
                {
                    StablesShop.ConfirmBuyHorse(subMenuConfirmBuy.MenuTitle);
                }
                else
                {
                    subMenuConfirmBuy.CloseMenu();
                }

            };
            #endregion

            #region OnItemSelect
            mainMenu.OnItemSelect += (_menu, _menuItem, _itemIndex) =>
            {
                Debug.WriteLine($"OnListItemSelect: [{_menu}, {_menuItem}, {_itemIndex}]");
                StablesShop.iIndex = _itemIndex;
                subMenuConfirmBuy.MenuTitle = _menu.ParentMenu.GetCurrentMenuItem().Text;
                subMenuConfirmBuy.MenuSubtitle = string.Format(GetConfig.Langs["subTitleConfirmBuy"], GetConfig.Langs[GetConfig.HorseLists.ElementAt(_menu.ParentMenu.GetCurrentMenuItem().Index).Value.ElementAt(_itemIndex).Key], GetConfig.HorseLists.ElementAt(_menu.ParentMenu.GetCurrentMenuItem().Index).Value.ElementAt(_itemIndex).Value.ToString());
                
                foreach (var _itemInMenu in subMenuConfirmBuy.GetMenuItems())
                {
                    if (_itemInMenu.ItemData == "buttonConfirmYes")
                    {
                        _itemInMenu.Label = string.Format(GetConfig.Langs["ConfirmBuyButton"], GetConfig.HorseLists.ElementAt(_menu.ParentMenu.GetCurrentMenuItem().Index).Value.ElementAt(_itemIndex).Value.ToString());
                    }
                }

                StablesShop.horsecost = GetConfig.HorseLists.ElementAt(_menu.ParentMenu.GetCurrentMenuItem().Index).Value.ElementAt(_itemIndex).Value;
                StablesShop.horsemodel = GetConfig.HorseLists.ElementAt(_menu.ParentMenu.GetCurrentMenuItem().Index).Value.ElementAt(_itemIndex).Key;
            };
            #endregion

            #region OnIndexChange
            mainMenu.OnIndexChange += async (_menu, _oldItem, _newItem, _oldIndex, _newIndex) =>
            {
                Debug.WriteLine($"OnListIndexChange: [{_menu}, {_oldItem}, {_newItem}, {_oldIndex}, {_newIndex}]");
                if (StablesShop.horseIsLoaded)
                {
                    await StablesShop.LoadHorsePreview(_menu.ParentMenu.GetCurrentMenuItem().Index, _newIndex, StablesShop.HorsePed);
                }
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

        private static Menu GetSubMenu(KeyValuePair<string, Dictionary<string, double>> horse_cat)
        {
            Menu buyHorsesSubMenu = new Menu(horse_cat.Key, GetConfig.Langs["SubTitleMenuBuyHorses"]);
            SetupSubMenu(buyHorsesSubMenu, horse_cat);
            return buyHorsesSubMenu;
        }

        public static Menu GetMenu()
        {
            SetupMenu();
            return buyHorsesMenu;
        }

    }
}
