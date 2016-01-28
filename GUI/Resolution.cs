using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;

namespace stonekart
{

    abstract public class Resolution
    {
        public static Res currentResolution;
        public static Resolutions currentResolutionx => currentResolution.resolution;

        static Resolution()
        {
            currentResolution = new Res(Resolutions.w1800x1000);
        }

        public static int get(string k)
        {
            ElementDimensions x;
            if (!Enum.TryParse(k, true, out x))
            {
                return -1;
            }
            return currentResolution.get(x);
        }

        public static int get(ElementDimensions d)
        {
            return currentResolution.get(d);
        }
        

        public static void set(string k, int v)
        {
            ElementDimensions x;
            if (!Enum.TryParse(k, true, out x))
            {
                return;
            }
            currentResolution.set(x, v);
        }

        public static void set(ElementDimensions d, int v)
        {
            currentResolution.set(d, v);
        }

        public static IEnumerable<Tuple<ElementDimensions, int>> getAllPairs()
        {
            return currentResolution.getAllPairs();
        }

        public static void save()
        {
            currentResolution.save();
        }
    }

    public class Res
    {
        public void scale(int n, int d)
        {
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (values[i]*n)/d;
                if (values[i] < 1)
                {
                    values[i] = 1;
                }
            }
        }

        private int[] values;

        public Resolutions resolution { get; private set; }
        
        public Res(Resolutions s)
        {
            resolution = s;

            values = new int[Enum.GetNames(typeof(ElementDimensions)).Length];

            FileStream f = File.Open(resolution + ".xml",FileMode.Open);
            using (XmlReader reader = XmlReader.Create(new StreamReader(f)))
            {
                while (true)
                {
                    if (!reader.ReadToFollowing("dim"))
                    {
                        break;
                    }
                    reader.MoveToFirstAttribute();
                    string dim = reader.Value;
                    ElementDimensions x;
                    if (!Enum.TryParse(dim, false, out x))
                    {
                        Console.WriteLine("shitty " + dim);
                        continue;
                    }

                    reader.ReadToFollowing("val");
                    int val = reader.ReadElementContentAsInt();
                    set(x, val);
                }
                f.Close();
                f.Dispose();
            }

        }
        

        public int get(ElementDimensions d)
        {
            return values[(int)d];
        }
        

        public void set(ElementDimensions d, int v)
        {
            values[(int)d] = v;
        }

        public IEnumerable<Tuple<ElementDimensions, int>> getAllPairs()
        {
            for (int i = 0; i < values.Length; i++)
            {
                yield return new Tuple<ElementDimensions, int>((ElementDimensions)i, values[i]);
            }
        }


        public void save()
        {
            using (StreamWriter writer = new StreamWriter(resolution + ".xml"))
            {
                writer.WriteLine("<resolution>");
                foreach (Tuple<ElementDimensions, int> v in getAllPairs())
                {
                    writer.Write("  <dim name=\"");
                    writer.Write(v.Item1.ToString());
                    writer.Write("\">\n    <val>");
                    writer.Write(v.Item2.ToString());
                    writer.Write("</val>\n  </dim>\n\n");
                }

                writer.WriteLine("</resolution>");
            }
        }
    }

    public enum ElementDimensions
    {
        FrameHeight,
        FrameWidth,

        CardButtonHeight,
        CardButtonWidth,
        CardButtonArtHeight,
        CardButtonArtWidth,
        CardButtonArtLocationX,
        CardButtonArtLocationY,
        CardButtonNameLocationX,
        CardButtonNameLocationY,
        CardButtonNameFontSize,
        CardButtonTextFontSize,
        CardButtonPTFontSize,
        CardButtonTypeTextLocationX,
        CardButtonTypeTextLocationY,
        CardButtonTextLocationX,
        CardButtonTextLocationY,
        CardButtonTextWidth,
        CardButtonTextHeight,
        CardButtonManaOrbLocationX,
        CardButtonManaOrbLocationY,
        CardButtonManaOrbSize,
        CardButtonManaOrbPadding,
        CardButtonManaOrbNumberLocationY,
        CardButtonPTAreaSize,
        CardButtonPTTextLocationP,
        CardButtonPTTextLocationY,
        CardButtonPTTextLocationT,

        HandPanelHeight,
        HandPanelWidth,
        HandPanelLocationX,
        HandPanelLocationY,


        ChoicePanelHeight,
        ChoicePanelWidth,
        ChoicePanelLocationX,
        ChoicePanelLocationY,


    }

    public enum Resolutions
    {
        w1800x1000,
        w900x500,
    }

    public interface Resolutionable
    {
        void updateResolution();
        
    }
}