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
        //todo(seba) make this an actual stack and not just retarded
        private string poppedMessage;
        private uint poppedButtons;
        private uint currentButtons;

        public Game game { get; private set; }
        public GamePanel gamePanel { get; private set; }
        


        public GameInterface()
        {

        }

        public void push()
        {
            poppedMessage = gamePanel.message;
            poppedButtons = currentButtons;
            clear();
        }

        public void pop()
        {
            gamePanel.message = poppedMessage;
            gamePanel.showButtons(poppedButtons);
        }

        public void clear()
        {
            gamePanel.message = "";
            setChoiceButtons();
        }

        public void setMessage(string s)
        {
            gamePanel.message = s;
        }

        public void setChoiceButtons(params Choice[] cs)
        {
            uint i = cs.Aggregate<Choice, uint>(0, (current, choice) => current | (uint)choice);

            currentButtons = i;
            gamePanel.showButtons(i);
        }

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
            gamePanel.addArrow(@from, to);
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

        //public Action resetFakeMana => gamePanel.heroPanel.resetFakeMana;
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
                    gameElementPressed(new GameElement(Choice.PASS));
                } break;
            }
        }

        public void showCards(Card[] cs)
        {
            //throw new NotImplementedException();
            /*
            CardPanel p = new CardPanel(()=>new CardButton(this), )
            p.Size = new Size(200, 200);
            p.BackColor = Color.Navy;
            GUI.showWindow(p);
            */
        }
    }

    
}
