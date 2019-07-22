using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace HKMPMain
{
    public class NetworkUI : MonoBehaviour
    {
        Text connectedMessage;

        public void Awake()
        {
            connectedMessage = transform.Find("MainInfo/InfoText").GetComponent<Text>();
        }

        public void Update()
        {
            string connectionmsg = "";

            switch (MainMod.manager.status)
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
                    connectionmsg = $"In main game room ({PhotonNetwork.room.PlayerCount} players)";
                    break;
            }

            connectedMessage.text = connectionmsg;
        }
    }
}
