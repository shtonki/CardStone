using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace stonekart
{
    /// <summary>
    /// Basically just a wrapper for the gamePanel class so 
    /// that a Game can draw what it needs to draw.
    /// </summary>
    public class GameInterface
    {
        private Choice[] currentButtons;

        public Game game { get; private set; }
        public GamePanel gamePanel { get; private set; }

        

        private Stack<Tuple<string, Choice[]>> cruft = new Stack<Tuple<string, Choice[]>>();

        public GameInterface()
        {
            currentButtons = new Choice[] { };
        }
        

        public void clearContext()
        {
            var t = cruft.Pop();
            gamePanel.message = t.Item1;
            gamePanel.showButtons(t.Item2);
        }

        public void setContext(string message, params Choice[] cs)
        {
            cruft.Push(new Tuple<string, Choice[]>(gamePanel.message, currentButtons));
            gamePanel.showButtons(cs);
            gamePanel.message = message;
            currentButtons = cs;
        }

        public void changeMessage(string s)
        {
            
        }
        /*
        public void setMessage(string s)
        {
            gamePanel.message = s;
        }

        public void setChoiceButtons(params Choice[] cs)
        {
            gamePanel.showButtons(cs);
        }
        */
        //hack overhaul this
        public void showAddMana(bool b)
        {
            gamePanel.showAddMana(b);
        }

        public IEnumerable<Control> getCardButtons(Card c)
        {
            List<Control> r = new List<Control>();

            foreach (Observer o in c.getObservers())
            {
                if (o is CardButton)
                {
                    r.Add((Control)o);
                }
            }

            return r;
        }

        public List<PlayerButton> getPlayerButton(Player p)
        {
            List<PlayerButton> r = new List<PlayerButton>();

            foreach (Observer o in p.getObservers())
            {
                if (o is PlayerPanel)
                {
                    r.Add(((PlayerPanel)o).playerPortrait);
                }
            }

            return r;
        }

        public void addArrow(GameUIElement from, GameUIElement to)
        {
            gamePanel.addArrow(from, to);
        }

        public void addArrows(CardButton f, IEnumerable<Target> ts)
        {
            foreach (Target t in ts)
            {
                Observable o;
                if (t.isCard())
                {
                    o = t.getCard();
                }
                else if (t.isPlayer())
                {
                    o = t.getPlayer();
                }
                else
                {
                    throw new Exception();
                }
                foreach (var v in o.getObservers())
                {
                    if (v is CardButton)
                    {
                        addArrow(f, v as CardButton);
                    }
                    else if (v is PlayerPanel)
                    {
                        addArrow(f, (v as PlayerPanel).playerPortrait);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
        }

        public void clearArrows()
        {
            gamePanel.clearArrows();
        }

        public void setObservers(Player h, Player v, Pile s)
        {
            gamePanel.setObservers(h, v, s);
        }

        public void setStep(TurnTracker t)
        {
            gamePanel.setStep((int)t.step, t.heroTurn);
        }

        public void sendMessage(string s)
        {
            
        }

        private WaitFor<GameElement> waitForGameElement = new WaitFor<GameElement>();

        public void gameElementPressed(GameElement e)
        {
            GUI.globalEscape();
            waitForGameElement.signal(e);
        }

        public GameElement getNextGameElementPress()
        {
            GameElement r = waitForGameElement.wait();
            return r;
        }

        public Choice getChoice(string message, params Choice[] cs)
        {
            setContext(message, cs);
            while (true)
            {
                GameElement g = getNextGameElementPress();
                if (g.choice != null)
                {
                    clearContext();
                    return g.choice.Value;
                }
            }
        }

        public void resetFakeMana()
        {
            gamePanel.heroPanel.resetFakeMana();
        }

        public void setFakeManas(int[] ms)
        {
            gamePanel.heroPanel.setFakeManas(ms);
        }

        public void decrementFakeMana(int c)
        {
            gamePanel.heroPanel.adjustFakeMana(c, -1);
        }

        public Colour getManaColour()
        {
            while (true)
            {
                GameElement g = getNextGameElementPress();
                if (g.manaColour != null)
                {
                    return g.manaColour.Value;
                }
            }
        }

        public void setGame(Game g)
        {
            if (game != null) { throw new RowNotInTableException();}
            game = g;
        }

        public void setPanel(GamePanel p)
        {
            if (gamePanel != null) { throw new RowNotInTableException(); }
            gamePanel = p;
        }

        public void keyPressed(Keys key)
        {
            switch (key)
            {
                case  Keys.F6:
                {
                    game.autoPass = !game.autoPass;
                    gameElementPressed(new GameElement(Choice.Pass));
                } break;
            }
        }

        public CardPanelControl showCards(params Card[] cs)
        {
            Pile pl = new Pile(cs);
            CardPanelControl c = new CardPanelControl(pl);
            return c;
        }
        
        public void showGraveyard(Player p)
        {
            CardPanel l = new CardPanel(() => new CardButton(), new LayoutArgs(false, false),p.graveyard);
            GUI.showWindow(l, new WindowedPanelArgs("Graveyard", true, true, false));
            
        }
    }



    public class CardPanelControl
    {
        public CardPanel panel { get; private set; }
        private Pile pile;
        private WaitFor<Card> waiter = new WaitFor<Card>();
        private WindowedPanel window;

        public CardPanelControl(Pile p)
        {
            panel = new CardPanel(() => 
            {
                var b = new CardButton(new FML(clickedCallback));
                b.setHeight(150);
                return b;
            }, new LayoutArgs(false, false));
            panel.Size = new Size(300, 150);
            panel.BackColor = Color.Navy;
            p.addObserver(panel);
            window = GUI.showWindow(panel);
        }
        

        public Card waitForCard()
        {
            return waiter.wait();
        }

        public void closeWindow()
        {
            //todo(seba) something needs to be released here
            window.close();
        }

        private void clickedCallback(CardButton b)
        {
            Card c = b.Card;
            waiter.signal(c);
        }

    }
}
