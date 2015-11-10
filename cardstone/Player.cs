using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class Player : Observable
    {
        private int[] curMana, maxMana;

        private Pile hand, graveyard, exile, field, deck;

        public Player()
        {
            hand = new Pile();
            graveyard = new Pile();
            exile = new Pile();
            field = new Pile();
            deck = new Pile();

            curMana = new int[5];
            maxMana = new int[5];
        }

        public void addMana(int i)
        {
            curMana[i]++;
            maxMana[i]++;
            notifyObserver();
        }

        public void draw()
        {
            deck.peek().moveTo(hand);
        }

        public void shuffleDeck()
        {
            deck.shuffle();
        }

        public int getCurrentMana(int color)
        {
            return curMana[color];
        }

        public int getMaxMana(int color)
        {
            return maxMana[color];
        }

        public void loadDeck(CardId[] deckList, Location l)
        {
            foreach (CardId cid in deckList)
            {
                deck.add(new Card(cid, l));
            }
        }

        public Pile getHand()
        {
            return hand;
        }

        public Pile getGraveyard()
        {
            return graveyard;
        }

        public Pile getExile()
        {
            return exile;
        }

        public Pile getField()
        {
            return field;
        }

        public Pile getDeck()
        {
            return deck;
        }
    }
}
