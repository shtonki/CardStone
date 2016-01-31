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
        //Button, GameElement 
    {
        private static System.Timers.Timer searchTimer;
        private  System.Timers.Timer paintTimer;
        private static Image save;
        private static List<CardId> deck; 
        private const int MAX_NR_OF_CARDS = 40;
        private static List<Card> cards; 
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
                deck.Add(id);
                return true;
            }
            return false;
        }

        public bool removeCardFromDeck(CardId? id)
        {
            if (deck.Count == 0) return false;
            if (id != null) deck.Remove((CardId) id);
            return true;
        }

        private void startPaintTimer()
        {
            paintTimer = new System.Timers.Timer(30);
            paintTimer.Start();
            paintTimer.Elapsed += repaint;
        }

        private void repaint(object sender, ElapsedEventArgs e)
        {
            Invalidate();
        }

        public DeckEditorPanel()
        {
            BackColor = Color.LightSeaGreen;
            startPaintTimer();
            Paint += vanGogh;
            MouseDown += OnDown;
            MouseUp += OnUp;
            deck = new List<CardId>();
            cards = new List<Card>();
            foreach (CardId c in Enum.GetValues(typeof (CardId)))
            {
                new Card(c); //cards.Add(new Card(c));
            }
            
            // <https://pbs.twimg.com/media/COz6-D_UEAA76ip.jpg>
            typeof (Panel).InvokeMember("DoubleBuffered",
                BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this, new object[] {true});
        }


        private void OnUp(object sender, MouseEventArgs e)
        {
            
        }

        private void OnDown(object sender, MouseEventArgs e)
        {
            
        }

        private void saveDeck()
        { 
            //todo if not 40 cards yell at user xD
            StreamWriter file = new StreamWriter(@"res\deck.txt");
            foreach (var id in deck)
            {
                file.WriteLine(id);
            }
            file.Close();
        }

        private static void vanGogh(object sender, PaintEventArgs e)
        {
            //why are we doing this every paint? XDDDDDDDD:PDDDDDDDDD
            Font muhFont = new Font("Arial", 24);
            SolidBrush muhBrush = new SolidBrush(Color.BlanchedAlmond);
            e.Graphics.DrawString("Card Count: " + deck.Count, muhFont, muhBrush, new PointF(50, 50)); 
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
