using System;

namespace HollowKnightMP.Core
{
    public static class MPLogger
    {
        public static void Log(string text)
        {
            Console.WriteLine($"[HKMP] {text}");
        }
    }
}
