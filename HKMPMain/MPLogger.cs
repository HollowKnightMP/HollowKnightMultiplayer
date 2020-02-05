using System;

namespace HKMPMain
{
    public static class MPLogger
    {
        public static void Log(string text)
        {
            Console.WriteLine($"[HKMP] {text}");
        }
    }
}
