using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;

namespace HKMPMain
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("ContinueGame")]
    public class GameManager_ContinueGame_Patch
    {
        public static void Postfix()
        {
            MainMod.manager.AttemptConnect();
        }
    }
}
