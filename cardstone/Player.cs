﻿using System;
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
            draw(1);
        }

        public void draw(int c)
        {
            for (int i = 0; i < c; i++)
            {
                deck.peek().moveTo(hand);
            }

            notifyObserver();
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

        public void spendMana(int color, int amount)
        {
            curMana[color] -= amount;
            notifyObserver();
        }

        public void resetMana()
        {
            for (int i = 0; i < 5; i++)
            {
                curMana[i] = maxMana[i];
            }
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
