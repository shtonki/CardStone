using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Linq;

/*todo:
    just hide scroll buttons when you are at last page and first page?
    list of filters
    make cardcount textthing instead of label, so we can change size and what not
    make unable to save deck that arent approved by shitstain
    create sub categories which shows uncompleted and completed decks. Or legit and illegit decks or watchamacallit
*/
namespace stonekart
{
    class DeckEditorPanel : DisplayPanel
    {
        private CardInfoPanel cardInfo;
        private Button saveButton, loadButton, backToMainMenuButton;
        private Button[] sortButtons;
        private Button scrollLeftButton, scrollRightButton;
        private CardButton[] cardSlot;
        private CardPanel p;
        private CardButton[] cards;
        private Label noDeckName, cardCount;
        private static TextBox tb;
        private Card[] ids;
        private List<Card> sortedIds;
        private Pile myDeckIsHard;
        private const int paddingX = 8;
        private const int paddingY = 8;
        private const int CARDS_PER_ROW = 6;
        private int currentPage;
        private const int cardsPerPage = 8;
        private Colour currentSortingColor;
        private List<Func<Card, bool>> filters;

        private Func<Card, bool> isRed;
        private Func<Card, bool> isBlue;
        private Func<Card, bool> isGreen;
        private Func<Card, bool> isBlack;
        private Func<Card, bool> isWhite;
        private Func<Card, bool> isGrey;
        private Func<Card, bool> isMulti;
        public DeckEditorPanel()
        {
            BackColor = Color.Beige;
            currentSortingColor = Colour.GREY;
            int nrOfCards = Enum.GetValues(typeof(CardId)).Length;
            currentPage = 0;
            cards = new CardButton[nrOfCards];
            sortedIds = new List<Card>();
            myDeckIsHard = new Pile(new Card[] { });
            p = new CardPanel(new Func<CardButton>(() => new CardButton()), new LayoutArgs(true, true));
            myDeckIsHard.addObserver(p);
            ids =
                ((CardId[])Enum.GetValues(typeof (CardId))).Select(id => new Card(id))
                    .OrderBy(card => card.colour)
                    .ToArray();
            cardInfo = new CardInfoPanel();
            cardInfo.BackColor = BackColor;
            Controls.Add(cardInfo);
            cardSlot = new CardButton[8];
            cardCount = new Label();

            //todo jasin: dont use color, use mana instead, otherwise it wont work with cards that are multicolored
            filters = new List<Func<Card, bool>>();
            isRed = c => c.colour == Colour.RED;
            isBlue = c => c.colour == Colour.BLUE;
            isGreen = c => c.colour == Colour.GREEN;
            isBlack = c => c.colour == Colour.BLACK;
            isWhite = c => c.colour == Colour.WHITE;
            isGrey = c => c.colour == Colour.GREY;
            isMulti = c => c.colour == Colour.MULTI;
            //addFilters(isWhite, isBlue, isBlack, isRed, isGreen, isGrey, isMulti);


            for (int i = 0; i < nrOfCards; i++)
            {
                cards[i] = new CardButton(ids[i].cardId);
                sortedIds.Add(ids[i]);
            }
            
            for (int i = 0; i < cardsPerPage; i++)
            {
                int i0 = i;
                cardSlot[i] = cards[i];
                cardSlot[i].Size = new Size(200,300);
                Controls.Add(cardSlot[i]);
                cardSlot[i].MouseDown += (_, __) =>
                {
                    if (__.Button == MouseButtons.Left) addToDeck(cards[i0].Card.cardId);
                    else if (__.Button == MouseButtons.Right) removeFromDeck(cards[i0].Card.cardId);
                };
                cardSlot[i].MouseHover += (_, __) =>
                {
                    cardInfo.showCard(cardSlot[i0].Card);
                };
            }

            scrollLeftButton = new Button();
            scrollRightButton = new Button();
            scrollLeftButton.Image = ImageLoader.getStepImage("arrowLeft", new Size(60, 60));
            scrollRightButton.Image = ImageLoader.getStepImage("arrowRight", new Size(60, 60));

            //todo: you need to double press if you scroll one way then the other. 
            scrollLeftButton.MouseDown += (_, __) =>
            {
                if (currentPage > 0)
                {
                    currentPage--;
                    int cardSlotNr = cardsPerPage - 1;
                    for (int i = currentPage*cardsPerPage + cardsPerPage - 1; i > currentPage*cardsPerPage - 1; i--)
                    {
                        cardSlot[cardSlotNr].Visible = true;
                        cardSlot[cardSlotNr].notifyObserver(new Card(sortedIds[i].cardId), null);
                        cardSlotNr--;
                    }
                }
            };
            scrollRightButton.MouseDown += (_, __) =>
            {
                //this if doesn't even make sense, but it just werks. todo 
                if (currentPage * cardsPerPage - cardsPerPage < sortedIds.Count - cardsPerPage * 2) currentPage++;
                int cardSlotNr = 0;
                for (int i = currentPage * cardsPerPage - cardsPerPage; i < currentPage * cardsPerPage; i++)
                {
                    if (i < sortedIds.Count - cardsPerPage)
                    {
                        cardSlot[cardSlotNr].Visible = true;
                        cardSlot[cardSlotNr].notifyObserver(new Card(sortedIds[i + cardsPerPage].cardId), null);
                    }
                    else cardSlot[cardSlotNr].Visible = false;
                    cardSlotNr++;
                }
            };

            backToMainMenuButton = new Button();
            backToMainMenuButton.Size = new Size(120, 40);
            backToMainMenuButton.Text = "back to main menu";
            backToMainMenuButton.MouseDown += (_, __) =>
            {
                GUI.transitionToMainMenu();
            };

            Controls.Add(scrollLeftButton);
            Controls.Add(scrollRightButton);

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

            sortButtons = new Button[7];
            //todo use images or something, instead of solid colors
            Color[] colors = new Color[7] {Color.White, Color.Blue, Color.Black, Color.Red, Color.Green, Color.Gray, Color.Brown};
            for(int i = 0; i < sortButtons.Count(); i++)
            {
                //todo ask seba why i0 is needed
                
                int i0 = i;
                sortButtons[i] = new Button();
                sortButtons[i].Tag = (Colour)i;
                sortButtons[i].BackColor = colors[i];
                sortButtons[i].Size = new Size(50, 50); 
                sortButtons[i].MouseDown += (_, __) =>
                {
                    sortAfterColor((Colour)sortButtons[i0].Tag);
                };
                Controls.Add(sortButtons[i]);
            }


            tb = new TextBox();
            Controls.Add(cardCount);
            Controls.Add(saveButton);
            Controls.Add(noDeckName);
            Controls.Add(loadButton);
            Controls.Add(tb);
            Controls.Add(p);
            Controls.Add(backToMainMenuButton);
        }

