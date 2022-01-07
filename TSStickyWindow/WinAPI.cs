using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace TSStickyWindow
{
    public static class WinAPI
    {
        private const int MOUSEEVENTF_LEFTUP = 0x04;

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        public static void MouseLeftUp(UIElement element)
        {
            var point = Mouse.GetPosition(element);
            mouse_event(MOUSEEVENTF_LEFTUP, (uint)point.X, (uint)point.Y, 0, 0);
        }
    }
}
