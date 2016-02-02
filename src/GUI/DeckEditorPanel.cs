using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace stonekart
{
    class DeckEditorPanel : DisplayPanel
    {
        private const int FRAMEWIDTH = 1800;
        private const int FRAMEHEIGHT = 1000;
        private static System.Timers.Timer searchTimer;
        private System.Timers.Timer paintTimer;
        private const int resetTime = 800; //MS 
        public static List<Keys> searchString;
        private static Dictionary<Image, PortraitInfo> Portraits;
        private static Image arrowRight, arrowLeft, save; //imageflip xD:P
        private const int SCROLL_SPEED = 8;
        private static Tuple<bool, int> pressedAndDirection;
        private static List<CardId> deck;
        private static Point deckPoint = new Point(50, 100);
        private const int MAX_NR_OF_CARDS = 40;
        public CardId[] loadDeckFromFile()
        {
            CardId[] myDeck = new CardId[MAX_NR_OF_CARDS];
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"res\deck.txt"))
                {
                    for (int i = 0; i < MAX_NR_OF_CARDS; i++)
                    {
                        string line = sr.ReadLine();
                        if (line != null) myDeck[i] = (CardId)Enum.Parse(typeof(CardId), line);
                        Console.WriteLine(deck[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            return myDeck;
        }

        public bool addCardToDeck(CardId id)
        {
            if (deck.Count != 40)
            {
                Console.WriteLine("Added " + id);
                deck.Add(id);
                return true;
            }
            return false;
        }

        public bool removeCardFromDeck(CardId? id)
        {
            if (deck.Count == 0) return false;
            if (id != null) deck.Remove((CardId)id);
            return true;
            /*bool removed = false;
            for (int i = 0; i < MAX_NR_OF_CARDS; i++)
            {
                if (deck[i] == id)
                {
                    removed = true;
                    if (i != MAX_NR_OF_CARDS - 1)
                    {
                        for (int j = i + 1; j < MAX_NR_OF_CARDS; j++)
                        {
                            deck[i] = deck[j];
                        }
                    }
                    nrOfCardsAdded -= 1;
                    return removed;
                }
            }
            return removed;*/
        }

        class PortraitInfo
        {
            public Rectangle rectangle;
            public CardId? id;

            public PortraitInfo(Rectangle r, CardId? i)
            {
                rectangle = r;
                id = i;
            }
        }

        private void startPaintTimer()
        {
            paintTimer = new System.Timers.Timer(30);
            paintTimer.Start();
            paintTimer.Elapsed += repaint;
        }

        private void repaint(object sender, ElapsedEventArgs e)
        {
            if (pressedAndDirection.Item1)
            {
                scrollCards(pressedAndDirection.Item2);
            }
            Invalidate();
        }

        private static void setSearchTimerLoop()
        {
            searchTimer = new System.Timers.Timer(resetTime);
            searchTimer.Start();
            searchTimer.Elapsed += printSearch;
        }

        //todo(seba) rename this
        private static void printSearch(object sender, ElapsedEventArgs e)
        {
            string search = string.Join("", searchString);
            Console.WriteLine(search);
            searchString.Clear();
        }

        private static void resetTheTimer()
        {
            searchTimer.Stop();
            searchTimer.Start();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

        }

        public DeckEditorPanel()
        {
            BackColor = Color.LightSeaGreen;
            setSearchTimerLoop();
            startPaintTimer();
            searchString = new List<Keys>();
            Paint += vanGogh;
            MouseDown += OnDown;
            MouseUp += OnUp;

            deck = new List<CardId>();
            // <https://pbs.twimg.com/media/COz6-D_UEAA76ip.jpg>
            typeof(Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this, new object[] { true });
            // </https://pbs.twimg.com/media/COz6-D_UEAA76ip.jpg>

            Portraits = new Dictionary<Image, PortraitInfo>();
            Point p = new Point(100, FRAMEHEIGHT / 2);
            foreach (CardId id in Enum.GetValues(typeof(CardId)))
            {
                var img = ImageLoader.getCardArt(id);
                Portraits.Add(img, new PortraitInfo(new Rectangle(p, new Size(img.Width, img.Height)), id));
                p.X += img.Width;
            }

            //xd no flip 360 with the wrist boy
            arrowRight = Image.FromFile(@"res\IMG\button\arrowRight.png");
            arrowLeft = Image.FromFile(@"res\IMG\button\arrowLeft.png");
            save = Image.FromFile(@"res\IMG\button\save.png");
            Portraits.Add(arrowRight, new PortraitInfo(new Rectangle(new Point(FRAMEWIDTH - arrowRight.Width * 2,
                FRAMEHEIGHT / 3 + arrowRight.Height / 2), new Size(arrowRight.Width, arrowRight.Height)), null));

            Portraits.Add(arrowLeft, new PortraitInfo(new Rectangle(new Point(32, FRAMEHEIGHT / 3 + arrowLeft.Height / 2),
                new Size(arrowLeft.Width, arrowLeft.Height)), null));

            Portraits.Add(save, new PortraitInfo(new Rectangle(new Point(FRAMEWIDTH - 128, 50), new Size(save.Width, save.Height)), null));

        }

        public static void pressedArrow(int direction)
        {
            pressedAndDirection = new Tuple<bool, int>(true, direction);
        }

        private void OnUp(object sender, MouseEventArgs e)
        {
            Console.WriteLine("xD");
            pressedAndDirection = new Tuple<bool, int>(false, 0);
        }

        private void OnDown(object sender, MouseEventArgs e)
        {
            foreach (var portrait in Portraits)
            {
                if (portrait.Value.rectangle.Contains(e.Location))
                {
                    if (portrait.Value.id == null)
                    {
                        if (e.X < FRAMEWIDTH / 2)
                        {
                            pressedAndDirection = new Tuple<bool, int>(true, -1);//scrollCards(-1);
                        }
                        else
                        {
                            if (e.Y < 100)
                            {
                                saveDeck();
                            }
                            else
                            {
                                pressedAndDirection = new Tuple<bool, int>(true, 1);//scrollCards(1);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("clicked " + portrait.Value.id);
                        Console.WriteLine(e.Button.ToString());
                        if (e.Button == MouseButtons.Left)
                        {
                            if (portrait.Value.id != null)
                            {
                                addCardToDeck((CardId)portrait.Value.id);
                            }
                        }
                        else if (e.Button == MouseButtons.Right)
                        {
                            removeCardFromDeck(portrait.Value.id);
                        }
                    }
                }
            }
            Invalidate();
        }

        private void saveDeck()
        {
            //todo if not 40 cards yell at user xD
            System.IO.StreamWriter file = new System.IO.StreamWriter(@"res\deck.txt");
            foreach (var id in deck)
            {
                file.WriteLine(id);
            }
            file.Close();
        }

        private void scrollCards(int direction)
        {
            foreach (var port in Portraits)
            {
                if (port.Value.id != null)
                {
                    port.Value.rectangle.X = port.Value.rectangle.X + direction * SCROLL_SPEED;
                }
            }
            Invalidate();
        }

        public static void addKeyToSearch(Keys k)
        {
            if (k >= Keys.D0 && k <= Keys.Z)
            {
                searchString.Add(k);
                resetTheTimer();
            }
            else if (k == Keys.Left || k == Keys.Right)
            {
                pressedAndDirection = new Tuple<bool, int>(false, 0);
            }
        }

        public static Image resizeImage(Image imgToResize, Size size)
        {
            return (Image)(new Bitmap(imgToResize, size));
        }

        private static void vanGogh(object sender, PaintEventArgs e)
        {
            //why are we doing this every paint? XDDDDDDDDDDDDDDDDD
            Font muhFont = new Font("Arial", 24);
            SolidBrush muhBrush = new SolidBrush(Color.BlanchedAlmond);
            int collumns = 3;
            string search = string.Join("", searchString); ;
            e.Graphics.DrawString(search, muhFont, muhBrush,
                new PointF(FRAMEWIDTH / 2, FRAMEHEIGHT / 3));
            e.Graphics.DrawString("Card Count: " + deck.Count, muhFont, muhBrush, new PointF(50, 50));

            foreach (var por in Portraits)
            {
                if (por.Value.id.ToString().ToUpper().Contains(search) || por.Value.id == null)
                    e.Graphics.DrawImage(por.Key, por.Value.rectangle);
                if (por.Value.id != null)
                    e.Graphics.DrawString(por.Value.id.ToString() + "   " + nrOfThisCard(por.Value.id), muhFont, muhBrush,
                        new PointF(deckPoint.X + (((int)(CardId)por.Value.id) / 3) * 3 * 128, deckPoint.Y + (28 * ((int)por.Value.id % collumns)))); //xd
            }
        }

        private static int nrOfThisCard(CardId? id)
        {
            int count = 0;
            for (int i = 0; i < deck.Count; i++)
            {
                if (deck[i] == id) count++;
            }
            return count;
        }
    }
}