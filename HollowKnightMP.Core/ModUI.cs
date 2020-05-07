using UnityEngine;
using UnityEngine.UI;

namespace HollowKnightMP.Core
{
    public class ModUI : MonoBehaviour
    {
        public static ModUI main;

        string connectionMessage = "";
        string friendName = "";
        Rect mainWindowRect = new Rect(100, 100, 1000, 1500);
        Vector2 scrollPos = Vector2.zero;
        bool showWindow = false;
        bool showButton = true;
        bool showSteam = true;
        bool showNonSteam = true;

        WindowState tab;

        public string playerName = "";

        public static void Init()
        {
            GameObject netCanv = NetworkManager.main.gameObject;
            main = netCanv.AddComponent<ModUI>();

            if(PlayerPrefs.HasKey("ShowButton"))
            {
                main.showButton = PlayerPrefs.GetInt("ShowButton") == 1;
            }
        }

        public void Update()
        {
            mainWindowRect.width = Mathf.RoundToInt(Screen.width / 2.5f);
            mainWindowRect.height = Mathf.RoundToInt(Screen.height * 0.6f);
            string infoMessage = "";

            if(Input.GetKeyDown(KeyCode.F3))
            {
                showWindow = !showWindow;
            }

            switch (PhotonNetwork.connectionStateDetailed)
            {
                case ClientState.Uninitialized:
                    infoMessage = "Uninitialized";
                    break;
                case ClientState.JoinedLobby:
                    infoMessage = "In Network Lobby";
                    break;
                case ClientState.Joining:
                    infoMessage = "Joining room...";
                    break;
                case ClientState.Joined:
                    infoMessage = $"In Game room ({PhotonNetwork.room.PlayerCount}) players";
                    break;
                case ClientState.Leaving:
                    infoMessage = "Leaving room...";
                    break;
                case ClientState.ConnectingToNameServer:
                    infoMessage = "Connecting...";
                    break;
                case ClientState.ConnectingToMasterserver:
                    infoMessage = "Connecting...";
                    break;
                case ClientState.PeerCreated:
                case ClientState.Disconnected:
                    infoMessage = "Disconnected";
                    break;
            }

            connectionMessage = infoMessage;
        }

        public void OnGUI()
        {
            using (new GUILayout.AreaScope(new Rect(Screen.width - 150, 0, 150, 200)))
            {
                if (showButton)
                {
                    if (GUILayout.Button("Show Menu (F3)"))
                    {
                        showWindow = !showWindow;
                    }
                }
            }

            if (showWindow)
            {
                mainWindowRect = GUI.Window(0, mainWindowRect, MainWindow, "Hollow Knight Multiplayer");
            }


        }

        public void MainWindow(int id)
        {
            if(!GameManager.instance.isPaused)
            {
                GameManager.instance.PauseGameToggle();
            }

            using(new GUILayout.HorizontalScope("Box"))
            {
                GUI.enabled = tab != WindowState.MainMenu;
                if(GUILayout.Button("Main Menu"))
                {
                    tab = WindowState.MainMenu;
                }
                GUI.enabled = tab != WindowState.FriendList && (PhotonNetwork.insideLobby || PhotonNetwork.inRoom);
                if (GUILayout.Button("Friends"))
                {
                    tab = WindowState.FriendList;
                }
                GUI.enabled = tab != WindowState.PlayerList && PhotonNetwork.inRoom;
                if (GUILayout.Button("Player List"))
                {
                    tab = WindowState.PlayerList;
                }
                GUI.enabled = true;
            }

            switch (tab)
            {
                case WindowState.MainMenu:
                    MainMenu();
                    break;
                case WindowState.PlayerList:
                    PlayerList();
                    break;
                case WindowState.FriendList:
                    FriendList();
                    break;
            }

            GUI.DragWindow();
        }

