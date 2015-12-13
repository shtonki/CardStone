using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    class ImageLoader
    {
        private const string cardArtPath = @"res/IMG/card/";
        private const string framePath = @"res/IMG/frame/";

        private static Dictionary<CardId, Image> imageMap;

        private static Image frame;

        public static void init()
        {
            frame = Image.FromFile(framePath + "basicBlue" + ".png");

            imageMap = new Dictionary<CardId, Image>();
        }

        public static Image getCardArt(CardId id)
        {
            if (imageMap.ContainsKey(id))
            {
                return imageMap[id];
            }
            Image i = Image.FromFile(cardArtPath + id + ".png");
            imageMap.Add(id, i);
            return i;
        }

        public static Image getFrame()
        {
            return frame;
        }
    }
}
