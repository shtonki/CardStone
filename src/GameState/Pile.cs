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
        public Card this[int key]
        {
            get { return Cards[key]; }
            set { Cards[key] = value; }
        }

        public int count { get { return Cards.Count; }  }

        public Location location { get; private set; }

        public IEnumerable<Card> cards => Cards;

        private List<Card> Cards;

        public Pile(Location l)
        {
            location = l;
            Cards = new List<Card>();
        }

        public Pile(Card[] cs)
        {
            Cards = new List<Card>(cs);
        }

        public void clear()
        {
            Cards.Clear();
            notifyObservers();
        }

        public void add(Card c)
        {
            Cards.Add(c);
            notifyObservers(new object[]{c, true});
        }

        public void remove(Card c)
        {
            Cards.Remove(c);
            notifyObservers(new object[] { c, false });
        }

        public Card peek()
        {
            return Cards[count - 1];
        }

        public void shuffle(Random rng)
        {
            int n = count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
            notifyObservers();
        }
    }
}
