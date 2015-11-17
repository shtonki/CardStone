using System;
using System.Collections.Generic;

namespace stonekart
{
    public class Cost
    {
        private ManaCost manaCost;
        private List<Coster> costs;

        public bool tryPay()
        {
            if (checkAll())
            {
                payAll();
                return true;
            }
            return false;
        }

        private bool checkAll()
        {
            foreach (Coster c in costs)
            {
                if (!c.check())
                {
                    return false;
                }
            }
            return true;
        }

        private void payAll()
        {
            foreach (Coster c in costs)
            {
                c.pay();
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
        public abstract bool check();
        abstract public void pay();
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

        public override bool check()
        {
            for (int i = 0; i < 5; i++)
            {
                if (GameController.currentGame.getHero().getCurrentMana(i) < colors[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override void pay()
        {
            for (int i = 0; i < 5; i++)
            {
                GameController.currentGame.getHero().spendMana(i, colors[i]);
            }
        }

        public int[] getColors()
        {
            return colors;
        }
    }
}