        private void addFilters(params Func<Card, bool>[] funcs)
        {
            foreach (var f in funcs)
            {
                filters.Add(f);
            }
        }

        public static bool deckVerificationThing(CardId[] ids)
        {
            const int minDeckSize = 25;
            if (ids.Count() < minDeckSize) return false;

            int nrOfIds = Enum.GetNames(typeof (CardId)).Length;
            int[] ctrs = new int[nrOfIds];
            foreach (CardId id in ids)
            {
                ctrs[(int) id]++;
            }
            
            for (int i = 0; i < nrOfIds; i++)
            {
                if (ctrs[i] > maxOf(Card.rarityOf((CardId)i))) return false;
            }
            return true;
        }

        //todo fix 99999999 cuz we're just brain damaged ironically
        private static int maxOf(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common: return 4;
                case Rarity.Uncommon: return 3;
                case Rarity.Ebin: return 2;
                case Rarity.Legendair: return 1;
                case Rarity.Token: return 0;
                case Rarity.Xperimental: return 99999999;
            }
            return 99999999;
        }
        
        private void sortAfterColor(Colour colour)
        {
            //seba if youre reading this its too late, code is too far gone
            //please dont judge
            
            switch (colour)
            {
                case Colour.BLACK:
                    if (filters.Contains(isBlack)) filters.Remove(isBlack);
                    else filters.Add(isBlack);
                    break;
                case Colour.BLUE:
                    if (filters.Contains(isBlue)) filters.Remove(isBlue);
                    else filters.Add(isBlue);
                    break;
                case Colour.GREEN:
                    if (filters.Contains(isGreen)) filters.Remove(isGreen);
                    else filters.Add(isGreen);
                    break;
                case Colour.GREY:
                    if (filters.Contains(isGrey)) filters.Remove(isGrey);
                    else filters.Add(isGrey);
                    break;
                case Colour.MULTI:
                    if (filters.Contains(isMulti)) filters.Remove(isMulti);
                    else filters.Add(isMulti);
                    break;
                case Colour.RED:
                    if (filters.Contains(isRed)) filters.Remove(isRed);
                    else filters.Add(isRed);
                    break;
                case Colour.WHITE:
                    if (filters.Contains(isWhite)) filters.Remove(isWhite);
                    else filters.Add(isWhite);
                    break;
            }
            
            sortedIds.Clear(); 
            sortedIds = ids.Where(d => filters.All(filter => filter(d))).ToList();
            currentPage = 0;
            for (int i = 0; i < cardsPerPage; i++)
            {
                if (sortedIds.ElementAtOrDefault(i) == null) cardSlot[i].Visible = false;
                else
                {
                    cardSlot[i].Visible = true;
                    cardSlot[i].notifyObserver(new Card(sortedIds[i].cardId), null);
                }
            }
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
            int X = 0;
            int c = 0;
            var g = GUI.showWindow(deckAsker);
            foreach (string name in deckNames)
            {
                c++;
                var xd = new Button();
                xd.Text = name;
                xd.Location = new Point(X, Y);
                Y += xd.Height;
                if (c%8 == 0)
                {
                    X += 80;
                    Y = 0;
                }
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
                    tb.Text = deckName.Substring(0, deckName.Length - 4);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return null;
            }
            return myDeck;
        }

