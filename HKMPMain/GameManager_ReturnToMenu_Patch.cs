using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;

namespace HKMPMain
{
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("ReturnToMainMenu")]
    public class GameManager_ReturnToMenu_Patch
    {
        public static void Postfix()
        {
            if(MainMod.manager.otherPlayer)
            {
                GameObject.Destroy(MainMod.manager.otherPlayer.gameObject);
            }
            MainMod.manager.needsToSetupOtherPlayer = false;
            MainMod.manager.otherPlayerID = 0;
            GameObject.Destroy(MainMod.manager.localPlayerView.gameObject);
            MainMod.manager.hasSetupPlayer = false;
            MainMod.manager.knightHasSpawned = false;
            PhotonNetwork.Disconnect();
        }
    }
}
