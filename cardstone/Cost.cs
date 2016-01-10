using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace stonekart
{
    public class Cost
    {
        //private ManaCoster manaCost;
        private List<Coster> costs;
        /*
        public bool tryPay(Player p)
        {
            if (checkAll(p))
            {
                payAll(p);
                return true;
            }
            return false;
        }
        */
        public int[][] check(Card card)
        {
            int[][] r = new int[costs.Count][];
            for (int i = 0; i < costs.Count; i++)
            {
                int[] c = costs[i].check(card);
                if (c == null) { return null; }
                r[i] = c;
            }
            return r;
        }

        public void pay(Card card, int[][] i)
        {
            if (i.Length != costs.Count) { throw new Exception("can't"); }
            for (int j = 0; j < i.Length; j++)
            {
                costs[j].pay(card, i[j]);
            }
        }

        public Cost(params Coster[] cs)
        {
            costs = new List<Coster>(cs);
        }

        public Cost(params Coster[][] cs)
        {
            int c = 0;
            //foreach (Coster[] l in cs) { c += l.Length; }
            costs = new List<Coster>(c);
            foreach (Coster[] xs in cs)
            {
                foreach (Coster t in xs)
                {
                    costs.Add(t);
                }
            }
        }
    }
    /*
    public class CastingCost : Cost
    {
        private ManaCoster manaCost;

        public CastingCost(ManaCoster c) : base(c)
        {
            manaCost = c;
        }

        public CastingCost(ManaCoster c, params Coster[] cs) : base(new Coster[]{c}, cs)
        {
            manaCost = c;
        }

        public ManaCoster getManaCost()
        {
            return manaCost;
        }
    }
    */
    public abstract class Coster
    {
        public abstract int[] check(Card c);
        abstract public void pay(Card c, int[] i);
    }

    public class ManaCoster : Coster
    {
        public const int
            WHITE = 0,
            BLUE = 1,
            BLACK = 2,
            RED = 3,
            GREEN = 4;

        private List<int> cost;

        public ManaCoster(int white, int blue, int black, int red, int green)
        {
            cost = new List<int>();
            int[] _ = new[] {white, blue, black, red, green};
            for (int c = 0; c < 5; c++)
            {
                for (int i = 0; i < _[c]; c++)
                {
                    cost.Add(c);
                }
            }
        }

        public override int[] check(Card card)
        {
            Player p = card.owner;

            int[] cs = new int[5];

            foreach (var b in cost)
            {
                cs[b]++;
            }

            for (int i = 0; i < 5; i++)
            {
                if (p.getCurrentMana(i) < cs[i]) { return null; }
            }
            
            return cost.ToArray();
        }

        public override void pay(Card card, int[] i)
        {
            card.owner.spendMana(i);
        }

        public int[] getColours()
        {
            return cost.ToArray();
        }
    }
}
