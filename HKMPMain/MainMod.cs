using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;


namespace HKMPMain
{
    public class MainMod
    {
        public static ServerSettings photonSettings;
        public static NetworkManager manager;

        public static void Main()
        {
            if (!manager)
            {
                photonSettings = ScriptableObject.CreateInstance<ServerSettings>();

                photonSettings.AppID = "91f3e558-5a8a-457c-81dd-807771c71246";

                photonSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
                photonSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
                photonSettings.PreferredRegion = CloudRegionCode.au;
                photonSettings.JoinLobby = true;
                photonSettings.RpcList = new List<string>()
                {
                    "TakeDamage"
                };

                PhotonNetwork.PhotonServerSettings = photonSettings;

                GameObject gobj = new GameObject("NetManager");
                manager = gobj.AddComponent<NetworkManager>();
                gobj.AddComponent<NetworkUI>();

                GameObject.DontDestroyOnLoad(gobj);

                HarmonyInstance harmony = HarmonyInstance.Create("hollowknightmp");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
        }
    }
}
