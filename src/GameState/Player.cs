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
        //todo(seba) move all these to props
        private int[] curMana, maxMana;
        public int totalMana => curMana.Sum(@v => v);
        private int health;

        //private Pile hand, graveyard, exile, field, deck;
        
        private Pile[] piles;

        private LocationPlayer side;

        public Game game { get; private set; }

        public Player(Game g, LocationPlayer l)
        {
            game = g;
            side = l;

            hand = new Pile(new Location(LocationPile.HAND, l));
            graveyard = new Pile(new Location(LocationPile.GRAVEYARD, l));
            exile = new Pile(new Location(LocationPile.EXILE, l));
            field = new Pile(new Location(LocationPile.FIELD, l));
            deck = new Pile(new Location(LocationPile.DECK, l));

            piles = new Pile[5];
            piles[(int)LocationPile.DECK] = deck;
            piles[(int)LocationPile.EXILE] = exile;
            piles[(int)LocationPile.FIELD] = field;
            piles[(int)LocationPile.GRAVEYARD] = graveyard;
            piles[(int)LocationPile.HAND] = hand;

            curMana = new int[5];
            maxMana = new int[5];

            health = 20;
        }


        public void addMana(int i)
        {
            curMana[i]++;
            maxMana[i]++;
            notifyObservers();
        }

        public bool draw(int c = 1)
        {
            for (int i = 0; i < c; i++)
            {
                if (deck.Count == 0) { return false; }
                game.moveCardTo(deck.peek(), hand); //deck.peek().moveTo(hand);
            }

            notifyObservers();
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


        public void setLifeRelative(int i)
        {
            health += i;
            notifyObservers();
        }

        public void spendMana(int color, int amount)
        {
            curMana[color] -= amount;
            notifyObservers();
        }

        public void spendMana(int[] i)
        {
            foreach (var v in i)
            {
                curMana[v]--;
            }

            notifyObservers();
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

            foreach (Card c in field.cards)
            {
                c.topped = false;
                c.summoningSick = false;
            }
        }


        public LocationPlayer getSide()
        {
            return side;
        }

        public void loadDeck(List<Card> deckList)
        {
            Location l = new Location(LocationPile.DECK, side);
            foreach (Card c in deckList)
            {
                c.owner = this;
                c.controller = this;
                deck.add(c);
                c.setLocationRaw(l);
            }
        }

        public Pile hand { get; }

        public Pile graveyard { get; }

        public Pile exile { get; }

        public Pile field { get; }

        public Pile deck { get; }

        public Pile getPile(LocationPile p)
        {
            return piles[(int)p];
        }
    }
}