using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace HollowKnightMP
{
    public class NetworkUI : MonoBehaviour
    {
        public void OnGUI()
        {
            string connectionmsg = "";

            switch(MainMod.manager.status)
            {
                case NetworkManager.ConnectionStatus.NotConnected:
                    connectionmsg = "Not Connected";
                    break;
                case NetworkManager.ConnectionStatus.ConnectedToPhoton:
                    connectionmsg = "Connected to main server";
                    break;
                case NetworkManager.ConnectionStatus.InLobby:
                    connectionmsg = "In Lobby";
                    break;
                case NetworkManager.ConnectionStatus.Connecting:
                    connectionmsg = "Connecting...";
                    break;
                case NetworkManager.ConnectionStatus.InRoom:
                    connectionmsg = "In main game room";
                    break;
            }

            GUILayout.Label(connectionmsg);
        }
    }
}
