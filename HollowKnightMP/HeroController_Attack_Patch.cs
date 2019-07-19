using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace HollowKnightMP
{
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("Attack")]
    public class HeroController_Attack_Patch
    {
        public static void Postfix(GlobalEnums.AttackDirection attackDir)
        {
            RaiseEventOptions options = new RaiseEventOptions();
            options.CachingOption = EventCaching.DoNotCache;

            PhotonNetwork.RaiseEvent(NetworkManager.OnSwingNailEvent, attackDir, true, options);
        }
    }
}
