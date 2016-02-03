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
        CardButton[] cards;
        List<CardId> myDeckIsHard;
        const int paddingX = 8;
        const int paddingY = 8;
        const int CARDS_PER_ROW = 7;
        
        public DeckEditorPanel()
        {
            int nrOfCards = Enum.GetValues(typeof(CardId)).Length;
            cards = new CardButton[nrOfCards];
            myDeckIsHard = new List<CardId>();

            for (int i = 0; i < nrOfCards; i++)
            {
                int i0 = i;
                cards[i] = new CardButton((CardId)i);
                Controls.Add(cards[i]);
                cards[i].MouseDown += (_, __) =>
                {
                    if (__.Button == MouseButtons.Left) addToDeck(cards[i0].Card.cardId);
                    else if (__.Button == MouseButtons.Right) removeFromDeck(cards[i0].Card.cardId);
                };
            }

            Button saveButton = new Button();
            //saveButton.Image = ImageLoader.
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            int x = cards[0].Size.Width;
            for (int i = 0; i < cards.Length; i++)
            {
                x += cards[i].Width;
                if (i % CARDS_PER_ROW == 0) x = cards[0].Size.Width;
                cards[i].setWidth(Size.Width/cards.Length);
                cards[i].Location = new Point(x + (i%CARDS_PER_ROW)*paddingX, cards[i].Size.Height + cards[i].Size.Height * (i/CARDS_PER_ROW));
            }   
        }

        private bool addToDeck(CardId id)
        {
            myDeckIsHard.Add(id);
            return true;
        }

        private bool removeFromDeck(CardId id)
        {
            return myDeckIsHard.Remove(id);
        }
    }
}