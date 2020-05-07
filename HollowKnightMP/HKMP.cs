using System;
using System.IO;
using System.Reflection;
using Modding;

namespace HollowKnightMP
{
    public class HKMP : Mod
    {
        private static string ModAssetsDir { get; } = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Path.Combine(Environment.CurrentDirectory, @"hollow_knight_Data\Managed\Mods"), "HKMP");
        private static string ManagedLibsDir { get; } = Path.Combine(Path.Combine(ModAssetsDir, ".."), "..");
        
        public override void Initialize()
        {
            // Resolve dependencies in HKMP folder inside the Mods folder
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;
            AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += OnAssemblyResolve;
            
            BootstrapHKMP();
        }

        private void BootstrapHKMP()
        {
            var core = Assembly.Load(new AssemblyName("HollowKnightMP.Core"));
            var mainType = core.GetType("HollowKnightMP.Core.HKMP");
            Activator.CreateInstance(mainType);
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs eventArgs)
        {
            string dllFileName = eventArgs.Name.Split(',')[0] + ".dll";

            // DLL should be either in HKMP dir or Unity's Managed dir
            string dllPath = Path.Combine(ModAssetsDir, dllFileName);
            if (!File.Exists(dllPath))
            {
                dllPath = Path.Combine(ManagedLibsDir, dllFileName);
            }

            return Assembly.LoadFile(dllPath);
        }
        
        public override string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
        }
    }
}