using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Modding;
using UnityEngine;
using Console = System.Console;

namespace HKMPMain
{
    public class HKMP : Mod
    {
        public static Texture2D logo;

        public override void Initialize()
        {
            logo = ImageUtils.LoadTextureFromFile(Path.Combine(Environment.CurrentDirectory, "hollow_knight_Data/Managed/Mods/HKMP/logo_white.png"));

            GameObject netManager = new GameObject("Network Manager");
            netManager.AddComponent<NetworkManager>();
            netManager.AddComponent<NetworkCallbacks>();
            GameObject.DontDestroyOnLoad(netManager);

            //AppDomain.CurrentDomain.AssemblyResolve += ResolveAssemblies;
        }

        Assembly ResolveAssemblies(object sender, ResolveEventArgs args)
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
            if(assembly != null)
            {
                return assembly;
            }

            string fname = "HKMP\\"+args.Name.Split(',')[0] + ".dll";

            string modFolder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(HKMP)).Location);

            string newAssembly = Path.Combine(modFolder, fname);

            MPLogger.Log("ASSEMBLY RESOLVE: " + newAssembly);

            try
            {
                return Assembly.LoadFrom(newAssembly);
            }
            catch
            {
                return null;
            }
        }

        public override string GetVersion()
        {
            return "0.0.2";
        }
    }
}
