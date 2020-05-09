using CitizenFX.Core;
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
    public class StablesBuy: BaseScript
    {
        static int CamHorse;
        static int CamCart;

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

            SetCamActive(CamHorse, false);
            RenderScriptCams(false, true, 1000, true, true, 0);
            DestroyCam(CamHorse, true);
        }

        public static void MenuStables(int stableId)
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

            foreach (JObject categories in GetConfig.Config["Horses"].Children<JObject>())
            {
                foreach (JProperty cat in categories.Properties())
                {
                    Debug.WriteLine(cat.Name); //Titulo
                }
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
            };

            subMenuBuyHorses.OnMenuClose += (_menu) =>
            {
                ExitBuyHorseMode();
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

            menuStables.OpenMenu();
        }

    }
}
