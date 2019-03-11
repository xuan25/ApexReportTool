using System.Drawing;
using Tesseract;

namespace ApexReportTool
{
    /// <summary>
    /// A class for parsing player id
    /// Author: Xuan525
    /// Date: 11/03/2019
    /// </summary>
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

            TesseractEngine tesseractEngine = new TesseractEngine("./bin/tessdata", "eng", EngineMode.TesseractAndCube);
            Page page = tesseractEngine.Process(newbitmap);

            return page.GetText().Trim();
        }
    }
}
