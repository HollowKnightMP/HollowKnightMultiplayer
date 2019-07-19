using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Client.Photon;
using UnityEngine;

namespace HollowKnightMP
{
    public class NetworkManager : MonoBehaviour, IPunCallbacks
    {
        public ConnectionStatus status = ConnectionStatus.NotConnected;

        public NetworkPlayerController otherPlayer;
        public PhotonView localPlayerView;
        public bool hasSetupPlayer = false;
        public bool knightHasSpawned = false;
        public bool needsToSetupOtherPlayer = false;
        public int otherPlayerID;

        public static readonly byte OnPlayerSetupEvent = 0;
        public static readonly byte OnSwingNailEvent = 1;

        public enum ConnectionStatus
        {
            NotConnected,
            Connecting,
            ConnectedToPhoton,
            InLobby,
            InRoom
        }

        void OnPhotonEvent(byte eventCode, object content, int senderId)
        {
            if(eventCode == OnPlayerSetupEvent)
            {
                object[] data = (object[])content;
                int id = (int)data[0];
                PhotonPlayer player = PhotonPlayer.Find(senderId);

                if (hasSetupPlayer)
                {
                    CreateRemotePlayerPrefab(player, id);
                }
                else
                {
                    needsToSetupOtherPlayer = true;
                    otherPlayerID = id;
                    Console.WriteLine($"[HollowKnightMP] Added {player.NickName} to creation queue");
                }
            }
            else if(eventCode == OnSwingNailEvent)
            {
                switch ((GlobalEnums.AttackDirection)content)
                {
                    case GlobalEnums.AttackDirection.normal:
                        otherPlayer.slashNormal.StartSlash();
                        break;
                    case GlobalEnums.AttackDirection.upward:
                        otherPlayer.slashUp.StartSlash();
                        break;
                    case GlobalEnums.AttackDirection.downward:
                        otherPlayer.slashDown.StartSlash();
                        break;
                }
            }
        }

        void Update()
        {
            if(hasSetupPlayer && needsToSetupOtherPlayer)
            {
                CreateRemotePlayerPrefab(PhotonNetwork.otherPlayers[0], otherPlayerID);
                needsToSetupOtherPlayer = false;
            }

            if(knightHasSpawned && PhotonNetwork.inRoom && !hasSetupPlayer)
            {
                SetupPlayer();
            }
        }

        void OnEnable()
        {
            PhotonNetwork.OnEventCall += OnPhotonEvent;
        }

        void OnDisable()
        {
            PhotonNetwork.OnEventCall -= OnPhotonEvent;
        }

        public string SerializeCurrentEnemies()
        {
            return "";
        }

        public void SetupPlayer()
        {
            GameObject player = new GameObject("NetworkPlayer");
            Console.WriteLine("[HollowKnightMP] Setting up local player networking");
            localPlayerView = player.AddComponent<PhotonView>();
            localPlayerView.viewID = PhotonNetwork.AllocateViewID();
            localPlayerView.synchronization = ViewSynchronization.UnreliableOnChange;
            localPlayerView.ownershipTransfer = OwnershipOption.Takeover;
            localPlayerView.TransferOwnership(PhotonNetwork.player);

            NetworkPlayerController sync = player.AddComponent<NetworkPlayerController>();

            localPlayerView.ObservedComponents = new List<Component>()
            {
                sync
            };

            object[] eventContent = new object[]
            {
                localPlayerView.viewID
            };

            RaiseEventOptions options = new RaiseEventOptions();
            options.CachingOption = EventCaching.AddToRoomCache;
            options.Receivers = ReceiverGroup.Others;

            PhotonNetwork.RaiseEvent(OnPlayerSetupEvent, eventContent, true, options);
            DontDestroyOnLoad(player);

            hasSetupPlayer = true;
        }

