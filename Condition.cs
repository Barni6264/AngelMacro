using System;
using System.Drawing;
using System.Threading;

namespace AngelMacro
{
    public static class Condition
    {
        public static Color GetPixel(int left, int top)
        {
            Bitmap bmp = new Bitmap(1, 1);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(left, top, 0, 0, new Size(1, 1));
            return bmp.GetPixel(0, 0);
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
