﻿using System;
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
        public int[][] check(Card card, GameInterface gi)
        {
            gi.setContext("pay shit coach", Choice.Cancel);
            int[][] r = new int[costs.Count][];
            for (int i = 0; i < costs.Count; i++)
            {
                int[] c = costs[i].check(card, gi);
                if (c == null)
                {
                    r = null;
                    break;
                }
                r[i] = c;
            }
            gi.clearContext();
            return r;
        }

        public GameEvent[] pay(Card card, GameInterface gi, int[][] i)
        {
            List<GameEvent> r = new List<GameEvent>();
            if (i.Length == 0) { return r.ToArray(); }
            if (i.Length != costs.Count) { throw new Exception("can't"); }
            for (int j = 0; j < i.Length; j++)
            {
                r.AddRange(costs[j].pay(card, gi, i[j]));
            }
            return r.ToArray();
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
    
    public abstract class SubCost
    {
        public abstract int[] check(Card c, GameInterface gi);
        abstract public GameEvent[] pay(Card c, GameInterface gi, int[] i);
    }

    public class ExhaustCost : SubCost
    {
        private Card exhaustMe;

        public ExhaustCost(Card exhaustMe)
        {
            this.exhaustMe = exhaustMe;
        }

        public override int[] check(Card c, GameInterface gi)
        {
            if (exhaustMe.canExhaust)
            {
                return new[] {exhaustMe.getId()};
            }
            else
            {
                return null;
            }
        }

        public override GameEvent[] pay(Card c, GameInterface gi, int[] i)
        {
            GameEvent[] r = new GameEvent[i.Length];
            for (int v = 0; v < r.Length; v++)
            {
                r[v] = new ExhaustEvent(gi.getCardById(i[v]));
            }
            return r;
        }
    }
    

    public class MoveThisCost : MoveToCost
    {
        private Card sacMe;

        public MoveThisCost(LocationPile from, LocationPile to, int cardsToMove, Card sacMe) : base(from, to, cardsToMove)
        {
            this.sacMe = sacMe;
        }

        public override int[] check(Card c, GameInterface gi)
        {
            if (sacMe.location.pile == LocationPile.FIELD && sacMe.controller.isHero)
            {
                return new[] {sacMe.getId()};
            }
            else
            {
                return null;
            }
        }
    }

    public class MoveToCost : SubCost
    {
        private LocationPile to;
        private LocationPile from;
        private int cardsToMove;

        public MoveToCost(LocationPile from, LocationPile to, int cardsToMove)
        {
            this.to = to;
            this.from = from;
            this.cardsToMove = cardsToMove;
        }

        public override int[] check(Card card, GameInterface gi)
        {
            int[] r = new int[cardsToMove];
            int i = 0;
            while (i < cardsToMove)
            {
                GameElement e = gi.getNextGameElementPress();
                if (e.card != null && e.card.location.pile == from && e.card.owner.isHero)
                {
                    r[i] = e.card.getId();
                    i++;
                }
                if (e.choice != null && e.choice.Value == Choice.Cancel)
                {
                    return null;
                }
            }
            return r;
        }

        public override GameEvent[] pay(Card c, GameInterface gi, int[] i)
        {
            return i.Select(n => new MoveCardEvent(gi.getCardById(n), to)).ToArray();
        }
    }

    public class ManaCost : SubCost
    {
        //todo(seba) reconsider how we store this information yet again, no it's fine xd
        private readonly int[] Costs = new int[6];
        public int[] costs => cloneLambda(Costs);

        public IEnumerable<int> costsEnumerable => Costs;

        private int[] cloneLambda(int[] a)
        {
            int[] r = new int[a.Length];
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = a[i];
            }
            return r;
        }

        public int CMC { get; private set; }

        public ManaCost(int white, int blue, int black, int red, int green, int grey)
        {
            Costs[(int)Colour.WHITE] = white;
            Costs[(int)Colour.BLUE] = blue;
            Costs[(int)Colour.BLACK] = black;
            Costs[(int)Colour.RED] = red;
            Costs[(int)Colour.GREEN] = green;
            Costs[(int)Colour.GREY] = grey;

            CMC = white + blue + black + red + green + grey;
        }

        public override int[] check(Card card, GameInterface gi)
        {
            Player owner = card.owner;

            if (CMC > owner.totalMana) { return null; }

            int[] r = new int[CMC];
            int[] cz = costs;
            int c = 0;

            for (int i = 0; i < 5; i++)
            {
                if (cz[i] > owner.getCurrentMana(i))
                {
                    return null;
                }
                while (cz[i]-- > 0)
                {
                    r[c++] = i;
                }
            }

            if (c == CMC) { return r; }

            if (CMC == owner.totalMana)
            {
                for (int i = 0; i < 5; i++)
                {
                    int v = owner.getCurrentMana(i) - Costs[i];
                    while (v-- > 0)
                    {
                        r[c++] = i;
                    }
                }
                return r;
            }
            else
            {
                var paid = costs;

                gi.setFakeManas(Costs);
                gi.setContext("", Choice.Cancel);

                while (c != CMC)
                {
                    gi.changeMessage("Pay " + (CMC - c));

                    GameElement element = gi.getNextGameElementPress();

                    if (element.choice != null && element.choice == Choice.Cancel)
                    {
                        gi.resetFakeMana();
                        gi.clearContext();
                        return null;
                    }

                    else if (element.manaColour != null)
                    {
                        int v = (int)element.manaColour;

                        if (owner.getCurrentMana(v) - paid[v] > 0)
                        {
                            r[c++] = v;
                            gi.decrementFakeMana(v);
                        }
                    }
                    
                }

                gi.resetFakeMana();
                gi.clearContext();
                return r;
            }
        }

        public override GameEvent[] pay(Card card, GameInterface gi, int[] i)
        {
            //hack as fuck
            card.owner.spendMana(i);
            return new GameEvent[] {};
        }
    }

    public class PayLifeCost : SubCost
    {
        public readonly int amount;

        public PayLifeCost(int amount)
        {
            this.amount = amount;
        }

        public override int[] check(Card c, GameInterface gi)
        {
            if (c.owner.getLife() > amount)
            {
                return new[] {amount};
            }
            else
            {
                return null;
            }
        }

        public override GameEvent[] pay(Card c, GameInterface gi, int[] i)
        {
            return new[] {new GainLifeEvent(c.owner, -i[0])};
        }
    }
}
