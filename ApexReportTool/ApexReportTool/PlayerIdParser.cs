using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Tesseract;

namespace ApexReportTool
{
    class PlayerIdParser
    {
        public delegate void BitmapDel(Bitmap bitmap);
        public event BitmapDel PlayerIdFound;
        public event BitmapDel PlayerIdMonochrome;

        public string Parse()
        {
            Bitmap bitmap = Screenshot.GetImg("Apex Legends");

            if (bitmap.Width < 2 || bitmap.Height < 2)
            {
                return null;
            }

            ApexLayout.ApexNameTagPosition tagPosition = new ApexLayout.ApexNameTagPosition(bitmap);

            Rectangle area = tagPosition.GetArea();
            if (area.Width <= 0)
            {
                return null;
            }

            Bitmap newbitmap = Screenshot.CropImage(bitmap, tagPosition.GetStartPoint(), area);

            PlayerIdFound?.Invoke(newbitmap);

            Screenshot.Monochrome(newbitmap);

            PlayerIdMonochrome?.Invoke(newbitmap);

            TesseractEngine tesseractEngine = new TesseractEngine("./tessdata", "eng", EngineMode.TesseractAndCube);
            Page page = tesseractEngine.Process(newbitmap);

            return page.GetText().Trim();
        }
    }
}
