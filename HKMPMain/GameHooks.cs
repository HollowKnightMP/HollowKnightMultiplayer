using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModCommon;
using GlobalEnums;

namespace HKMPMain
{
    using SceneManager = UnityEngine.SceneManagement.SceneManager;

    public static class GameHooks
    {
        public static void Init()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                foreach (HealthManager hm in GameObject.FindObjectsOfType<HealthManager>())
                {
                    if(hm.gameObject.IsGameEnemy())
                    {
                        EnemyDatabase.AddEnemy(hm.gameObject);
                    }
                }

                foreach(EnemySpawner enemy in GameObject.FindObjectsOfType<EnemySpawner>())
                {
                    if(enemy.enemyPrefab.IsGameEnemy())
                    {
                        EnemyDatabase.AddEnemy(enemy.enemyPrefab);
                    }
                }
            };

            On.GameManager.FinishedEnteringScene += (orig, self) =>
            {
                orig(self);
                return;


                if (!GameManager.instance.IsGameplayScene() || !PhotonNetwork.inRoom)
                {
                    return;
                }

                bool isHost = true;
                NetworkPlayer sceneHost = null;

                foreach (NetworkPlayer player in NetworkManager.main.playerList.Values)
                {
                    if(player.levelName == GameManager.instance.GetSceneNameString())
                    {
                        isHost = false;
                        sceneHost = player;
                    }
                }

                NetworkManager.main.localPlayer.isSceneHost = isHost;
                MPLogger.Log(isHost ? "We are the host!" : "Someone else is the host!");

                HealthManager[] enemies = GameObject.FindObjectsOfType<HealthManager>();
                List<string> enemyNames = new List<string>();
                List<int> enemyIDs = new List<int>();
                foreach (HealthManager enemy in enemies)
                {
                    if (enemy.gameObject.IsGameEnemy())
                    {
                        if (isHost)
                        {
                            string name = EnemyDatabase.TrimFromName(enemy.gameObject);
                            MPLogger.Log($"Setting up enemy for network: {name}");

                            enemyNames.Add(name);

                            NetworkEnemy netEnemy = enemy.gameObject.AddComponent<NetworkEnemy>();
                            enemy.gameObject.AddComponent<PhotonView>();

                            MPLogger.Log(enemy.animator == null ? "ANIMATOR IS NULL" : "ANIMATOR IS FINE");

                            netEnemy.anim = enemy.animator;

                            var view = netEnemy.photonView;

                            view.ownershipTransfer = OwnershipOption.Takeover;
                            view.synchronization = ViewSynchronization.UnreliableOnChange;
                            view.viewID = PhotonNetwork.AllocateViewID();
                            view.TransferOwnership(PhotonNetwork.player);
                            view.ObservedComponents = new List<Component>
                            {
                                netEnemy
                            };

                            enemyIDs.Add(view.viewID);
                        }
                        else
                        {
                            GameObject.Destroy(enemy.gameObject);
                        }
                    }
                }

                if (!isHost)
                {
                    MPLogger.Log("Spawning other player's enemies");

                    enemyIDs = sceneHost.enemyIDs;
                    enemyNames = sceneHost.enemyNames;

                    if (enemyNames != null && enemyNames.Count > 0)
                    {
                        for (int i = 0; i < enemyIDs.Count; i++)
                        {
                            string name = enemyNames[i];
                            GameObject enemy = EnemyDatabase.SpawnEnemy(name, Vector3.zero);

                            PhotonView view = enemy.AddComponent<PhotonView>();
                            NetworkEnemy netEnemy = enemy.AddComponent<NetworkEnemy>();
                            netEnemy.anim = enemy.GetComponent<HealthManager>().animator;
                            view.ownershipTransfer = OwnershipOption.Takeover;
                            view.synchronization = ViewSynchronization.UnreliableOnChange;
                            view.viewID = enemyIDs[i];
                            view.TransferOwnership(sceneHost.photonView.owner);
                            view.ObservedComponents = new List<Component>
                            {
                                netEnemy
                            };
                        }
                    }
                }
                else
                {
                    var serializeData = new NetworkPlayer.SerializedEnemies()
                    {
                        enemyIDs = enemyIDs,
                        enemyNames = enemyNames
                    };

                    string dataToSend = JsonUtility.ToJson(serializeData);
                    MPLogger.Log($"Sending data: {dataToSend}");

                    PhotonNetwork.RaiseEvent(NetworkCallbacks.OnEnterNewRoom, dataToSend, true, new RaiseEventOptions()
                    {
                        CachingOption = EventCaching.AddToRoomCache,
                        Receivers = ReceiverGroup.Others
                    });
                }
            };

            On.GameManager.BeginSceneTransition += (orig, self, info) =>
            {
                orig(self, info);

                // Leaving a scene, transition owner
            };

            On.GameManager.ReturnToMainMenu += (orig, self, saveMode, callback) =>
            {
                var result = orig(self, saveMode, callback);

                if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom();

                return result;
            };

            On.GameManager.EmergencyReturnToMenu += (orig, self, callback) =>
            {
                orig(self, callback);
                if (PhotonNetwork.inRoom) PhotonNetwork.LeaveRoom();
            };

            On.HeroController.Attack += (orig, self, attackDir) =>
            {
                orig(self, attackDir);

                if (PhotonNetwork.inRoom)
                {
                    NetAttackDir direction = NetAttackDir.normal;

                    if (self.cState.wallSliding)
                    {
                        direction = NetAttackDir.wall;
                    }
                    else if (self.cState.downAttacking)
                    {
                        direction = NetAttackDir.down;
                    }
                    else if(self.cState.upAttacking)
                    {
                        direction = NetAttackDir.up;
                    }
                    else if(!self.cState.altAttack)
                    {
                        direction = NetAttackDir.normalalt;
                    }

                    object[] data = new object[4];
                    data[0] = direction;
                    data[1] = HeroController.instance.playerData.equippedCharm_13;
                    data[2] = HeroController.instance.playerData.equippedCharm_18;
                    data[3] = HeroController.instance.cState.facingRight;

                    PhotonNetwork.RaiseEvent(NetworkCallbacks.OnPlayerSwingNail, data, true, new RaiseEventOptions());
                }
            };

            On.MenuStyleTitle.SetTitle += (orig, self, index) =>
            {
                self.Title.sprite = Sprite.Create(HKMP.logo, new Rect(0,0,HKMP.logo.width, HKMP.logo.height), new Vector2(0.5f, 0.5f));
                self.Title.transform.localScale = Vector3.one * 2f;
            };
        }
    }
}
