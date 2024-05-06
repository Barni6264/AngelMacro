using System;
using System.Drawing;
using System.Threading;


namespace AngelMacro
{
    public static class Condition
    {
        static int sLeft = (int)System.Windows.SystemParameters.VirtualScreenLeft;
        static int sWidth = (int)System.Windows.SystemParameters.VirtualScreenWidth;
        static int sTop = (int)System.Windows.SystemParameters.VirtualScreenTop;
        static int sHeight = (int)System.Windows.SystemParameters.VirtualScreenHeight;
        static Bitmap bmp = new Bitmap(sWidth, sHeight);
        static Graphics g = Graphics.FromImage(bmp);
        static Size size = new Size(sWidth,sHeight);

        public static Color GetPixel(int left, int top)
        {
            return bmp.GetPixel(left-sLeft, top-sTop);
        }

        public static void ScreenShot()
        {
            g.CopyFromScreen(sLeft, sTop, 0, 0, size);
        }

        public static Tuple<Point, Color> GetCursorInfo()
        {
            Point location = DLLs.GetMousePosition();
            Thread.Sleep(100);
            DLLs.SetMousePosition(0, 0);
            Thread.Sleep(100);
            Color color = GetPixel(location.X, location.Y);
            Thread.Sleep(100);
            DLLs.SetMousePosition(location.X, location.Y);

            return new Tuple<Point, Color>(location, color);
        }
    }
}
