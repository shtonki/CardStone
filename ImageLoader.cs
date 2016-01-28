using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
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
        private const string cardArtPath = @"res/IMG/card/";
        private const string framePath = @"res/IMG/frame/";

        private static Dictionary<CardId, Image> imageMap;

        private static Image frame;

        static ImageLoader()
        {
            updateResolution();
        }

        public static void updateResolution()
        {
            int width = Resolution.get(ElementDimensions.CardButtonWidth);
            int height = Resolution.get(ElementDimensions.CardButtonHeight);

            frame = resizeImage(Image.FromFile(framePath + "white2" + ".png"), width, height);

            imageMap = new Dictionary<CardId, Image>();
        }

        public static Image getCardArt(CardId id)
        {
            if (imageMap.ContainsKey(id))
            {
                return imageMap[id];
            }

            int width = Resolution.get(ElementDimensions.CardButtonArtWidth);
            int height = Resolution.get(ElementDimensions.CardButtonArtHeight);

            Image i = Image.FromFile(cardArtPath + id + ".png");
            Image rz = resizeImage(i, width, height);
            imageMap.Add(id, rz);
            //rz.Save(id.ToString() + ".jpg");
            return i;
        }

        public static Image getFrame()
        {
            return frame;
        }

        private static Image resizeImage(Image image, int width, int height)
        {
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
