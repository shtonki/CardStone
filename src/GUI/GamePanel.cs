using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    public class GamePanel : DisplayPanel
    {
        private GameInterface gameInterface;
        private TextBox inputBox, outputBox;
        public CardPanel handPanel;
        public PlayerPanel heroPanel, villainPanel;
        private ChoicePanel choicePanel;
        public CardPanel stackPanel;
        public CardPanel heroFieldPanel;
        public CardPanel villainFieldPanel;
        private TurnPanel turnPanel;
        private CardInfoPanel cardInfoPanel;
        private List<ArrowPanel> arrows = new List<ArrowPanel>();   //todo(seba) allow the arrow to move when what it's pointing to/from moves

        public string message { get { return choicePanel.Text; } set { choicePanel.Text = value; } }

        public GamePanel(GameInterface g)
        {
            gameInterface = g;
            BackColor = Color.Silver;

            Action<CardButton> clickCallBack = (b) => g.gameElementPressed(new GameElement(b.Card));
            FML f = new FML(clickCallBack, g.addArrows, g.clearArrows, (c) => g.setFocusCard(c.Card));
            handPanel = new CardPanel(()=>new CardButton(f), new LayoutArgs(false, false));
            //handPanel.Location = new Point(400, 660);

            cardInfoPanel = new CardInfoPanel();

            choicePanel = new ChoicePanel(g);
            //choicePanel.Location = new Point(20, 370);

            heroPanel = new PlayerPanel(g);
            //heroPanel.Location = new Point(20, 525);

            villainPanel = new PlayerPanel(g);
            //villainPanel.Location = new Point(20, 10);
            
            //stackPanel = new CardBox(g, 190, 500);
            stackPanel = new CardPanel(() => new CardButton(f), new LayoutArgs(true, true, 0.25));
            //stackPanel.Location = new Point(400, 20);
            //stackPanel.Size = new Size(190, 500);

            //heroFieldPanel = new FieldPanel(g, true);
            heroFieldPanel = new CardPanel(() => new SnapCardButton(f, true), new LayoutArgs(false, false));
            heroFieldPanel.BackColor = Color.MediumSeaGreen;
            //heroFieldPanel.Location = new Point(600, 330);
            
            villainFieldPanel = new CardPanel(() => new SnapCardButton(f, false), new LayoutArgs(false, false));
            villainFieldPanel.BackColor = Color.Maroon;



            turnPanel = new TurnPanel();
            //turnPanel.Location = new Point(325, 200);
            
            Controls.Add(choicePanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            //Controls.Add(textPanel);
            Controls.Add(stackPanel);
            Controls.Add(heroFieldPanel);
            Controls.Add(villainFieldPanel);
            Controls.Add(turnPanel);
            Controls.Add(villainPanel);
            Controls.Add(cardInfoPanel);
            Visible = false;
        }
        

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            
            int height = Size.Height;
            int width = Size.Width;

            double fkme = 0.06*height;

            int fieldPanelW = (int)(width * 0.6 - fkme);
            int fieldPanelX = (int)(width * 0.245 + fkme);

            int handPanelX = (int)(width * 0.152);
            int handPanelY = (int)(height * 0.7);
            int handPanelW = (int)(width * 0.6);
            int handPanelH = (int)(height * 0.3);
            handPanel.Location = new Point(handPanelX, handPanelY);
            handPanel.Size = new Size(handPanelW, handPanelH);

            int heroFieldPanelY = (int)(height * 0.3);
            int heroFieldPanelH = (int)(height * 0.300);
            heroFieldPanel.Location = new Point(fieldPanelX, heroFieldPanelY);
            heroFieldPanel.Size = new Size(fieldPanelW, heroFieldPanelH);

            int villainFieldPanelY = (int)(height * 0);
            int villainFieldPanelH = (int)(height * 0.300);
            villainFieldPanel.Location = new Point(fieldPanelX, villainFieldPanelY);
            villainFieldPanel.Size = new Size(fieldPanelW, villainFieldPanelH);


            int turnPanelX = (int)(width * 0.245);
            int turnPanelY = (int)(height * 0);
            int turnPanelH = (int)(height * 0.6);
            turnPanel.Location = new Point(turnPanelX, turnPanelY);
            turnPanel.setHeight(turnPanelH);

            int stackPanelX = (int)(width * 0.152);
            int stackPanelY = (int)(height * 0);
            int stackPanelW = (int)(width * 0.09);
            int stackPanelH = (int)(height * 0.6);
            stackPanel.Location = new Point(stackPanelX, stackPanelY);
            stackPanel.Size = new Size(stackPanelW, stackPanelH);

            int playerPanelX = (int)(width * 0.0);
            int playerPanelW = (int)(width * 0.15);
            int playerPanelH = (int)(height * 0.3);

            int heroPanelY = (int)(height * 0.6);
            heroPanel.Location = new Point(playerPanelX, heroPanelY);
            heroPanel.Size = new Size(playerPanelW, playerPanelH);

            int choicePanelY = (int)(height * 0.3);
            choicePanel.Location = new Point(playerPanelX, choicePanelY);
            choicePanel.Size = new Size(playerPanelW, playerPanelH);

            int villainPanelY = (int)(height * 0.0);
            villainPanel.Location = new Point(playerPanelX, villainPanelY);
            villainPanel.Size = new Size(playerPanelW, playerPanelH);

            int cardInfoPanelX = (int)(width * (1-0.154));
            int cardInfoPanelY = (int)(height * 0);
            int cardInfoPanelW = (int)(width * 0.154);
            int cardInfoPanelH = (int)(height * 0.6);
            cardInfoPanel.Location = new Point(cardInfoPanelX, cardInfoPanelY);
            cardInfoPanel.Size = new Size(cardInfoPanelW, cardInfoPanelH);
        }


        public void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.addObserver(heroPanel);
            villain.addObserver(villainPanel);

            hero.hand.addObserver(handPanel);

            stack.addObserver(stackPanel);

            hero.field.addObserver(heroFieldPanel);
            villain.field.addObserver(villainFieldPanel);
        }

        public void setStep(int s, bool a)
        {
            turnPanel.setStep(s, a);
        }

        public void showCardInfo(Card c)
        {
            cardInfoPanel.showCard(c);
        }

        public void showButtons(Choice[] cs)
        {
            choicePanel.showButtons(cs);
        }

        public void showAddMana(bool b)
        {
            heroPanel.showAddMana(b);
        }

        public void addArrow(GameUIElement from, GameUIElement to)
        {
            ArrowPanel a = new ArrowPanel();
            Control f = (Control)from;
            Control t = (Control)to;
            a.setStartAndEnd(fn(f), fn(t));
            arrows.Add(a);
            Controls.Add(a);
            a.BringToFront();
        }

        //finds center of control relative to the form it's in hence the name fn
        private static Point fn(Control control)
        {
            Point r = control.FindForm().PointToClient(control.Parent.PointToScreen(control.Location));
            r.X += control.Width/2;
            r.Y += control.Height/2;
            return r;
        }

        public override void handleKeyPress(Keys key)
        {
            gameInterface.keyPressed(key);
        }

        public void clearArrows()
        {
            foreach (ArrowPanel a in arrows)
            {
                Controls.Remove(a);
            }
            arrows.Clear();
        }


    }

    class CardInfoPanel : Panel
    {
        private CardButton pb;
        private Label[] labelList;
        private const int MAX_SPECIAL_ABILITIES = 5;
        private const int BOX_HEIGHT = 50;
        private static string[] KeyAbilityDescription;

        static CardInfoPanel()
        {
            KeyAbilityDescription = new string[Enum.GetNames(typeof(KeyAbility)).Length];
            KeyAbilityDescription[(int)KeyAbility.Fervor] = ": creature can attack same turn it is played.";
        }

        public CardInfoPanel()
        {
            BackColor = Color.Olive;
            pb = new CardButton();
            labelList = new Label[MAX_SPECIAL_ABILITIES];
            Controls.Add(pb);
            for(int i = 0; i < MAX_SPECIAL_ABILITIES; i++)
            {
                labelList[i] = new Label();
                Controls.Add(labelList[i]);
            }
        }

        public void showCard(Card c)
        {
            for(int i = 0; i < MAX_SPECIAL_ABILITIES; i++)
            {
                if(i < c.keyAbilities.Count) //if the ability even exists
                {
                    labelList[i].Visible = true;
                    labelList[i].Text = c.keyAbilities[i].ToString() + KeyAbilityDescription[(int)c.keyAbilities[i]];
                    labelList[i].BackColor = Color.Black;
                    labelList[i].ForeColor = Color.Beige;
                }
                else
                {
                    labelList[i].Visible = false;
                }
            }
            pb.notifyObserver(c, null);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            pb.setWidth(Size.Width - 5);
            for(int i = 0; i < MAX_SPECIAL_ABILITIES; i++)
            {
                labelList[i].Size = new Size(Size.Width-5, BOX_HEIGHT);
                labelList[i].Location = new Point(0, /*Location.Y*/0 + pb.Height + BOX_HEIGHT * i);
            }
        }
    }
}