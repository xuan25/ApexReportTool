using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ApexReportTool
{
    /// <summary>
    /// A class for Processing screenshots in Apex
    /// Author: Xuan525
    /// Date: 09/03/2019
    /// </summary>
    class Screenshot
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(
            string lpClassName, 
            string lpWindowName
            );

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(
            IntPtr hwnd
            );

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(
            IntPtr hwnd, 
            ref Rectangle rectangle
            );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(
               IntPtr hdc, // handle to DC
               int nWidth, // width of bitmap, in pixels
               int nHeight // height of bitmap, in pixels
               );

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(
                IntPtr hdc // handle to DC
                );

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(
               IntPtr hdc, // handle to DC
               IntPtr hgdiobj // handle to object
               );

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(
               IntPtr hwnd, // Window to copy,Handle to the window that will be copied. 
               IntPtr hdcBlt, // HDC to print into,Handle to the device context. 
               UInt32 nFlags // Optional flags,Specifies the drawing options. It can be one of the following values. 
               );

        [DllImport("gdi32.dll")]
        public static extern int DeleteDC(
               IntPtr hdc // handle to DC
               );

        [DllImport("gdi32.dll")]
        public static extern int DeleteObject(
               IntPtr hdc
               );

        public static Bitmap GetImg(IntPtr hWnd)
        {
            IntPtr hscrdc = GetWindowDC(hWnd);
            Rectangle windowRect = new Rectangle();
            GetWindowRect(hWnd, ref windowRect);
            int width = Math.Abs(windowRect.X - windowRect.Width);
            int height = Math.Abs(windowRect.Y - windowRect.Height);
            IntPtr hbitmap = CreateCompatibleBitmap(hscrdc, width, height);
            IntPtr hmemdc = CreateCompatibleDC(hscrdc);
            SelectObject(hmemdc, hbitmap);
            PrintWindow(hWnd, hmemdc, 0);
            Bitmap bmp = Image.FromHbitmap(hbitmap);
            DeleteDC(hscrdc);
            DeleteObject(hbitmap);
            DeleteDC(hmemdc);
            return bmp;
        }

        public static Bitmap GetImg(string windowName)//得到窗口截图
        {
            IntPtr hWnd = FindWindow(null, "Apex Legends");
            return GetImg(hWnd);
        }

        public static Bitmap CropImage(Bitmap SourceImage, Point StartPoint, Rectangle CropArea)
        {
            if (CropArea.Width <= 0)
                return null;
            Bitmap NewBitmap = new Bitmap(CropArea.Width, CropArea.Height);
            Graphics tmpGraph = Graphics.FromImage(NewBitmap);
            tmpGraph.DrawImage(SourceImage, CropArea, StartPoint.X, StartPoint.Y, CropArea.Width, CropArea.Height, GraphicsUnit.Pixel);
            tmpGraph.Dispose();
            return NewBitmap;
        }

        public static void Monochrome(Bitmap bitmap)
        {
            for (int x = 0; x < bitmap.Width; x++)
                for (int y = 0; y < bitmap.Height; y++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    if(c.R < 1 && c.G < 1 && c.G < 1)
                        bitmap.SetPixel(x, y, Color.White);
                    else
                        bitmap.SetPixel(x, y, Color.Black);
                }
        }

    }
}
