﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace stonekart
{
    class DeckEditorPanel : DisplayPanel
    {
        Button saveButton;
        CardPanel p;
        CardButton[] cards;
        TextBox tb;
        Pile myDeckIsHard;
        const int paddingX = 8;
        const int paddingY = 8;
        const int CARDS_PER_ROW = 6;
        
        public DeckEditorPanel()
        {
            
            int nrOfCards = Enum.GetValues(typeof(CardId)).Length;
            cards = new CardButton[nrOfCards];
            myDeckIsHard = new Pile(new Card[] { });
            p = new CardPanel(new Func<CardButton>(() => new CardButton()), new LayoutArgs(true, true));
            myDeckIsHard.addObserver(p);
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

            saveButton = new Button();
            saveButton.Image = ImageLoader.getStepImage("save", new Size(60,60));
            saveButton.Size = saveButton.Image.Size;
            
            saveButton.MouseDown += (_, __) =>
            {
                saveDeck();
            };

            tb = new TextBox();
            
            Controls.Add(tb);
            Controls.Add(p);
            Controls.Add(saveButton);
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

        public List<CardId> loadDeckFromFile()
        {
            Panel deckAsker = new Panel();
            deckAsker.Size = new Size(500, 200);
            deckAsker.Location = new Point(Size.Width/2, (Size.Height/3)*2);
            GUI.showWindow(deckAsker);
            
            List<CardId> myDeck = new List<CardId>();
            try
            { 
                using (StreamReader sr = new StreamReader(@"deck.txt"))
                {
                    foreach(CardId id in myDeck)
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

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            tb.Size = new Size(Size.Width / 10, Size.Height / 30);
            tb.Location = new Point(Size.Width / 2, Size.Height / 40);
            saveButton.Location = new Point(Size.Width - saveButton.Image.Width, 0);
            p.Size = new Size(cards[0].Width, Size.Height);
            int x = cards[0].Size.Width;
            for (int i = 0; i < cards.Length; i++)
            {
                x += cards[i].Width;
                if (i % CARDS_PER_ROW == 0) x = cards[0].Size.Width*2;
                cards[i].setWidth(Size.Width/cards.Length);
                cards[i].Location = new Point(x + (i%CARDS_PER_ROW)*paddingX, cards[i].Size.Height + cards[i].Size.Height * (i/CARDS_PER_ROW));
            }
        }

        private bool addToDeck(CardId id)
        {
            myDeckIsHard.add(new Card(id));
            return true;
        }

        private bool removeFromDeck(CardId id)
        {
            for(int i = myDeckIsHard.Count-1; i >= 0; i--)
            {
                if (myDeckIsHard.cards[i].cardId == id)
                {
                    myDeckIsHard.remove(myDeckIsHard.cards[i]);
                    return true;
                }
            }
            return false;
        }
    }
}