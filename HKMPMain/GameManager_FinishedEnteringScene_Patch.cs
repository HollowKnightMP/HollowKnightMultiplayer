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
    [HarmonyPatch("FinishedEnteringScene")]
    public class GameManager_FinishedEnteringScene_Patch
    {
        public static void Postfix()
        {
            if (PhotonNetwork.inRoom)
            {
                if (GameManager.instance.sceneName != MainMod.manager.otherPlayerScene)
                {
                    string data = MainMod.manager.SyncEnemies();

                    PhotonNetwork.RaiseEvent(NetworkManager.OnSceneTransitionEvent, data, true, new RaiseEventOptions());
                }
                else
                {
                    MainMod.manager.SetupOtherPlayerEnemies();
                }
            }
        }
    }
}