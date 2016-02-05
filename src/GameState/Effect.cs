using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace stonekart
{
    public class Effect
    {
        private SubEffect[] subEffects;
        private TargetRule[] targetRules;

        private string Explanation;

        public Effect(params SubEffect[] es)
        {
            subEffects = es;

            targetRules = new TargetRule[targetCount];
            int i = 0;
            foreach (var v in es)
            {
                foreach (var x in v.targetRules)
                {
                    targetRules[i++] = x;
                }
            }
        }

        public List<GameEvent> resolve(Card c, Target[] ts, GameInterface ginterface, GameState gameState)
        {
            List<GameEvent> r = new List<GameEvent>();
            IEnumerator<Target> targetEnumerator = ((IEnumerable<Target>)ts).GetEnumerator();
            targetEnumerator.MoveNext();

            foreach (SubEffect e in subEffects)
            {
                foreach (var v in e.resolve(c, targetEnumerator, ginterface, gameState))
                {
                    r.Add(v);
                }
            }
            
            return r;
        }

        public TargetRule[] getTargetRules()
        {
            return targetRules;
        }

        public int targetCount => subEffects.Sum(v => v.targetCount);
        
        public string explanation => Explanation;
    }

    public abstract class SubEffect
    {
        public TargetRule[] targetRules => targets;
        public int targetCount => targets.Length;
        protected TargetRule[] targets;

        protected SubEffect()
        {
            targets = new TargetRule[0];
        }

        abstract public GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game);
        
    }
    

    public class Timelapse : SubEffect
    {
        private int n;

        public Timelapse(int n)
        {
            this.n = n;
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            Choice shuffle;
            if (c.owner.side == LocationPlayer.HERO)
            {
                CardPanelControl p = ginterface.showCards(c.owner.deck.cards.Take(n).ToArray());
                shuffle = ginterface.getChoice("Shuffle deck?", Choice.Yes, Choice.No);
                ginterface.sendSelection((int)shuffle);
                p.closeWindow();
            }
            else
            {
                shuffle = (Choice)ginterface.demandSelection();
            }
            
            if (shuffle == Choice.Yes)
            {
                throw new NotImplementedException();
                //g.shuffleDeck(c.owner);
            }
            ginterface.showCards(c.owner.deck.cards.Take(2).ToArray());
            return new GameEvent[]{};
        }
    }

    public class OwnerDraws : SubEffect
    {
        private int i;

        public OwnerDraws(int cards)
        {
            i = cards;
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> _, GameInterface ginterface, GameState game)
        {
            GameEvent e = new DrawEvent(c.controller, i);
            return new[] {e};
        }
        
    }

    public class PingN : SubEffect
    {
        private int d;

        public PingN(int targets, int damage)
        {
            this.targets = new TargetRule[targets];

            for (int i = 0; i < targets; i++)
            {
                this.targets[i] = new TargetRule(TargetRules.ZAPPABLE); 
            }
            d = damage;
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {

            GameEvent[] r = new GameEvent[targets.Length];

            for (int i = 0; i < r.Length; i++)
            {
                Target t = ts.Current;
                ts.MoveNext();

                if (t.isPlayer())
                {
                    r[i] = new DamagePlayerEvent(t.getPlayer(), c, d);
                }
                else
                {
                    r[i] = new DamageCreatureEvent(t.getCard(), c, d);
                }
            }

            return r;
        }
        
    }

    public class ExileTarget : SubEffect
    {
        public ExileTarget()
        {
            targets = new TargetRule[1];
            targets[0] = new TargetRule(TargetRules.CREATUREONFIELD);
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            GameEvent[] r = new GameEvent[1];
            Target v = ts.Current;
            ts.MoveNext();
            r[0] = new MoveCardEvent(v.getCard(), LocationPile.EXILE);
            return r;
        }
    }

    public class GainLife : SubEffect
    {
        private int life;

        public GainLife(int n)
        {
            life = n;
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            GameEvent[] l = {new GainLifeEvent(c.controller, life)};
            return l;
        }
        
    }

    public class SummonNTokens : SubEffect
    {
        public int count { get; private set; }
        public CardId card { get; private set; }
        public SummonNTokens(int n, CardId c)
        {
            count = n;
            card = c;
            
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            GameEvent[] r = new GameEvent[count];

            for (int i = 0; i < count; i++)
            {
                r[i] = new SummonTokenEvent(c.controller, card);
            }

            return r;
        }
    }

    public class SubEffectModifyUntil : SubEffect
    {
        public readonly Card card;
        public readonly Modifiable attribute;
        public readonly Clojurex filter;
        public readonly int value;

        public SubEffectModifyUntil(Card card, Modifiable attribute, Clojurex filter, int value)
        {
            this.card = card;
            this.attribute = attribute;
            this.filter = filter;
            this.value = value;
        }

        public SubEffectModifyUntil(TargetRule t, Modifiable attribute, Clojurex filter, int value)
        {
            targets = new []{t};
            this.attribute = attribute;
            this.filter = filter;
            this.value = value;
        }

        

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            Card t;
            if (card == null)
            {
                var r = ts.Current;
                ts.MoveNext();
                t = r.getCard();
            }
            else
            {
                t = card;
            }
            return new GameEvent[]
            {
                new ModifyCardEvent(t, attribute, filter, value)
            };
        }
    }

    public class SubEffectPlayerDiscard : SubEffect
    {
        private int cards;
        private bool castersChoice;

        public SubEffectPlayerDiscard(int cards, bool castersChoice)
        {
            this.cards = cards;
            this.castersChoice = castersChoice;
            targets = new TargetRule[] {new TargetRule(TargetRules.PLAYER), };
        }

        public override GameEvent[] resolve(Card c, IEnumerator<Target> ts, GameInterface ginterface, GameState game)
        {
            if (cards != 1)
            {
                throw new NotImplementedException();
            }

            Card[] r;
            Player victim = ts.Current.getPlayer();
            ts.MoveNext();
            if (castersChoice == c.ownedByMe)
            {
                r = new Card[cards];
                CardPanelControl p = ginterface.showCards(victim.hand.cards.ToArray());
                ginterface.setContext("Pick a card");
                for (int i = 0; i < cards; i++)
                {
                    r[i] = p.waitForCard();
                }
                ginterface.clearContext();
                p.closeWindow();
                ginterface.sendMultiSelection(r);
            }
            else
            {
                r = ginterface.demandMultiSelection().Select(game.getCardById).ToArray();
            }
            return r.Select(crd => new MoveCardEvent(crd, LocationPile.GRAVEYARD)).ToArray();

        }
    }

    
}
