using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class Player : Observable
    {
        private int[] curMana, maxMana;
        private int health;

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

            health = 20;
        }


        public void addMana(int i)
        {
            curMana[i]++;
            maxMana[i]++;
            notifyObserver();
        }

        public bool draw(int c = 1)
        {
            for (int i = 0; i < c; i++)
            {
                if (deck.Count == 0) { return false; }
                deck.peek().moveTo(hand);
            }

            notifyObserver();
            return true;
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

        public int getHealth()
        {
            return health;
        }


        public void damage(int i)
        {
            health -= i;
            notifyObserver();
        }

        public void spendMana(int color, int amount)
        {
            curMana[color] -= amount;
            notifyObserver();
        }

        public void spendMana(int[] i)
        {
            foreach (var v in i)
            {
                curMana[v]--;
            }

            notifyObserver();
        }


        public void resetMana()
        {
            for (int i = 0; i < 5; i++)
            {
                curMana[i] = maxMana[i];
            }
        }

        public void untop()
        {
            resetMana();

            foreach (Card c in field.getCards())
            {
                c.unTop();
            }
        }


        public void loadDeck(List<Card> deckList, Location l)
        {
            foreach (Card c in deckList)
            {
                c.setOwner(this);
                deck.add(c);
                c.setLocationRaw(l);
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
