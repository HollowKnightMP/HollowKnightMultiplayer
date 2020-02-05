using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using GlobalEnums;
using Steamworks;

namespace HKMPMain
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager main;
        // Is set to true when the player is in a room, the knight has spawned and the local player has not been initialized
        public bool needsToSetupPlayer = false;
        public NetworkPlayer localPlayer;

        public NetworkPlayer sceneHost;

        // Queue for setting up other players
        public Dictionary<PhotonPlayer, int> setupQueue = new Dictionary<PhotonPlayer, int>();
        // Instantiated player objects (excluding local player)
        public Dictionary<PhotonPlayer, NetworkPlayer> playerList = new Dictionary<PhotonPlayer, NetworkPlayer>();
        public List<string> friends = new List<string>();
        public List<string> steamFriendNames = new List<string>();
        public List<string> nonSteamFriendNames = new List<string>();

        public List<FriendInfo> steamFriends = new List<FriendInfo>();
        public List<FriendInfo> nonSteamFriends = new List<FriendInfo>();

        // Should only be set if we are the host
        public byte[] saveData = null;

        [Serializable]
        public class FriendList
        {
            public string[] friendNames;
        }

        // Init methods
        public void Awake()
        {
            main = this;
            InitializePhoton();
            GameHooks.Init();
            ModUI.Init();
            PhotonNetwork.OnEventCall += NetworkCallbacks.OnPhotonEvent;
            if(SteamAPI.IsSteamRunning())
            {
                PhotonNetwork.playerName = SteamFriends.GetPersonaName();
                ModUI.main.playerName = PhotonNetwork.playerName;

                int friendCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
                friends = new List<string>();
                for (int i = 0; i < friendCount; i++)
                {
                    CSteamID friend = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    string name = SteamFriends.GetFriendPersonaName(friend);
                    friends.Add(name);
                    steamFriendNames.Add(name);
                }
            }
            if (PlayerPrefs.HasKey("NonSteamFriends"))
            {
                nonSteamFriendNames = JsonUtility.FromJson<FriendList>(PlayerPrefs.GetString("NonSteamFriends")).friendNames.ToList();

                friends.AddRange(nonSteamFriendNames);
            }

            PhotonNetwork.AuthValues = new AuthenticationValues(PhotonNetwork.playerName);
            ConnectToLobby();
        }

        public void InitializePhoton()
        {
            MPLogger.Log("Initializing Photon Settings");
            ServerSettings settings = ScriptableObject.CreateInstance<ServerSettings>();
            settings.HostType = ServerSettings.HostingOption.BestRegion;
            settings.AppID = "91f3e558-5a8a-457c-81dd-807771c71246";
            settings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
            settings.JoinLobby = true;

            PhotonNetwork.PhotonServerSettings = settings;
        }

        // Monobehaviour messages
        public void Update()
        {
            if(GameManager.instance)
            {
                if(GameManager.instance.IsMenuScene())
                {
                    if (PhotonNetwork.inRoom && PhotonNetwork.isMasterClient) PhotonNetwork.LeaveRoom();
                }
                else if(GameManager.instance.IsGameplayScene())
                {
                    foreach(KeyValuePair<PhotonPlayer, int> kvp in setupQueue)
                    {
                        SetupOtherPlayer(kvp.Key, kvp.Value);
                    }
                    setupQueue.Clear();
                }
            }

            if(PhotonNetwork.inRoom && needsToSetupPlayer && GameManager.instance.IsGameplayScene())
            {
                SetupLocalPlayer();
            }
            else if(!PhotonNetwork.inRoom && localPlayer)
            {
                Destroy(localPlayer);
            }
        }

        public void OnApplicationQuit()
        {
            FriendList friendlist = new FriendList();
            friendlist.friendNames = nonSteamFriendNames.ToArray();

            PlayerPrefs.SetString("NonSteamFriends", JsonUtility.ToJson(friendlist));
        }

        // Connection methods
        public void ConnectToLobby()
        {
            MPLogger.Log("Connecting to Photon");
            PhotonNetwork.ConnectUsingSettings("0.0.2");
        }

        public IEnumerator ReconnectOnceDisconnected()
        {
            yield return new WaitUntil(() => !PhotonNetwork.connected);

            ConnectToLobby();
        }
        
        public void CreateGame()
        {
            if(GameManager.instance.IsGameplayScene())
            {
                MPLogger.Log("Creating room...");

                RoomOptions options = new RoomOptions()
                {
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
                    {
                        { "IsPublic", true },
                        { "Name", PhotonNetwork.playerName },
                    },
                    CustomRoomPropertiesForLobby = new string[] { "IsPublic", "Name" }
                };

                PhotonNetwork.CreateRoom(new Guid().ToString(), options, PhotonNetwork.lobby);
            }
            else
            {
                MPLogger.Log("Tried to create room, but we weren't in the game!");
            }
        }

        public void JoinGame(string name)
        {
            if(GameManager.instance.IsMenuScene())
            {
                MPLogger.Log("Joining room...");
                PhotonNetwork.JoinRoom(name);
            }
            else
            {
                MPLogger.Log("Tried to join a room when we weren't in the menu");
            }
        }

        // Object spawning methods
        public int SetupLocalPlayer()
        {
            MPLogger.Log("Setting up local player");
            MPLogger.Log($"{HeroController.instance.col2d.GetType().ToString()}");

            GameObject local = new GameObject("NetworkPlayerSender");
            DontDestroyOnLoad(local);

            PhotonView view = local.AddComponent<PhotonView>();
            NetworkPlayer player = local.AddComponent<NetworkPlayer>();

            view.ownershipTransfer = OwnershipOption.Takeover;
            view.synchronization = ViewSynchronization.UnreliableOnChange;
            view.viewID = PhotonNetwork.AllocateViewID();
            view.TransferOwnership(PhotonNetwork.player);
            view.ObservedComponents = new List<Component>
            {
                player
            };

            RaiseEventOptions options = new RaiseEventOptions()
            {
                CachingOption = EventCaching.AddToRoomCache
            };

            PhotonNetwork.RaiseEvent(NetworkCallbacks.OnSetupLocalPlayer, view.viewID, true, options);

            localPlayer = player;

            needsToSetupPlayer = false;

            return view.viewID;
        }

        public void SetupOtherPlayer(PhotonPlayer player, int viewID)
        {
            MPLogger.Log($"Setting up other player: {player.NickName}");
            GameObject playerObj = new GameObject("OTHER");
            var netSync = playerObj.AddComponent<NetworkPlayer>();

            var view = playerObj.AddComponent<PhotonView>();

            var body = playerObj.AddComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            var box = playerObj.AddComponent<BoxCollider2D>();
            box.size = ((BoxCollider2D)HeroController.instance.col2d).size;
            box.offset = ((BoxCollider2D)HeroController.instance.col2d).offset;
            playerObj.layer = (int)PhysLayers.BOUNCER;
            playerObj.AddComponent<BigBouncer>();

            view.ownershipTransfer = OwnershipOption.Takeover;
            view.synchronization = ViewSynchronization.UnreliableOnChange;
            view.viewID = viewID;
            view.TransferOwnership(player);
            view.ObservedComponents = new List<Component>
            {
                netSync
            };

            var rend = playerObj.AddComponent<MeshRenderer>();
            playerObj.AddComponent<MeshFilter>();

            var spriteAnim = playerObj.AddComponent<tk2dSpriteAnimation>();
            spriteAnim.clips = HeroController.instance.animCtrl.animator.Library.clips;
            playerObj.AddComponent<tk2dSprite>();
            var anim = tk2dSpriteAnimator.AddComponent(playerObj, spriteAnim, 0);
            var collection = playerObj.AddComponent<tk2dSpriteCollectionData>();

            netSync.anim = anim;
            netSync.renderer = rend;

            collection.spriteDefinitions = HeroController.instance.animCtrl.animator.Sprite.collection.spriteDefinitions;

            anim.SetSprite(collection, 0);

            playerObj.transform.position = HeroController.instance.transform.position + (Vector3.one * 1000f);

            var txt = CreateTextMesh(player.NickName);
            var follow = txt.gameObject.AddComponent<FollowTransform>();
            follow.target = playerObj.transform;
            follow.offset = Vector3.up;

            DontDestroyOnLoad(playerObj);
            DontDestroyOnLoad(txt.gameObject);

            playerList.Add(player, netSync);
        }

        public TextMesh CreateTextMesh(string text)
        {
            GameObject mesh = new GameObject();
            mesh.AddComponent<MeshRenderer>();

            TextMesh txt = mesh.AddComponent<TextMesh>();
            txt.characterSize = 0.05f;
            txt.fontSize = 100;
            txt.anchor = TextAnchor.MiddleCenter;
            txt.text = text;

            return txt;
        }

        public void CreateNailSwing(int playerID, NetAttackDir dir, bool mantis, bool longnail, bool right)
        {
            PhotonPlayer player = PhotonPlayer.Find(playerID);

            if(!playerList.ContainsKey(player))
            {
                MPLogger.Log("Got nailswing for player who hasnt been initialized!");
                return;
            }

            NailSlash original = null;
            NailSlash slash = null;

            switch (dir)
            {
                case NetAttackDir.normal:
                    original = HeroController.instance.normalSlash;
                    slash = Instantiate(original);
                    break;
                case NetAttackDir.up:
                    original = HeroController.instance.upSlash;
                    slash = Instantiate(original);
                    break;
                case NetAttackDir.down:
                    original = HeroController.instance.downSlash;
                    slash = Instantiate(original);
                    break;
                case NetAttackDir.normalalt:
                    original = HeroController.instance.alternateSlash;
                    slash = Instantiate(original);
                    break;
                case NetAttackDir.wall:
                    original = HeroController.instance.wallSlash;
                    slash = Instantiate(original);
                    break;
            }

            slash.SetMantis(mantis);
            slash.SetLongnail(longnail);

            slash.StartSlash();
            slash.gameObject.layer = (int)PhysLayers.ENEMIES;

            Vector3 scale = slash.transform.localScale;
            scale.x *= right ? -1 : 1;
            slash.transform.localScale = scale;

            var follow = slash.gameObject.AddComponent<FollowTransform>();
            follow.target = NetworkManager.main.playerList[player].transform;
            follow.offset = original.transform.localPosition;

            Destroy(slash.gameObject, 2f);
        }

        // Player info methods
        public int GetPlayerHP(PhotonPlayer player)
        {
            if(player == PhotonNetwork.player)
            {
                return HeroController.instance.playerData.health;
            }

            return playerList[player].health;
        }
    }
}
