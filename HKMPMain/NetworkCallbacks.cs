using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using GlobalEnums;

namespace HKMPMain
{
    public class NetworkCallbacks : MonoBehaviour, IPunCallbacks
    {
        public const byte OnConnectedToRoom = 0;
        public const byte OnSetupLocalPlayer = 1;
        public const byte OnPlayerSwingNail = 2;
        public const byte OnGetHit = 3;
        public const byte OnEnterNewRoom = 4;

        public static void OnPhotonEvent(byte code, object content, int sender)
        {
            if(code == OnConnectedToRoom)
            {
                MPLogger.Log("Recieved host game data");
                if (!PhotonNetwork.isMasterClient)
                {
                    object[] data = (object[])content;

                    byte[] saveData = (byte[])data[0];

                    Platform.Current.WriteSaveSlot(-1, saveData, delegate (bool success) { GameManager.instance.LoadGameFromUI(-1); });
                }
            }
            else if(code == OnSetupLocalPlayer)
            {
                MPLogger.Log("Recieved ID from other player");
                NetworkManager.main.setupQueue.Add(PhotonPlayer.Find(sender), (int)content);
            }
            else if(code == OnPlayerSwingNail)
            {
                object[] data = (object[])content;

                if(!NetworkManager.main.playerList.ContainsKey(PhotonPlayer.Find(sender)))
                {
                    return;
                }

                NetAttackDir dir = (NetAttackDir)data[0];
                bool mantis = (bool)data[1];
                bool longnail = (bool)data[2];
                bool right = (bool)data[3];

                NetworkManager.main.CreateNailSwing(sender, dir, mantis, longnail, right);
            }
            else if(code == OnEnterNewRoom)
            {
                string data = (string)content;
                MPLogger.Log($"Recieved data: {data}");

                NetworkPlayer.SerializedEnemies enemies = JsonUtility.FromJson<NetworkPlayer.SerializedEnemies>(data);

                NetworkPlayer host = NetworkManager.main.playerList[PhotonPlayer.Find(sender)];
                host.enemyIDs = enemies.enemyIDs;
                host.enemyNames = enemies.enemyNames;
            }
        }

        public void OnConnectedToMaster()
        {
            MPLogger.Log("Connected to master server");
        }

        public void OnConnectedToPhoton()
        {
            MPLogger.Log("Connected to Photon main");
        }

        public void OnConnectionFail(DisconnectCause cause)
        {
            MPLogger.Log($"Connection Failed: {cause.ToString()}");
        }

        public void OnCreatedRoom()
        {
            NetworkManager.main.StartCoroutine(CreateRoomAsync());
        }

        public static IEnumerator CreateRoomAsync()
        {
            MPLogger.Log("Sending save data to other player...");
            if (NetworkManager.main.needsToSetupPlayer)
            {
                NetworkManager.main.SetupLocalPlayer();
            }

            Platform.current.ReadSaveSlot(GameManager.instance.profileID, delegate (byte[] dat)
            {
                NetworkManager.main.saveData = dat;
            });
            if (NetworkManager.main.saveData == null)
            {
                yield return null;
            }

            MPLogger.Log($"Length of data: {NetworkManager.main.saveData.Length}");

            object[] data = new object[]
            {
                NetworkManager.main.saveData
            };
            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.AddToRoomCache
            };

            PhotonNetwork.RaiseEvent(NetworkCallbacks.OnConnectedToRoom, data, true, options);
            yield break;
        }

        public void OnCustomAuthenticationFailed(string debugMessage)
        {
        }

        public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
        {
        }

        public void OnDisconnectedFromPhoton()
        {
        }

        public void OnFailedToConnectToPhoton(DisconnectCause cause)
        {
            MPLogger.Log($"Failed to connect: {cause.ToString()}");
        }

        public void OnJoinedLobby()
        {
            MPLogger.Log("Connected to lobby successfully");
            PhotonNetwork.FindFriends(NetworkManager.main.friends.ToArray());
        }

        public void OnJoinedRoom()
        {
            MPLogger.Log("Joined room successfully!");
            NetworkManager.main.needsToSetupPlayer = true;
        }

        public void OnLeftLobby()
        {
        }

        public void OnLeftRoom()
        {
            if(GameManager.instance.profileID == -1)
            {
                GameManager.instance.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.DontSave);
            }

            NetworkManager.main.saveData = null;
        }

        public void OnLobbyStatisticsUpdate()
        {
        }

        public void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
        }

        public void OnOwnershipRequest(object[] viewAndPlayer)
        {
        }

        public void OnOwnershipTransfered(object[] viewAndPlayers)
        {
        }

        public void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
        }

        public void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
        {
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
        }

        public void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
        }

        public void OnPhotonMaxCccuReached()
        {
        }

        public void OnPhotonPlayerActivityChanged(PhotonPlayer otherPlayer)
        {
        }

        public void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            MPLogger.Log("Other player joined");
        }

        public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if(GameManager.instance.profileID == -1)
            {
                PhotonNetwork.LeaveRoom();
                GameManager.instance.ReturnToMainMenu(GameManager.ReturnToMainMenuSaveModes.DontSave);
            }
        }

        public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
        }

        public void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
        }

        public void OnReceivedRoomListUpdate()
        {
            PhotonNetwork.FindFriends(NetworkManager.main.friends.ToArray());
        }

        public void OnUpdatedFriendList()
        {
            NetworkManager.main.steamFriends = new List<FriendInfo>();
            NetworkManager.main.nonSteamFriends = new List<FriendInfo>();
            foreach(FriendInfo friend in PhotonNetwork.Friends)
            {
                if(NetworkManager.main.steamFriendNames.Contains(friend.UserId))
                {
                    NetworkManager.main.steamFriends.Add(friend);
                }
                else
                {
                    NetworkManager.main.nonSteamFriends.Add(friend);
                }
            }
        }

        public void OnWebRpcResponse(OperationResponse response)
        {
        }
    }
}
