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
            MPLATIN = 0,
            MANGALB = 1,
            MAIANDRA = 2;

        private static FontFamily[] fontFamilies = new FontFamily[2];

        public static void init()
        {
<<<<<<< HEAD
            //var a = new PrivateFontCollection();

            //a.AddFontFile(@"res/FONT/COMICSANS.ttf");
            //a.AddFontFile(@"res/FONT/WINGDINGS.ttf");

            //fontFamilies[COMICSANS] = a.Families[0];
            //fontFamilies[WINGDINGS] = a.Families[1];
        } //
=======
            var a = new PrivateFontCollection();
            
            a.AddFontFile(@"res/FONT/mplantin.ttf");
            a.AddFontFile(@"res/FONT/mangalb.ttf");
            a.AddFontFile(@"res/FONT/Maiandra GD.ttf");

            fontFamilies[MPLATIN] = a.Families[0];
            fontFamilies[MANGALB] = a.Families[1];
            fontFamilies[MAIANDRA] = a.Families[2];
        } 
>>>>>>> de9db7d06e070f31eabf2bd57b810562a38d2a57

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

