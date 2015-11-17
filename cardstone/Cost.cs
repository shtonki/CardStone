using System;
using System.Collections.Generic;

namespace stonekart
{
    public class Cost
    {
        private ManaCost manaCost;
        private List<Coster> costs;

        public bool tryPay(Player p)
        {
            if (checkAll(p))
            {
                payAll(p);
                return true;
            }
            return false;
        }

        private bool checkAll(Player p)
        {
            foreach (Coster c in costs)
            {
                if (!c.check(p))
                {
                    return false;
                }
            }
            return true;
        }

        private void payAll(Player p)
        {
            foreach (Coster c in costs)
            {
                c.pay(p);
            }
        }

        public Cost(ManaCost c)
        {
            manaCost = c;
            costs = new List<Coster>();
            costs.Add(manaCost);
        }

        public ManaCost getManaCost()
        {
            return manaCost;
        }
    }

    public abstract class Coster
    {
        public abstract bool check(Player p);
        abstract public void pay(Player p);
    }

    public class ManaCost : Coster
    {
        public const int
            WHITE = 0,
            BLUE = 1,
            BLACK = 2,
            RED = 3,
            GREEN = 4;

        private int[] colors;

        public ManaCost(int white, int blue, int black, int red, int green)
        {
            colors = new int[5];
            colors[WHITE] = white;
            colors[BLUE] = blue;
            colors[BLACK] = black;
            colors[RED] = red;
            colors[GREEN] = green;
        }

        public override bool check(Player p)
        {
            for (int i = 0; i < 5; i++)
            {
                if (p.getCurrentMana(i) < colors[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override void pay(Player p)
        {
            for (int i = 0; i < 5; i++)
            {
                p.spendMana(i, colors[i]);
            }
        }

        public int[] getColors()
        {
            return colors;
        }
    }
}