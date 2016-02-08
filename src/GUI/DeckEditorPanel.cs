using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Linq;
namespace stonekart
{
    class DeckEditorPanel : DisplayPanel
    {
        
        Button saveButton, loadButton;
        Button[] sortButtons;
        CardPanel p;
        CardButton[] cards;
        Label noDeckName;
        TextBox tb;
        Card[] ids;
        Pile myDeckIsHard;
        const int paddingX = 8;
        const int paddingY = 8;
        const int CARDS_PER_ROW = 6;
        Colour currentSortingColor;
        public DeckEditorPanel()
        {
            
            BackColor = Color.Beige;
            currentSortingColor = Colour.GREY;
            int nrOfCards = Enum.GetValues(typeof(CardId)).Length;
            cards = new CardButton[nrOfCards];
            myDeckIsHard = new Pile(new Card[] { });
            p = new CardPanel(new Func<CardButton>(() => new CardButton()), new LayoutArgs(true, true));
            myDeckIsHard.addObserver(p);
            ids =
                ((CardId[])Enum.GetValues(typeof (CardId))).Select(id => new Card(id))
                    .OrderBy(card => card.colour)
                    .ToArray();
            for (int i = 0; i < nrOfCards; i++)
            {
                int i0 = i;
                cards[i] = new CardButton(ids[i].cardId);
                Controls.Add(cards[i]);
                cards[i].MouseDown += (_, __) =>
                {
                    if (__.Button == MouseButtons.Left) addToDeck(cards[i0].Card.cardId);
                    else if (__.Button == MouseButtons.Right) removeFromDeck(cards[i0].Card.cardId);
                };
            }
            noDeckName = new Label();
            noDeckName.Text = "Every deck needs a name.";
            noDeckName.Visible = false;
            noDeckName.Size = new Size(250, 25);
            noDeckName.BackColor = Color.Red;
            loadButton = new Button();
            loadButton.Image = ImageLoader.getStepImage("load", new Size(60, 60));
            loadButton.Size = loadButton.Image.Size;
            loadButton.MouseDown += (_, __) =>
            {
                loadDeckFromFile((s) => loadIntoEditor(loadDeck(s)));
            };
            
            saveButton = new Button();
            saveButton.Image = ImageLoader.getStepImage("save", new Size(60, 60));
            saveButton.Size = saveButton.Image.Size;
            saveButton.MouseDown += (_, __) =>
            {
                if (tb.Text != "")
                    saveDeck();
                else
                {
                    noDeckName.Visible = true;
                }
            };

            sortButtons = new Button[5];
            //todo use images or something, instead of solid colors
            Color[] colors = new Color[5] {Color.White, Color.Blue, Color.Black, Color.Red, Color.Green};
            for(int i = 0; i < sortButtons.Count(); i++)
            {
                //todo ask seba why i0 is needed
                
                int i0 = i;
                sortButtons[i] = new Button();
                sortButtons[i].Tag = (Colour)i;
                sortButtons[i].BackColor = colors[i];
                sortButtons[i].Location = new Point(Size.Width / 2 + 50 * i, 50);
                sortButtons[i].Size = new Size(50, 50); 
                sortButtons[i].MouseDown += (_, __) =>
                {
                    sortAfterColor((Colour)sortButtons[i0].Tag);
                };
                Controls.Add(sortButtons[i]);
            }

            tb = new TextBox();
            Controls.Add(noDeckName);
            Controls.Add(loadButton);
            Controls.Add(tb);
            Controls.Add(p);
            Controls.Add(saveButton);
        }

        private void sortAfterColor(Colour colour)
        {
            if (currentSortingColor == colour) currentSortingColor = Colour.GREY;
            else currentSortingColor = colour;
            //List <CardButton> newButtonsToDraw = new List<CardButton>();
            for (int i = 0; i < ids.Count(); i++)
            {
                if(ids[i].colour != currentSortingColor && currentSortingColor != Colour.GREY)
                {
                    cards[i].Hide();
                }
                else
                {
                    //newButtonsToDraw.Add(cards[i]);
                    cards[i].Show();
                }
            }
            //drawTheseButtons(newButtonsToDraw.ToArray());
        }
        private void saveDeck()
        {
            StreamWriter file = new StreamWriter(tb.Text + ".jas");
            foreach (var id in myDeckIsHard.cards)
            {
                file.WriteLine(id.cardId);
            }
            file.Close();
        }

