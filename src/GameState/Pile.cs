using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    /// <summary>
    /// A class representing an ordered set of cards
    /// </summary>
    public class Pile : Observable//, IList<int>
    {
        public int Count { get { return cards.Count; }  }

        public Location location { get; private set; }
        
        public List<Card> cards { get; private set; }

        public Pile(Location l)
        {
            location = l;
            cards = new List<Card>();
        }

        public Pile(Card[] cs)
        {
            cards = new List<Card>(cs);
        }


        public void add(Card c)
        {
            cards.Add(c);
            notifyObservers(new object[]{c, true});
        }

        public void remove(Card c)
        {
            cards.Remove(c);
            notifyObservers(new object[] { c, false });
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
            notifyObservers();
        }
        

    }
}