        public void PlayerList()
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (NetworkManager.main.playerList.ContainsKey(player) || player == PhotonNetwork.player)
                {
                    using (new GUILayout.HorizontalScope("Box"))
                    {
                        GUILayout.Label($"{player.NickName} | HP: {NetworkManager.main.GetPlayerHP(player)}");
                        if(PhotonNetwork.isMasterClient && player != PhotonNetwork.player)
                        {
                            GUILayout.Button("Kick", GUILayout.Width(mainWindowRect.width*0.2f));
                        }
                    }
                }
            }
        }

        public void MainMenu()
        {
            showButton = GUILayout.Toggle(showButton, "Show Corner Button");

            Color defaultColor = GUI.color;
            GUILayout.Label(connectionMessage);

            string nameTextLabel = $"Player Name: {PhotonNetwork.playerName}";
            bool playerNameValid = !string.IsNullOrEmpty(PhotonNetwork.playerName);
            if (!playerNameValid)
            {
                GUI.color = Color.red;
                nameTextLabel = "Please set a player name!";
            }

            GUILayout.Label(nameTextLabel);
            GUI.color = defaultColor;
            GUI.enabled = !PhotonNetwork.inRoom;
            using (new GUILayout.HorizontalScope())
            {
                playerName = GUILayout.TextField(playerName);
                if (GUILayout.Button("Apply", GUILayout.Width(mainWindowRect.width / 5)))
                {
                    if (PhotonNetwork.connected)
                    {
                        PhotonNetwork.Disconnect();
                    }
                    PhotonNetwork.playerName = playerName;
                    PhotonNetwork.AuthValues.UserId = playerName;
                    StartCoroutine(NetworkManager.main.ReconnectOnceDisconnected());
                }
            }

            GUI.enabled = playerNameValid;

            if(!PhotonNetwork.inRoom)
            {
                if (GameManager.instance.IsGameplayScene() && PhotonNetwork.connected)
                {
                    GUI.color = Color.green;
                    if (GUILayout.Button("Create Room"))
                    {
                        NetworkManager.main.CreateGame();
                    }
                }
                else
                {
                    GUI.color = Color.red;
                    GUI.enabled = false;
                    string text = "You must be in-game to create a room!";
                    if(!PhotonNetwork.connected)
                    {
                        text = "You must be connected to the lobby to create a room!";
                    }
                    GUILayout.Button(text);
                    GUI.enabled = true;
                }
            }
            else
            {
                GUI.color = Color.red;
                if (GUILayout.Button("Leave Room"))
                {
                    PhotonNetwork.LeaveRoom();
                }
            }
            GUI.color = defaultColor;

            if (PhotonNetwork.inRoom)
            {
                if (PhotonNetwork.isMasterClient)
                {
                    GUILayout.Label("You are the host");
                    bool val = (bool)PhotonNetwork.room.CustomProperties["IsPublic"];
                    if (GUILayout.Button(val ? "Make Room Private" : "Make Room Public"))
                    {
                        var hash = new ExitGames.Client.Photon.Hashtable()
                        {
                            { "IsPublic", !val }
                        };

                        PhotonNetwork.room.SetCustomProperties(hash);
                    }
                }
                else
                {
                    GUILayout.Label($"{PhotonNetwork.masterClient.NickName} is the host");
                }

                if(NetworkManager.main.localPlayer)
                {
                    if(NetworkManager.main.localPlayer.isSceneHost)
                    {
                        GUILayout.Label("You are the scene host");
                    }
                    else
                    {
                        GUILayout.Label("Someone else is the scene host");
                    }
                }
            }

            GUILayout.Label("Room List");
            using(var scroll = new GUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scroll.scrollPosition;

                foreach(RoomInfo info in PhotonNetwork.GetRoomList())
                {
                    if(!(bool)info.CustomProperties["IsPublic"])
                    {
                        continue;
                    }

                    using (new GUILayout.HorizontalScope("Box", GUILayout.Width(mainWindowRect.width*0.9f)))
                    {
                        GUILayout.Label((string)info.CustomProperties["Name"], GUILayout.Width(mainWindowRect.width*0.7f));
                        if (GameManager.instance.IsMenuScene())
                        {
                            if (GUILayout.Button("Join", GUILayout.Width(mainWindowRect.width * 0.2f)))
                            {
                                NetworkManager.main.JoinGame(info.Name);
                            }
                        }
                        else
                        {
                            GUI.enabled = false;
                            GUILayout.Button("Join", GUILayout.Width(mainWindowRect.width * 0.2f));
                        }
                    }
                    GUILayout.Space(mainWindowRect.height * 0.04f);
                }
            }
            GUI.enabled = true;
        }

        public void FriendList()
        {
            GUI.enabled = true;

            var defaultColor = GUI.color;
            GUILayout.Label("Friends");

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Enter Friend Name: ");
                friendName = GUILayout.TextField(friendName);
                if(GUILayout.Button("Add", GUILayout.Width(mainWindowRect.width*0.2f)))
                {
                    NetworkManager.main.friends.Add(friendName);
                    friendName = "";

                    PhotonNetwork.FindFriends(NetworkManager.main.friends.ToArray());
                }
            }

            using (var scroll = new GUILayout.ScrollViewScope(scrollPos))
            {
                scrollPos = scroll.scrollPosition;

                showSteam = GUILayout.Toggle(showSteam, "Steam Friends", "Button");

                if (showSteam)
                {
                    foreach (FriendInfo friend in NetworkManager.main.steamFriends)
                    {
                        using (new GUILayout.HorizontalScope("Box"))
                        {
                            GUILayout.Label(friend.UserId, GUILayout.Width(mainWindowRect.width * 0.3f));

                            GUI.color = friend.IsOnline ? Color.green : Color.red;
                            string labelText = "OFFLINE";

                            if(friend.IsInRoom)
                            {
                                labelText = "IN ROOM";
                            }
                            else if(friend.IsOnline)
                            {
                                labelText = "ONLINE";
                            }

                            GUILayout.Label(labelText, GUILayout.Width(mainWindowRect.width * 0.3f));
                            GUI.color = defaultColor;

                            GUI.enabled = friend.IsInRoom;
                            if (GUILayout.Button("Join", GUILayout.Width(mainWindowRect.width * 0.3f)))
                            {
                                NetworkManager.main.JoinGame(friend.Room);
                            }
                            GUI.enabled = true;
                        }
                    }

                }
                showNonSteam = GUILayout.Toggle(showNonSteam, "Non Steam Friends", "Button");

                if (showNonSteam)
                {
                    foreach (FriendInfo friend in NetworkManager.main.nonSteamFriends)
                    {
                        using (new GUILayout.HorizontalScope("Box"))
                        {
                            GUILayout.Label(friend.UserId, GUILayout.Width(mainWindowRect.width * 0.3f));

                            GUI.color = friend.IsOnline ? Color.green : Color.red;
                            string labelText = "OFFLINE";

                            if (friend.IsInRoom)
                            {
                                labelText = "IN ROOM";
                            }
                            else if (friend.IsOnline)
                            {
                                labelText = "ONLINE";
                            }

                            GUILayout.Label(labelText, GUILayout.Width(mainWindowRect.width * 0.3f));
                            GUI.color = defaultColor;

                            bool roomHasSlots = false;
                            if (friend.IsInRoom)
                            {
                                foreach (RoomInfo room in PhotonNetwork.GetRoomList())
                                {
                                    if (room.Name == friend.Room)
                                    {
                                        roomHasSlots = room.IsOpen;
                                    }
                                }
                            }

                            GUI.enabled = friend.IsInRoom && roomHasSlots;
                            if (GUILayout.Button("Join", GUILayout.Width(mainWindowRect.width * 0.15f)))
                            {
                                NetworkManager.main.JoinGame(friend.Room);
                            }
                            GUI.enabled = true;
                            if (GUILayout.Button("Unfriend", GUILayout.Width(mainWindowRect.width * 0.15f)))
                            {
                                NetworkManager.main.friends.Remove(friend.UserId);
                                PhotonNetwork.FindFriends(NetworkManager.main.friends.ToArray());
                            }
                        }
                    }
                }
            }
        }

        void OnApplicationQuit()
        {
            PlayerPrefs.SetInt("ShowButton", showButton ? 1 : 0);
        }
    }

    public enum WindowState
    {
        MainMenu,
        PlayerList,
        FriendList
    }
}
