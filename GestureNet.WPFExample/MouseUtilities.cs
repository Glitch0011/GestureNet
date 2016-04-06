using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;

namespace GestureNet.WPFExample
{
    public static class MouseUtilities
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);

        [DllImport("user32.dll")]
        private static extern short GetKeyState(int pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        
        private static int ConvertMouse(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return 0x01;
                case MouseButton.Right:
                    return 0x02;
                case MouseButton.Middle:
                    return 0x04;
                case MouseButton.XButton1:
                case MouseButton.XButton2:
                default:
                    throw new ArgumentException();
            }
        }

        public static bool IsMouseButtonDown(MouseButton button) => GetKeyState(ConvertMouse(button)) < 0;

        public static Point GetMousePosition()
        {
            var w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
    }
}