        private void loadIntoEditor(List<CardId> deck)
        {
            myDeckIsHard.clear();
            for (int i = 0; i < deck.Count; i++)
            {
                myDeckIsHard.add(new Card(deck[i]));
            }
            
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            scrollLeftButton.Location = new Point(250, Size.Height/8);
            scrollRightButton.Location = new Point(Size.Width-250, Size.Height/8);

            //todo jasin: fix 8 to something else, so we can change it easier MAYBE? coach??
            int currentXPosition = Width / 5;
            int currentYPosition = Size.Height/4;
            
            for (int i = 0; i < 8; i++)
            {
                //location
                cardSlot[i].Location = new Point(currentXPosition, currentYPosition);
                if (i == 3)
                {
                    currentXPosition = Width/5;
                    currentYPosition += cardSlot[i].Height;
                }
                else currentXPosition += cardSlot[i].Width;

                //size
                cardSlot[i].setWidth(Width/10);
            }

            backToMainMenuButton.Location = new Point(Width/10, 0);

            tb.Size = new Size(Size.Width / 10, Size.Height / 30);
            tb.Location = new Point(Size.Width / 2, Size.Height / 40);
            cardCount.Size = new Size(Size.Width / 10, Size.Height / 30);
            cardCount.Location = new Point(backToMainMenuButton.Location.X, backToMainMenuButton.Location.Y+backToMainMenuButton.Size.Height);

            saveButton.Location = new Point(Size.Width - saveButton.Image.Width, 0);
            p.Size = new Size(cards[0].Width, Size.Height);
            loadButton.Location = new Point(saveButton.Location.X, saveButton.Location.Y + saveButton.Height);

            for (int i = 0; i < sortButtons.Count(); i++)
            {
                sortButtons[i].Location = new Point(Size.Width / 2 + 50 * i - 60, Size.Height / 20);
            }

            noDeckName.Location = new Point(tb.Location.X, tb.Location.Y - 20);
            noDeckName.Size = tb.Size;

            cardInfo.Size = new Size(cardSlot[0].Width*2, cardSlot[0].Height*2 + 250);
            cardInfo.Location = new Point(Size.Width - cardInfo.Size.Width - 5, Size.Height/5);
        }

        private void drawTheseButtons(CardButton[] cards)
        {
            int x = cards[0].Size.Width;
            for (int i = 0; i < cards.Length; i++)
            {
                x += cards[i].Width;
                if (i % CARDS_PER_ROW == 0) x = cards[0].Size.Width * 2;
                cards[i].setWidth(Size.Width / (cards.Length/2));
                cards[i].Location = new Point(x + (i % CARDS_PER_ROW) * paddingX, cards[i].Size.Height/3 + cards[i].Size.Height * (i / CARDS_PER_ROW));
            }
        }

        private bool addToDeck(CardId id)
        {
            if (myDeckIsHard.cards.Count(card => card.cardId == id) == maxOf(Card.rarityOf(id))) return false;
            myDeckIsHard.add(new Card(id));
            cardCount.Text = "Count: " + myDeckIsHard.count;
            return true;
        }

        private bool removeFromDeck(CardId id)
        {
            for(int i = myDeckIsHard.count-1; i >= 0; i--)
            {
                if (myDeckIsHard[i].cardId == id)
                {
                    myDeckIsHard.remove(myDeckIsHard[i]);
                    cardCount.Text = "Count: " + myDeckIsHard.count;
                    return true;
                }
            }
            return false;
        }
    }
}