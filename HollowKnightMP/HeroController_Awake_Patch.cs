using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace HollowKnightMP
{
    [HarmonyPatch(typeof(HeroController))]
    [HarmonyPatch("Awake")]
    public class HeroController_Awake_Patch
    {
        public static void Postfix()
        {
            MainMod.manager.knightHasSpawned = true;
        }
    }
}
