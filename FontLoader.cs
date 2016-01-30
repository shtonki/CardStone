using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace stonekart
{
    /// <summary>
    /// ebin 10/10
    /// Isn't even used though xd.
    /// </summary>
    public class FontLoader
    {

        public const int
            MPLATIN = 0,
            MANGALB = 1,
            MAIANDRA = 2;

        private static FontFamily[] fontFamilies = new FontFamily[3];

        public static void init()
        {
            var a = new PrivateFontCollection();
            
            a.AddFontFile(@"res/FONT/mplantin.ttf");
            a.AddFontFile(@"res/FONT/MatrixBold.ttf");
            a.AddFontFile(@"res/FONT/Maiandra GD.ttf");

            fontFamilies[MPLATIN] = a.Families[0];
            fontFamilies[MANGALB] = a.Families[1];
            fontFamilies[MAIANDRA] = a.Families[2];
        } 

        public static Font getFont(int baseFont, int size)
        {
            size = size < 1 ? 1 : size;
            Font f = new Font(fontFamilies[baseFont],
                              size,
                              FontStyle.Regular,
                              GraphicsUnit.Pixel);
            return f;
        }

    }
}

