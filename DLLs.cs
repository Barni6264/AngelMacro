using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace AngelMacro
{
    public static class DLLs
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("user32.dll")]
        internal static extern bool GetCursorPos(ref Win32Point pt);
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        internal struct Win32Point
        {
            public int X;
            public int Y;
        };

        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);

            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static void SetMousePosition(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}
