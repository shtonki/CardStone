using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace stonekart
{
    public enum GameEventType
    {
        DRAW,
        CAST,
        GAINMANA,
        ATTACK,
        DEFEND,
        TOP,
        UNTOP,
        UNTOPPLAYER,
        MOVECARD,
        STEP,
        RESOLVE,
        DAMAGEPLAYER,
    }

    public abstract class GameEvent
    {

        /*
        public const int
            DRAW = 1,
            CAST = 2,
            UNTOP = 3,
            GAINMANA = 4,
            DECLAREATTACKERS = 5,
            TOP = 6,
            RESETMANA = 7,

            xd = -1;
        */
        private GameEventType type;

        public GameEvent(GameEventType type)
        {
            this.type = type;
        }

        public GameEventType getType()
        {
            return type;
        }
    }

    class DrawEvent : PlayerEvent
    {
        public DrawEvent(Player p) : base(p, GameEventType.DRAW)
        {
        }

        public new Player getPlayer()
        {
            return p;
        }
    }

    class CastEvent : CardEvent
    {
        public CastEvent(Card c) : base(c, GameEventType.CAST)
        {
        }
    }

    class GainManaOrbEvent : PlayerEvent
    {
        private int c;

        public GainManaOrbEvent(Player player, int color) : base(player, GameEventType.GAINMANA)
        {
            c = color;
        }

        public int getColor()
        {
            return c;
        }
    }

    class AttackingEvent : MultiCardEvent
    {
        public AttackingEvent(List<Card> c) : base(c, GameEventType.ATTACK)
        {
        }
    }

    class UntopEvent : MultiCardEvent
    {
        public UntopEvent(List<Card> cs) : base(cs, GameEventType.UNTOP)
        {
        }
    }

    class TopEvent : MultiCardEvent
    {
        public TopEvent(Card c) : base(c, GameEventType.TOP)
        {
        }
    }

    class UntopPlayerEvent : PlayerEvent
    {
        public UntopPlayerEvent(Player player) : base(player, GameEventType.UNTOPPLAYER)
        {
        }
    }

    class MoveCardEvent : GameEvent
    {
        private Card c;
        private Location l;

        public MoveCardEvent(Card card, Location loc) : base(GameEventType.MOVECARD)
        {
            c = card;
            l = loc;
        }

        public Card getCard()
        {
            return c;
        }

        public Location getLocation()
        {
            return l;
        }
    }

    class StepEvent : GameEvent
    {
        private int s;

        public const int
            UNTOP = 0,
            DRAW = 1,
            MAIN1 = 2,
            BEGINCOMBAT = 3,
            ATTACKERS = 4,
            DEFENDERS = 5,
            DAMAGE = 6,
            ENDCOMBAT = 7,
            MAIN2 = 8,
            END = 9;


        public StepEvent(int step) : base(GameEventType.STEP)
        {
            s = step;
        }

        public int getStep()
        {
            return s;
        }
    }

    class ResolveEvent : CardEvent
    {
        public ResolveEvent(Card card) : base(card, GameEventType.RESOLVE)
        {
        }
    }

    class DamagePlayerEvent : GameEvent
    {
        private Player p;
        private Card s;
        private int d;

        public DamagePlayerEvent(Player player, Card source, int damage) : base(GameEventType.DAMAGEPLAYER)
        {
            p = player;
            s = source;
            d = damage;
        }

        public Player getPlayer()
        {
            return p;
        }

        public Card getSource()
        {
            return s;
        }

        public int getDamage()
        {
            return d;
        }
    }


    abstract class PlayerEvent : GameEvent
    {
        protected Player p;

        public PlayerEvent(Player player, GameEventType type) : base(type)
        {
            p = player;
        }

        public Player getPlayer()
        {
            return p;
        }
    }

    abstract class CardEvent : GameEvent
    {
        protected Card c;

        public CardEvent(Card card, GameEventType type) : base(type)
        {
            c = card;
        }

        public Card getCard()
        {
            return c;
        }
    }

    abstract class MultiCardEvent : GameEvent
    {
        protected List<Card> cs;

        public MultiCardEvent(List<Card> cards, GameEventType t) : base(t)
        {
            cs = cards;
        }

        public MultiCardEvent(Card c, GameEventType t) : base(t)
        {
            cs = new List<Card>(1);
            cs.Add(c);
        }

        public List<Card> getCards()
        {
            return cs;
        }

    }

    public class EventHandler
    {
        public delegate void eventHandler(GameEvent e);

        public GameEventType type;
        private eventHandler main, pre, post;

        public EventHandler(GameEventType t, eventHandler e)
        {
            type = t;
            main = e;
        }

        public void invoke(GameEvent e)
        {
            if (type == e.getType())
            {
                main(e);
            }
        }
    }
}
