using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace stonekart
{
    public abstract class GameEvent
    {
        public const int
            DRAW = 1,
            CAST = 2,
            PASS = 3,
            GAINMANA = 4,
            DECLAREATTACKERS = 5,
            RESOLVECARD = 6;

        private int type;

        public GameEvent(int type)
        {
            this.type = type;
        }

        public int getType()
        {
            return type;
        }


        public string toNetworkString()
        {
            return "event:" + type + ':' + getCruftString();
        }

        protected abstract string getCruftString();

    }

    class DrawEvent : GameEvent
    {
        private bool homePlayer;

        public DrawEvent(bool homePlayer) : base(DRAW)
        {
            this.homePlayer = homePlayer;
        }

        protected override string getCruftString()
        {
            return homePlayer ? "home" : "away";
        }
    }

    class CastEvent : GameEvent
    {
        private Card card;

        public CastEvent(Card c) : base(CAST)
        {
            card = c;
        }

        public Card getCard()
        {
            return card;
        }

        protected override string getCruftString()
        {
            return card.getId().ToString();
        }
    }

    class PassEvent : GameEvent
    {
        public PassEvent() : base(PASS)
        {
        }

        protected override string getCruftString()
        {
            return "";
        }
    }

    class GainManaOrbEvent : GameEvent
    {
        private int color;

        public GainManaOrbEvent(int color) : base(GAINMANA)
        {
            this.color = color;
        }

        protected override string getCruftString()
        {
            return color.ToString();
        }
    }

    class DeclareAttackersEvent : GameEvent
    {
        private Card[] attackers;

        public DeclareAttackersEvent(Card[] c) : base(DECLAREATTACKERS)
        {
            attackers = c;
        }

        protected override string getCruftString()
        {
            if (attackers.Length == 0) { return ""; }

            StringBuilder s = new StringBuilder();
            int i = 0;

            for (; i < attackers.Length - 1; i++)
            {
                s.Append(attackers[i].getId() + ",");
            }
            s.Append(attackers[i].getId());

            return s.ToString();
        }
    }

    class ResolveCardEvent : GameEvent
    {
        private Card card;

        public ResolveCardEvent(Card c) : base(RESOLVECARD)
        {
            card = c;
        }

        protected override string getCruftString()
        {
            return card.getId().ToString();
        }
    }
}