        public GameObject CreateRemotePlayerPrefab(PhotonPlayer player, int id)
        {
            Console.WriteLine($"[HollowKnightMP] Creating prefab instance for player {player.NickName} with view ID {id}");

            GameObject other = new GameObject($"RemotePlayer {player.NickName}");

            CopyComponent(HeroController.instance.col2d, other);
            other.AddComponent<Rigidbody2D>().isKinematic = true;
            MeshRenderer rend = other.AddComponent<MeshRenderer>();
            MeshFilter filter = other.AddComponent<MeshFilter>();
            filter.sharedMesh = HeroController.instance.GetComponent<MeshFilter>().sharedMesh;
            rend.material = new Material(HeroController.instance.renderer.material);
            tk2dSpriteAnimator anim = (tk2dSpriteAnimator)CopyComponent(HeroController.instance.animCtrl.animator, other);
            anim.Library = (tk2dSpriteAnimation)CopyComponent(HeroController.instance.animCtrl.animator.Library, other);
            anim._sprite = (tk2dBaseSprite)CopyComponent(HeroController.instance.animCtrl.animator.Sprite, other);
            anim.SetSprite((tk2dSpriteCollectionData)CopyComponent(HeroController.instance.animCtrl.animator.Sprite.collection, other), anim._sprite.spriteId);

            // Setup Nail Slashing
            NailSlash slashUp = Instantiate(HeroController.instance.upSlash).GetComponent<NailSlash>();
            slashUp.transform.SetParent(other.transform);
            slashUp.transform.localPosition = HeroController.instance.upSlash.transform.localPosition;

            NailSlash slashNormal = Instantiate(HeroController.instance.normalSlash).GetComponent<NailSlash>();
            slashNormal.transform.SetParent(other.transform);
            slashNormal.transform.localPosition = HeroController.instance.normalSlash.transform.localPosition;

            NailSlash slashDown = Instantiate(HeroController.instance.downSlash).GetComponent<NailSlash>();
            slashDown.transform.SetParent(other.transform);
            slashDown.transform.localPosition = HeroController.instance.downSlash.transform.localPosition;

            ParticleSystem dash = Instantiate(HeroController.instance.dashParticlesPrefab, other.transform).GetComponent<ParticleSystem>();
            dash.transform.localPosition = HeroController.instance.dashParticlesPrefab.transform.localPosition;

            ParticleSystem shadowDash = Instantiate(HeroController.instance.shadowdashParticlesPrefab, other.transform).GetComponent<ParticleSystem>();
            shadowDash.transform.localPosition = HeroController.instance.shadowdashParticlesPrefab.transform.localPosition;

            ParticleSystem feathers = Instantiate(HeroController.instance.dJumpFeathers.gameObject, other.transform).GetComponent<ParticleSystem>();
            feathers.transform.localPosition = HeroController.instance.dJumpFeathers.transform.localPosition;

            GameObject wings = Instantiate(HeroController.instance.dJumpWingsPrefab, other.transform);
            wings.transform.localPosition = HeroController.instance.dJumpWingsPrefab.transform.localPosition;

            PhotonView view = other.AddComponent<PhotonView>();
            view.viewID = id;
            view.synchronization = ViewSynchronization.UnreliableOnChange;
            view.ownershipTransfer = OwnershipOption.Takeover;
            view.TransferOwnership(player);

            NetworkPlayerController sync = other.AddComponent<NetworkPlayerController>();
            view.ObservedComponents = new List<Component>()
            {
                sync
            };
            sync.anim = anim;
            sync.renderer = rend;
            sync.slashUp = slashUp;
            sync.slashNormal = slashNormal;
            sync.slashDown = slashDown;
            sync.dash = dash;
            sync.shadowDash = shadowDash;
            sync.jumpFeathers = feathers;
            sync.jumpWings = wings;

            DontDestroyOnLoad(other);
            otherPlayer = sync;

            return other;
        }

        public Component CopyComponent(Component original, GameObject destination)
        {
            Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            // Copied fields can be restricted with BindingFlags
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                try
                {
                    field.SetValue(copy, field.GetValue(original));
                }
                catch(Exception e)
                {
                    Console.WriteLine($"[HollowKnightMP] COPYCOMPONENTERROR {e.Message}");
                }
            }
            return copy;
        }

        public void AttemptConnect()
        {
            status = ConnectionStatus.Connecting;
            PhotonNetwork.playerName = Guid.NewGuid().ToString();
            PhotonNetwork.ConnectUsingSettings("0.0.1");
            Console.WriteLine("[HollowKnightMP] Attempting Connection");
        }

        public void OnConnectedToMaster()
        {
        }

        public void OnConnectedToPhoton()
        {
            status = ConnectionStatus.ConnectedToPhoton;
            Console.WriteLine("[HollowKnightMP] Connected to Photon");
        }

        public void OnConnectionFail(DisconnectCause cause)
        {
        }

        public void OnCreatedRoom()
        {
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
        }

        public void OnJoinedLobby()
        {
            status = ConnectionStatus.InLobby;
            Console.WriteLine("[HollowKnightMP] Connected to Lobby");

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 10;
            options.IsVisible = true;

            PhotonNetwork.JoinOrCreateRoom("Main", options, PhotonNetwork.lobby);
        }

        public void OnJoinedRoom()
        {
            status = ConnectionStatus.InRoom;
            Console.WriteLine($"[HollowKnightMP] Connected to Game Room with {PhotonNetwork.room.PlayerCount} players");
        }

        public void OnLeftLobby()
        {
        }

        public void OnLeftRoom()
        {
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
            Console.WriteLine($"[HollowKnightMP] Player {newPlayer.NickName} joined");
        }

        public void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if(this.otherPlayer)
            {
                Destroy(this.otherPlayer.gameObject);
            }
            needsToSetupOtherPlayer = false;
            otherPlayerID = 0;
            Console.WriteLine($"[HollowKnightMP] Player {otherPlayer.NickName} left");
        }

        public void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
        }

        public void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
        }

        public void OnReceivedRoomListUpdate()
        {
        }

        public void OnUpdatedFriendList()
        {
        }

        public void OnWebRpcResponse(OperationResponse response)
        {
        }
    }
}
