using System;
using System.Data.SqlTypes;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;


namespace stonekart
{
    public class PlayerPanel : Panel, Observer
    {
        private ManaButton[][] manaButtons = new ManaButton[5][];
        private Player player;
        //private Label health;
        public PlayerButton playerPortrait { get; private set; }
        private GameInterface game;

        private int[] fakes = new int[5];

        private const int NORMALBUTTONS = 5;
        private Button[] normalButtons;
        private Button health => normalButtons[1];
        private Button deck => normalButtons[2];
        private Button hand => normalButtons[3];
        private Button yard => normalButtons[4];

        private static Font f = new Font(new FontFamily("Comic Sans MS"), 20);
        
        public PlayerPanel(GameInterface g)
        {
            game = g;

            for (int i = 0; i < 5; i++)
            {
                manaButtons[i] = new ManaButton[6];
            }

            normalButtons = new Button[NORMALBUTTONS];

            for (int i = 1; i < NORMALBUTTONS; i++)
            {
                normalButtons[i] = new Button();
            }

            yard.Click += clickGraveyard;
            
            Controls.Add(health);
            Controls.Add(deck);
            Controls.Add(hand);
            Controls.Add(yard);

            BackColor = Color.Aquamarine;
            
            for (int i = 0; i < 5; i++)
            {
                Colour colour = (Colour)i;
                for (int j = 0; j < 6; j++)
                {
                    Color c = Color.Chartreuse;
                    switch (colour)
                    {
                        case Colour.WHITE:
                        {
                            c = Color.White;
                        } break;
                        case Colour.BLUE:
                        {
                            c = Color.Blue;
                        } break;
                        case Colour.BLACK:
                        {
                            c = Color.Black;
                        } break;
                        case Colour.RED:
                        {
                            c = Color.Red;
                        } break;
                        case Colour.GREEN:
                        {
                            c = Color.Green;
                        } break;
                    }
                    ManaButton b = new ManaButton(colour);
                    b.setState(ManaButton.HIDDEN);
                    manaButtons[i][j] = b;
                    var j1 = j;
                    var i1 = i;
                    b.Click += (sender, args) =>
                    {
                        manaButtonPressed(b);
                    };

                    Controls.Add(b);
                }
            }

            playerPortrait = new PlayerButton();
            normalButtons[0] = playerPortrait;
            playerPortrait.Click += (_, __) =>
            {
                game.gameElementPressed(playerPortrait.getElement());
            };
            Controls.Add(playerPortrait);

        }

        private void manaButtonPressed(ManaButton b)
        {
            if (b.getState() == ManaButton.HIDDEN) { return; }
            game.gameElementPressed(b.getElement());
        }

        public void resetFakeMana()
        {
            for (int i = 0; i < 5; i++)
            {
                fakes[i] = 0;
            }
            updateManaDisplay();
        }

        public void setFakeManas(int[] ns)
        {
            for (int i = 0; i < 5; i++)
            {
                fakes[i] = ns[i];
            }
            updateManaDisplay();
        }

        public void adjustFakeMana(int c, int i)
        {
            fakes[c] -= i;
            updateManaDisplay();
        }

        public void showAddMana(bool y)
        {
            int q = y ? 1 : 0;

            for (int c = 0; c < 5; c++)
            {
                int i = 0;
                for (; i < player.getCurrentMana(c); i++)
                {
                    manaButtons[c][i].setState(ManaButton.FILLED);
                }
                for (; i < q + player.getMaxMana(c); i++)
                {
                    if (i == 6) { break; }
                    manaButtons[c][i].setState(ManaButton.HOLLOW);
                }
                for (; i < 6; i++)
                {
                    manaButtons[c][i].setState(ManaButton.HIDDEN);
                }
            }
        }

        private void updateManaDisplay()
        {
            for (int c = 0; c < 5; c++)
            {
                int i = 0;
                for (; i < player.getCurrentMana(c) - fakes[c]; i++)
                {
                    manaButtons[c][i].setState(ManaButton.FILLED);
                }
                for (; i < player.getMaxMana(c); i++)
                {
                    manaButtons[c][i].setState(ManaButton.HOLLOW);
                }
                for (; i < 6; i++)
                {
                    manaButtons[c][i].setState(ManaButton.HIDDEN);
                }
            }
        }

