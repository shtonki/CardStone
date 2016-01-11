using System;
using System.Data;

namespace stonekart
{
    /// <summary>
    /// Basically just a wrapper for the gamePanel class so 
    /// that a Game can draw what it needs to draw.
    /// </summary>
    public class GameUI
    {
        //todo(seba) make this an actual stack and not just retarded
        private string poppedMessage;
        private int poppedButtons;
        private int currentButtons;

        public Game game { get; private set; }
        public GamePanel gamePanel { get; private set; }

        private ThankGodThereIsNoFriendKeywordInThisLanguageOrIWouldntNeedToDoThisNonsense nonsense;

        public GameUI(ThankGodThereIsNoFriendKeywordInThisLanguageOrIWouldntNeedToDoThisNonsense n)
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
            setChoiceButtons(GUI.NOTHING);
        }

        public void setMessage(string s)
        {
            gamePanel.message = s;
        }

        public void setChoiceButtons(int i)
        {
            currentButtons = i;
            gamePanel.showButtons(i);
        }

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

        public void addArrow(GameElement from, GameElement to)
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

        public void gameElementPressed(GameElement e)
        {
            game.gameElementPressed(e);
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
    }
}