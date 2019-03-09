using System;
using System.Drawing;

namespace ApexReportTool
{
    /// <summary>
    /// A class for Finding name tag in Apex screenshot
    /// Author: Xuan525
    /// Date: 09/03/2019
    /// </summary>
    class ApexLayout
    {
        public class ApexNameTagPosition
        {
            private const int StandardWidth = 1920;
            private const int StandardHeight = 1080;
            private const int AreaHeight = 50;
            private const int bottomMargin = 80;

            private int Height, Width;
            private Bitmap Bitmap;

            public ApexNameTagPosition(Bitmap bitmap)
            {
                Bitmap = bitmap;
                Height = bitmap.Height;
                Width = bitmap.Width;
            }

            private int GetAreaWidth()
            {
                int firstLineY = GetStartY();
                for (int x = Width / 2; x >= 0; x--)
                {
                    Color c = Bitmap.GetPixel(x, firstLineY);
                    if (c.R > 5 || c.G > 5 || c.B > 5)
                    {
                        return ((Width / 2 - x) * 2);
                    }
                }
                return Width;
            }

            private int GetAreaHeight()
            {
                return Math.Abs(AreaHeight * Height / StandardHeight);
            }

            private int GetStartY()
            {
                int y = (int)Math.Ceiling((double)(Height - (bottomMargin + AreaHeight) * Height / StandardHeight));
                return y + 1;
            }

            private int GetStartX()
            {
                int x = (int)Math.Ceiling((double)((Width - GetAreaWidth()) / 2));
                return x + 1;
            }

            public Point GetStartPoint()
            {
                return new Point(GetStartX(), GetStartY());
            }

            public Rectangle GetArea()
            {
                return new Rectangle(0, 0, GetAreaWidth(), GetAreaHeight());
            }
        }
    }
}
