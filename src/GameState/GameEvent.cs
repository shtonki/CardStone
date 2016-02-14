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
        GAINLIFE,
        SUMMONTOKEN,
        MODIFYCARD,
        SHUFFLEDECK,
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
        public int cardCount { get; private set; }

        public DrawEvent(Player plr) : base(plr, GameEventType.DRAW)
        {
            cardCount = 1;
        }

        public DrawEvent(Player plr, int cards) : base(plr, GameEventType.DRAW)
        {
            cardCount = cards;
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

        public GainManaOrbEvent(Player plr, int color) : base(plr, GameEventType.GAINMANAORB)
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
        public UntopPlayerEvent(Player plr) : base(plr, GameEventType.UNTOPPLAYER)
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

        public MoveCardEvent(Card card, LocationPile pile) : this(card, new Location(pile, card.owner.side))
        {
            
        }

        

    }

    class StepEvent : GameEvent
    {
        public Step step { get; private set; }
        public Player activePlayer { get; private set; }

        public StepEvent(Step step, Player activePlayer) : base(GameEventType.STEP)
        {
            this.step = step;
            this.activePlayer = activePlayer;
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

    class SummonTokenEvent : GameEvent
    {
        public CardId id {get; private set; }
        public Player player { get; private set; }
        public SummonTokenEvent(Player p, CardId id) : base(GameEventType.SUMMONTOKEN)
        {
            this.id = id;
            player = p;
        }
    }

    class DamagePlayerEvent : PlayerEvent
    {
        public int damage { get; private set; }
        public Card source { get; private set; }

        public DamagePlayerEvent(Player plr, Card src, int dmg) : base(plr, GameEventType.DAMAGEPLAYER)
        {
            source = src;
            damage = dmg;
        }
        
    }

    class ShuffleDeckEvent : PlayerEvent
    {
        public ShuffleDeckEvent(Player plr) : base(plr, GameEventType.SHUFFLEDECK)
        {
        }
    }
    
    class GainLifeEvent : PlayerEvent
    {
        public int life { get; private set; }

        public GainLifeEvent(Player p, int l) : base(p, GameEventType.GAINLIFE)
        {
            life = l;
        }
    }

    class DamageCreatureEvent : GameEvent
    {
        public Card creature { get; private set; }
        public Card source { get; private set; }
        public int damage { get; private set; }

        public DamageCreatureEvent(Card crt, Card src, int dmg) : base(GameEventType.DAMAGECREATURE)
        {
            creature = crt;
            source = src;
            damage = dmg;
        }
        
    }
    
    class ModifyCardEvent : GameEvent
    {
        public readonly Card card;
        public readonly Modifiable modifiable;
        public readonly int value;
        public readonly Clojurex clojure;

        public ModifyCardEvent(Card card, Modifiable m, Clojurex c, int value) : base(GameEventType.MODIFYCARD)
        {
            this.card = card;
            modifiable = m;
            this.value = value;
            clojure = c;
        }
    }

    abstract class PlayerEvent : GameEvent
    {
        public Player player { get; private set; }

        public PlayerEvent(Player plr, GameEventType type) : base(type)
        {
            player = plr;
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
