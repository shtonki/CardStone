using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stonekart
{
    public class GameState
    {
        public IEnumerable<Card> allCards => cardFactory.allCards;
        public IEnumerable<Card> herosCards => cardFactory.heroCards;
        public IEnumerable<Card> villainsCards => cardFactory.villainCards;
        public IEnumerable<Card> fieldCards => hero.field.cards.Concat(villain.field.cards);
        public Player hero { get; private set; }
        public Player villain { get; private set; }
        public Player activePlayer => herosTurn ? hero : villain;
        public Player inactivePlayer => !herosTurn ? hero : villain;
        public Pile stack { get; private set; }
        public Step currentStep { get; private set; }
        public bool herosTurn { get; private set; }
        public bool heroCanSorc => stack.count == 0 && herosTurn && (currentStep == Step.MAIN1 || currentStep == Step.MAIN2);

        private CardFactory cardFactory;
        
        
        public GameState()
        {
            cardFactory = new CardFactory();
            hero = new Player(this, LocationPlayer.HERO);
            villain = new Player(this, LocationPlayer.VILLAIN);
            stack = new Pile(new Location(LocationPile.STACK, LocationPlayer.NOONE));
        }


        public void loadDeck(Player p, CardId[] cs)
        {
            List<Card> myDeck = cardFactory.makeList(hero, cs);
            p.loadDeck(myDeck);
        }

        public void setHeroStarting(bool b)
        {
            herosTurn = b;
        }

        public Card makeCard(Player p, CardId id)
        {
            return cardFactory.makeCard(p, id);
        }

        public Card getCardById(int i)
        {
            return cardFactory.getCardById(i);
        }

        public void clearBonusMana()
        {
            hero.clearBonusMana();
            villain.clearBonusMana();
        }

        public void advanceStep()
        {
            currentStep = (Step)(((int)currentStep + 1) % (Enum.GetNames(typeof(Step))).Count());
            herosTurn = currentStep == 0 ? !herosTurn : herosTurn;
        }

        private class CardFactory
        {
            public IEnumerable<Card> heroCards => hero;
            public IEnumerable<Card> villainCards => villain;
            public IEnumerable<Card> allCards => hero.Concat(villain);

            private Dictionary<int, Card> cards = new Dictionary<int, Card>();
            private int ctr = 0;
            private List<Card> hero = new List<Card>(40), villain = new List<Card>(40);

            public Card makeCard(Player owner, CardId id)
            {
                int i = ctr++;
                Card c = new Card(id);
                c.setId(i);
                c.owner = owner;
                c.controller = owner;
                cards.Add(i, c);
                if (owner.isHero)
                {
                    hero.Add(c);
                }
                else
                {
                    villain.Add(c);
                }

                return c;
            }

            public List<Card> makeList(Player p, params CardId[] ids)
            {
                return ids.Select((a) => makeCard(p, a)).ToList(); //LINQ: pretty and readable
            }

            public Card getCardById(int i)
            {
                return cards[i];
            }
        }
    }
    

    public enum Step
    {
        UNTOP,
        MAIN1,
        STARTCOMBAT,
        ATTACKERS,
        DEFENDERS,
        DAMAGE,
        MAIN2,
        END,
    }
}
