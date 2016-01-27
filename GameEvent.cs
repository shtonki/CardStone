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
        GAINMANAORB,
        ATTACK,
        DEFEND,
        TOPCARD,
        UNTOP,
        UNTOPPLAYER,
        UNTOPCARD,
        MOVECARD,
        STEP,
        RESOLVE,
        DAMAGEPLAYER,
        DAMAGECREATURE,
        BURYCREATURE,
        GAINLIFE,
    }

    /// <summary>
    /// Represents an action to be taken within the game
    /// </summary>
    public abstract class GameEvent
    {
        public GameEventType type { get; private set; }

        public GameEvent(GameEventType type)
        {
            this.type = type;
        }
        
    }

    class DrawEvent : PlayerEvent
    {
        private int cs;

        public DrawEvent(Player p) : base(p, GameEventType.DRAW)
        {
            cs = 1;
        }

        public DrawEvent(Player p, int cards) : base(p, GameEventType.DRAW)
        {
            cs = cards;
        }

        public new Player getPlayer()
        {
            return p;
        }

        public int getCards()
        {
            return cs;
        }
    }

    class CastEvent : GameEvent
    {
        private StackWrapper xd;

        public CastEvent(Card c, Ability ability, params Target[] cs) : base(GameEventType.CAST)
        {
            xd = new StackWrapper(c, ability , cs);
        }

        public CastEvent(StackWrapper x) : base(GameEventType.CAST)
        {
            xd = x;
        }

        public StackWrapper getStackWrapper()
        {
            return xd;
        }
    }

    class GainManaOrbEvent : PlayerEvent
    {
        private int c;

        public GainManaOrbEvent(Player player, int color) : base(player, GameEventType.GAINMANAORB)
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

    class TopEvent : CardEvent
    {
        public TopEvent(Card c) : base(c, GameEventType.TOPCARD)
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
        public Card card { get; private set; }
        public Location to { get; private set; }
        public Location from { get; private set; }

        public MoveCardEvent(Card card, Location loc) : base(GameEventType.MOVECARD)
        {
            this.card = card;
            to = loc;
            from = card.location;
        }

        public MoveCardEvent(Card card, LocationPile pile) : this(card, new Location(pile, card.owner.getSide()))
        {
            
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

    class ResolveEvent : GameEvent
    {
        private StackWrapper xdd;

        public ResolveEvent(StackWrapper xd) : base(GameEventType.RESOLVE)
        {
            xdd = xd;
        }

        public StackWrapper getStackWrapper()
        {
            return xdd;
        }
    }

    abstract class DamageFooEvent : GameEvent
    {
        private Card s;
        private int d;


        public Card getSource()
        {
            return s;
        }

        public int getDamage()
        {
            return d;
        }

        protected DamageFooEvent(Card source, int damage, GameEventType type) : base(type)
        {
            s = source;
            d = damage;
        }
    }

    class DamagePlayerEvent : DamageFooEvent
    {
        private Player p;

        public DamagePlayerEvent(Player player, Card source, int damage) : base(source, damage, GameEventType.DAMAGEPLAYER)
        {
            p = player;
        }

        public Player getPlayer()
        {
            return p;
        }
    }

    class GainLifeEvent
    {
        
    }

    class DamageCreatureEvent : DamageFooEvent
    {
        private Card c;

        public DamageCreatureEvent(Card creature, Card source, int damage) : base(source, damage, GameEventType.DAMAGECREATURE)
        {
            c = creature;
        }

        public Card getCreature()
        {
            return c;
        }
    }

    class BuryCreature : CardEvent
    {
        public BuryCreature(Card card) : base(card, GameEventType.BURYCREATURE)
        {
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

    public delegate bool EventFilter(GameEvent e);
    public delegate void EventAction(GameEvent e);
    public enum EventTiming { Pre, Main, Post };


    public class EventHandler
    {

        public readonly EventFilter filter;
        public readonly EventAction action;
        public readonly EventTiming timing;


        public EventHandler(GameEventType t, EventAction e)
        {
            //todo(seba) don't think these are interned the way you'd hope
            filter = @v => v.type == t;
            action = e;
            timing = EventTiming.Main;
        }

        public void handle(GameEvent e, EventTiming t)
        {
            if (t == timing && filter(e))
            {
                action(e);
            }
        }
        
    }
}