        private void clickGraveyard(object o, EventArgs a)
        {
            game.showGraveyard(player);
        }

        public void notifyObserver(Observable o, object args)
        {
            player = (Player)o;
            playerPortrait.player = player;
            
            updateManaDisplay();

            safeSetText(health, player.getHealth().ToString());
            safeSetText(deck, player.deck.count.ToString());
            safeSetText(hand, player.hand.count.ToString());
            safeSetText(yard, player.graveyard.count.ToString());
            
            Invalidate();
        }

        private static void safeSetText(Button b, string s)
        {
            if (b.InvokeRequired)
            {
                b.Invoke(new Action(() => b.Text = s));
            }
            else
            {
                b.Text = s;
            }
        }
        

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            base.OnPaint(e);
        }
        

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);

            //Size = new Size(300, 350);
            int height = Size.Height;
            int width = Size.Width;
            int nsY = (int)(height*27.0/35.0);
            int nsH = (int)(height * 7.0 / 35.0);
            int nsW = (int)(width * 6.8 / 35.0);
            int healthX = (int)(width * 1.0 / 30.0);
            int deckX = (int)(width * 6.0 / 30.0); 
            int handX = (int)(width * 11.0 / 30.0);
            int yardX = (int)(width * 16.0 / 30.0);
            int sidePadding = 3;
            int padX = (int)(width * 45.0 / 300.0); 
            int padY = (int)(height * 45.0 / 350.0);
            int orbW = (int)(width * 40.0 / 300.0);
            int orbH = (int)(height * 40.0 / 350.0);
            Size s = new Size(orbW, orbH);

            for (int i = 0; i < NORMALBUTTONS; i++)
            {
                normalButtons[i].Location = new Point(sidePadding + nsW*i, nsY);
                normalButtons[i].Size = new Size(nsW, nsH);
            }

            for (int i = 0; i < 5; i++)
            {
                Colour colour = (Colour)i;
                for (int j = 0; j < 6; j++)
                {
                    manaButtons[i][j].Location = new Point(sidePadding + padX * j, sidePadding + padY * i);
                    manaButtons[i][j].Size = s;
                    manaButtons[i][j].Invalidate();
                }
            }

            Invalidate();
        }

        public class ManaButton : UserControl, GameUIElement
        {
            public const int
                FILLED = 0,
                HOLLOW = 1,
                HIDDEN = 2;

            //private static Color[] colors = new[] {Color.White, Color.Blue, Color.Black, Color.Red, Color.Green,};
            private static Brush[] brushes = new []{new SolidBrush(Color.White), new SolidBrush(Color.Blue), 
                new SolidBrush(Color.Black), new SolidBrush(Color.Red), new SolidBrush(Color.Green) };
            private static Pen[] pens = new [] { new Pen(Color.White, thickness), new Pen(Color.Blue, thickness),
                new Pen(Color.Black, thickness), new Pen(Color.Red, thickness), new Pen(Color.Green, thickness), };

            private const int thickness = 4;

            private int state = 0;
            private Colour color;

            public ManaButton(Colour c)
            {
                color = c;
                state = FILLED;
            }
            

            public void setState(int i)
            {
                state = i;
                Invalidate();
            }

            public int getState()
            {
                return state;
            }

            public GameElement getElement()
            {
                return new GameElement(color);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                int width = Size.Width - 1;
                int height = Size.Height - 1;
                if (state == FILLED)
                {
                    graphics.FillEllipse(brushes[(int)color], 0, 0, width, height);
                }
                else if (state == HOLLOW)
                {
                    graphics.DrawEllipse(pens[(int)color], thickness - 2, thickness - 2, width - thickness, height - thickness);
                }
                else if (state == HIDDEN)
                {
                    
                }
                else
                {
                    throw new Exception("fghj");
                }
            }
        }
    }


    public class PlayerButton : Button, GameUIElement
    {
        public Player player;

        public PlayerButton()
        {
            
        }

        public GameElement getElement()
        {
            return new GameElement(player);
        }
        /*
        public void setPlayer(Player player)
        {
            p = player;
        }

        public Player getPlayer()
        {
            return p;
        }
        */
    }
}
