using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace stonekart
{
    public class Cost
    {
        //private ManaCoster manaCost;
        private List<SubCost> costs;
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

        public Cost(params SubCost[] cs)
        {
            costs = new List<SubCost>(cs);
        }

        public Cost(params SubCost[][] cs)
        {
            int c = 0;
            //foreach (Coster[] l in cs) { c += l.Length; }
            costs = new List<SubCost>(c);
            foreach (SubCost[] xs in cs)
            {
                foreach (SubCost t in xs)
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
    public abstract class SubCost
    {
        public abstract int[] check(Card c);
        abstract public void pay(Card c, int[] i);
    }

    public class ManaCost : SubCost
    {
        //todo(seba) reconsider how we store this information yet again
        public readonly int[] costs = new int[6];
        public int CMC { get; private set; }

        public ManaCost(int white, int blue, int black, int red, int green, int grey)
        {
            costs[(int)ManaColour.WHITE] = white;
            costs[(int)ManaColour.BLUE] = blue;
            costs[(int)ManaColour.BLACK] = black;
            costs[(int)ManaColour.RED] = red;
            costs[(int)ManaColour.GREEN] = green;
            costs[(int)ManaColour.GREY] = grey;

            CMC = white + blue + black + red + green + grey;
        }

        public override int[] check(Card card)
        {
            int[] r = new int[CMC];
            int c = 0;

            for (int i = 0; i < 5; i++)
            {
                int t = costs[i];
                while (t-- > 0)
                {
                    r[c++] = i;
                }
            }

            return r;
        }

        public override void pay(Card card, int[] i)
        {
            card.owner.spendMana(i);
        }
    }
}
