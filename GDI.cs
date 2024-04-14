using System;
using System.Drawing;
using System.Threading;

namespace AngelMacro
{
    public static class GDI
    {
        public static void WriteToScreen(int left, int top, Bitmap bitmap)
        {
            using (Graphics graphics = Graphics.FromHdc(DLLs.GetDC(IntPtr.Zero)))
            {
                graphics.DrawImage(bitmap, left, top);
            }
        }

        public static void WriteDuration(int left, int top, Bitmap bitmap, int msDuration)
        {
            new Thread(() =>
            {
                bool threadRunning = true;
                new Thread(() =>
                {
                    while (threadRunning)
                    {
                        WriteToScreen(left, top, bitmap);
                    }
                }).Start();
                Thread.Sleep(msDuration);
                threadRunning = false;
            }).Start();
        }

        public static void WriteTextDuration(int left, int top, string text, int msDuration, Brush bgBrush, Brush fgBrush)
        {
            Font font = new Font("consolas", 20);
            Bitmap bitmap = new Bitmap(text.Length * (int)font.Size, font.Height);
            Graphics graphics = Graphics.FromImage(bitmap);
            Rectangle rectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            graphics.FillRectangle(bgBrush, rectangle);
            graphics.DrawString(text, font, fgBrush, rectangle);
            WriteDuration(left, top, bitmap, msDuration);
        }
    }
}
