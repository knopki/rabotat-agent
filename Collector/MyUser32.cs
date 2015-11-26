using System;
using System.Runtime.InteropServices;

namespace RabotatAgent.Collector
{
    public static class MyUser32
    {
        public static IntPtr GetForegroundWindow() {
            return NativeMethods.GetForegroundWindow();
        }

        public static IntPtr GetDesktopWindow() {
            return NativeMethods.GetDesktopWindow();
        }

        public static IntPtr GetWindowThreadProcessId(IntPtr hWnd, out UInt32 processId) {
            return NativeMethods.GetWindowThreadProcessId(hWnd, out processId);
        }

        private static bool GetLastInputInfo(ref NativeMethods.LASTINPUTINFO plii) {
            return NativeMethods.GetLastInputInfo(ref plii);
        }

        public static DateTime LastInput
        {
            get
            {
                DateTime bootTime = DateTime.UtcNow.AddMilliseconds(-Environment.TickCount);
                DateTime lastInput = bootTime.AddMilliseconds(LastInputTicks);
                return lastInput;
            }
        }

        public static TimeSpan IdleTime
        {
            get
            {
                return DateTime.UtcNow.Subtract(LastInput);
            }
        }

        public static int LastInputTicks
        {
            get
            {
                NativeMethods.LASTINPUTINFO lii = new NativeMethods.LASTINPUTINFO();
                lii.cbSize = (uint)Marshal.SizeOf(typeof(NativeMethods.LASTINPUTINFO));
                GetLastInputInfo(ref lii);
                return lii.dwTime;
            }
        }
    }

    internal class NativeMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out UInt32 processId);

        [StructLayout(LayoutKind.Sequential)]
        public struct LASTINPUTINFO
        {
            public uint cbSize;
            public int dwTime;
        }

        [DllImport("user32.dll", SetLastError = false)]
        public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    }
}
