using System;
using System.Data;
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

        private ThankGodThereIsNoFriendKeywordInThisLanguageOrIWouldntNeedToDoThisNonsense nonsense;

        public GameInterface(ThankGodThereIsNoFriendKeywordInThisLanguageOrIWouldntNeedToDoThisNonsense n)
        {
            nonsense = n;
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

        public CardButton getCardButton(Card c)
        {
            return c.getObserver(nonsense) as CardButton;
        }

        public PlayerButton getPlayerButton(Player p)
        {
            PlayerPanel o = p.getObserver(nonsense) as PlayerPanel;
            return o.playerButton;
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

        public void setStep(int s, bool a)
        {
            gamePanel.setStep(s, a);
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

        public ManaColour getManaColour()
        {
            while (true)
            {
                GameElement g = getNextGameElementPress();
                if (g.manaColor != null)
                {
                    return g.manaColor.Value;
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
    }

    
}