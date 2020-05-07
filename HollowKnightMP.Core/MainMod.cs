using System.Reflection;
using UnityEngine;
using Harmony;
using Modding;
using Console = System.Console;

namespace HKMPMain
{
    public class MainMod : Mod
    {
        public static ServerSettings photonSettings;
        public static NetworkManager manager;
        public static AssetBundle bundle;

        public override void Initialize()
        {
            if (!manager)
            {
                Console.WriteLine("[HollowKnightMP] Initializing mod");
                photonSettings = ScriptableObject.CreateInstance<ServerSettings>();

                photonSettings.AppID = "91f3e558-5a8a-457c-81dd-807771c71246";

                photonSettings.HostType = ServerSettings.HostingOption.PhotonCloud;
                photonSettings.Protocol = ExitGames.Client.Photon.ConnectionProtocol.Udp;
                photonSettings.PreferredRegion = CloudRegionCode.au;
                photonSettings.JoinLobby = true;

                PhotonNetwork.PhotonServerSettings = photonSettings;

                GameObject gobj = new GameObject("NetManager");
                manager = gobj.AddComponent<NetworkManager>();

                bundle = AssetBundle.LoadFromFile("Assets/mpassets");

                // Create Canvas
                GameObject netCanv = GameObject.Instantiate(bundle.LoadAsset("NetworkCanvas") as GameObject);
                netCanv.transform.Find("MainInfo").localPosition = new Vector2((-Screen.width/2) + 170, (Screen.height/2) + -30);
                manager.ui = netCanv.AddComponent<NetworkUI>();
                GameObject.DontDestroyOnLoad(netCanv);

                GameObject.DontDestroyOnLoad(gobj);

                HarmonyInstance harmony = HarmonyInstance.Create("hollowknightmp");
                harmony.PatchAll(Assembly.GetAssembly(typeof(NetworkManager)));
            }
        }
    }
}
