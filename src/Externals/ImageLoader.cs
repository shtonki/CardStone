using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    /// <summary>
    /// It just werks
    /// </summary>
    class ImageLoader
    {
        private const int cruft = frames + steps;
        private const int frames = 7;
        private const int steps = 10;

        private static Dictionary<Size, Dictionary<string, Image>> imageMap;

        private static Dictionary<string, Image> dflt;
        private static Image nothing = Image.FromFile(Settings.resCard+"NOTHING.png");

        static ImageLoader()
        {
            imageMap = new Dictionary<Size, Dictionary<string, Image>>();
            dflt = new Dictionary<string, Image>();
        }
        
        public static Image getCardArt(CardId id, Size s)
        {
            return getImage(s, Settings.resCard + id + ".png");
        }

        public static Image getFrame(Colour colour, Size s)
        {
            return getImage(s, Settings.resFrame + colour + "3.png");
        }

        public static Image getStepImage(string file, Size s)
        {
            return getImage(s, Settings.resButton + file + ".png");
        }

        private static Image getImage(Size? s, string i)
        {
            if (s == null)
            {
                if (dflt.ContainsKey(i))
                {
                    return dflt[i];
                }
                Image m;
                try
                {
                    m = Image.FromFile(i);
                }
                catch (FileNotFoundException e)
                {
                    m = nothing;
                }
                dflt[i] = m;
                return dflt[i];
            }

            Dictionary<string, Image> images;
            if (imageMap.ContainsKey(s.Value))
            {
                images = imageMap[s.Value];
            }
            else
            {
                images = new Dictionary<string, Image>();
                imageMap.Add(s.Value, images);
            }
            if (!images.ContainsKey(i))
            {
                images[i] = resizeImage(getImage(null, i), s.Value.Width, s.Value.Height);
            }
            return images[i];
        }

        private static Image resizeImage(Image image, int width, int height)
        {
            if (width == 0) { width = 1; }
            if (height == 0) { height = 1; }

            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
