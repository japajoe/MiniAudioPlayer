using System;
using System.Runtime.InteropServices;

namespace MiniAudioPlayer.Utilities
{
    public static class NativeString
    {
        public static IntPtr Allocate(string str)
        {
            if(string.IsNullOrEmpty(str))
                return IntPtr.Zero;
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Marshal.StringToHGlobalUni(str);
            return Marshal.StringToHGlobalAnsi(str);
        }

        public static void Free(IntPtr str)
        {
            if(str == IntPtr.Zero)
                return;
            Marshal.FreeHGlobal(str);
        }

        public static string Get(IntPtr str)
        {
            if(str == IntPtr.Zero)
                return null;
            if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return Marshal.PtrToStringUni(str);
            return Marshal.PtrToStringAnsi(str);
        }
    }
}