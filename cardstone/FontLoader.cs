using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace stonekart
{
    public class FontLoader
    {

        public const int
            COMICSANS = 0,
            WINGDINGS = 1;

        private static FontFamily[] fontFamilies = new FontFamily[2];

        public static void init()
        {
            var a = new PrivateFontCollection();

            a.AddFontFile(@"res/FONT/COMICSANS.ttf");
            a.AddFontFile(@"res/FONT/WINGDINGS.ttf");

            fontFamilies[COMICSANS] = a.Families[0];
            fontFamilies[WINGDINGS] = a.Families[1];
        } //

        public static Font getFont(int baseFont, int size)
        {
            Font f = new Font(fontFamilies[baseFont],
                              size,
                              FontStyle.Regular,
                              GraphicsUnit.Pixel);
            return f;
        }

    }
}

