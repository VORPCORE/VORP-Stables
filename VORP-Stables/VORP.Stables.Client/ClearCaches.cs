using CitizenFX.Core;
using System;
using static CitizenFX.Core.Native.API;

namespace VORP.Stables.Client
{
    public class ClearCaches : BaseScript
    {
        public ClearCaches()
        {
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;
            int horseDef = HorseManagment.spawnedHorse.Item1;
            DeletePed(ref horseDef);

            Debug.WriteLine($"{resourceName} cleared blips and NPC's.");

            foreach (int blip in InitStables.StableBlips)
            {
                int _blip = blip;
                RemoveBlip(ref _blip);
            }


            foreach (int ped in InitStables.StableNpc)
            {
                int _ped = ped;
                DeletePed(ref _ped);
            }
        }
    }
}