        public static void loadDeckFromFile(Action<string> buttonClickedCallBack)
        {
            var deckNames = Directory.GetFiles(".").Where(x => x.EndsWith(".jas")).Select(x => x.Substring(2)).ToArray();
            Panel deckAsker = new Panel();
            deckAsker.Size = new Size(500, 200);
            //deckAsker.Location = new Point(Size.Width / 2, (Size.Height / 3) * 2);
            int Y = 0;
            var g = GUI.showWindow(deckAsker);
            foreach (string name in deckNames)
            {
                var xd = new Button();
                xd.Text = name;
                xd.Location = new Point(0, Y);
                Y += xd.Height;

                var name1 = name;
                xd.MouseDown += (_, __) =>
                {
                    buttonClickedCallBack(name1);
                    g.close();
                };

                if (deckAsker.InvokeRequired)
                {
                    deckAsker.Invoke(new Action(() =>
                    {
                        deckAsker.Controls.Add(xd);
                    }));
                }
                else
                {
                    deckAsker.Controls.Add(xd);
                }
            }
        }

        public static List<CardId> loadDeck(string deckName)
        {
            List<CardId> myDeck = new List<CardId>();
            try
            {
                using (StreamReader sr = new StreamReader(deckName))
                {
                    while(!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line != null) myDeck.Add((CardId)Enum.Parse(typeof(CardId), line));
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

        private void loadIntoEditor(List<CardId> deck)
        {
            myDeckIsHard.clear();
            for (int i = 0; i < deck.Count; i++)
            //for (int i = 0; i < myDeckIsHard.count; i++)
            {
                myDeckIsHard.add(new Card(deck[i]));
                //todo: remove cards from pile by pressing pile cards
                /*
                var pileButton = new CardButton(deck[i]);
                pileButton.Location = new Point(750, 250);
                pileButton.Size = new Size(400,400);
                Controls.Add(pileButton);
                pileButton.MouseDown += (_, __) =>
                {
                    Console.WriteLine("xD");
                };*/
            }
            
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            tb.Size = new Size(Size.Width / 10, Size.Height / 30);
            tb.Location = new Point(Size.Width / 2, Size.Height / 40);
            saveButton.Location = new Point(Size.Width - saveButton.Image.Width, 0);
            p.Size = new Size(cards[0].Width, Size.Height);
            loadButton.Location = new Point(saveButton.Location.X, saveButton.Location.Y+saveButton.Height);
            for(int i = 0; i < sortButtons.Count(); i++)
            {
                //ishigity
                sortButtons[i].Location = new Point(Size.Width / 2 + 50 * i - 35, Size.Height / 20);
            }

            noDeckName.Location = new Point(tb.Location.X, tb.Location.Y-20);
            noDeckName.Size = tb.Size;
            drawTheseButtons(cards);
            /*for (int i = 0; i < cards.Length; i++)
            {
                x += cards[i].Width;
                if (i % CARDS_PER_ROW == 0) x = cards[0].Size.Width * 2;
                cards[i].setWidth(Size.Width / cards.Length);
                cards[i].Location = new Point(x + (i % CARDS_PER_ROW) * paddingX, cards[i].Size.Height + cards[i].Size.Height * (i / CARDS_PER_ROW));
            }*/
        }

        private void drawTheseButtons(CardButton[] cards)
        {
            int x = cards[0].Size.Width;
            for (int i = 0; i < cards.Length; i++)
            {
                x += cards[i].Width;
                if (i % CARDS_PER_ROW == 0) x = cards[0].Size.Width * 2;
                cards[i].setWidth(Size.Width / cards.Length);
                cards[i].Location = new Point(x + (i % CARDS_PER_ROW) * paddingX, cards[i].Size.Height + cards[i].Size.Height * (i / CARDS_PER_ROW));
            }
        }

        private bool addToDeck(CardId id)
        {
            myDeckIsHard.add(new Card(id));
            return true;
        }

        private bool removeFromDeck(CardId id)
        {
            for(int i = myDeckIsHard.count-1; i >= 0; i--)
            {
                if (myDeckIsHard[i].cardId == id)
                {
                    myDeckIsHard.remove(myDeckIsHard[i]);
                    return true;
                }
            }
            return false;
        }
    }
}