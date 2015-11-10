﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class Pile : Observable
    {
        private List<Card> cards;

        public int Count { get { return cards.Count; }  }

        public Pile()
        {
            cards = new List<Card>();
        }

        public Pile(CardId[] cs)
        {
            cards = new List<Card>();

            foreach (CardId i in cs)
            {
                new Card(i).moveTo(this);
            }
        }

        public List<Card> getCards()
        {
            return cards;
        }

        public void add(Card c)
        {
            cards.Add(c);
            notifyObserver();
        }

        public void remove(Card c)
        {
            cards.Remove(c);
            notifyObserver();
        }

        public Card peek()
        {
            return cards[cards.Count - 1];
        }

        private static Random rng = new Random();

        public void shuffle()
        {
            int n = cards.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
        }

        public Location getLocation()
        {
            return Location.getLocation(this);
        }

    }
}
