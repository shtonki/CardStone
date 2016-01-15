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
        private Label health;
        public PlayerButton playerButton { get; private set; }
        private GameInterface game;

        private string
            hlt = "x",
            dck = "x",
            hnd = "x",
            yrd = "x";

        private static Font f = new Font(new FontFamily("Comic Sans MS"), 20);

        private static int x = 0;
        public PlayerPanel(GameInterface g)
        {
            game = g;

            for (int i = 0; i < 5; i++)
            {
                manaButtons[i] = new ManaButton[6];
            }

            Size = new Size(300, 350);
            BackColor = Color.Aquamarine;

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Color c = Color.Chartreuse;
                    switch (i)
                    {
                        case ManaCoster.WHITE:
                        {
                            c = Color.White;
                        } break;
                        case ManaCoster.BLUE:
                        {
                            c = Color.Blue;
                        } break;
                        case ManaCoster.BLACK:
                        {
                            c = Color.Black;
                        } break;
                        case ManaCoster.RED:
                        {
                            c = Color.Red;
                        } break;
                        case ManaCoster.GREEN:
                        {
                            c = Color.Green;
                        } break;
                    }
                    ManaButton b = new ManaButton(i);
                    b.Location = new Point(10 + 45*j, 10 + 50*i);
                    b.setState(ManaButton.HIDDEN);
                    manaButtons[i][j] = b;
                    var j1 = j;
                    var i1 = i;
                    b.Click += (sender, args) =>
                    {
                        manaButtonPressed(i1*6 + j1, b);
                    };

                    Controls.Add(b);


                    playerButton = new PlayerButton();
                    playerButton.Size = new Size(70, 70);
                    playerButton.Location = new Point(220, 260);
                    playerButton.Click += (_, __) =>
                    {
                        game.gameElementPressed(playerButton);
                    };
                    Controls.Add(playerButton);

                    //health = new Label();
                    //health.Size = new Size(100, 100);
                    //health.AutoSize = true;
                    //health.Font = f;
                    //health.Location = new Point(10, 300);
                    //Controls.Add(health);
                }
            }

            
        }

        private void manaButtonPressed(int i, ManaButton b)
        {
            if (b.getState() == ManaButton.HIDDEN) { return; }
            game.gameElementPressed(b);
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

        public void notifyObserver(Observable o)
        {
            player = (Player)o;
            playerButton.setPlayer(player);

            for (int c = 0; c < 5; c++)
            {
                int i = 0;
                for (; i < player.getCurrentMana(c); i++)
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

            hlt = player.getHealth().ToString();
            dck = player.getDeck().Count.ToString();
            hnd = player.getHand().Count.ToString();
            yrd = player.getGraveyard().Count.ToString();


            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawString(hlt, f, new SolidBrush(Color.Black), 10, 300);
            e.Graphics.DrawString(dck, f, new SolidBrush(Color.Black), 60, 300);
            e.Graphics.DrawString(hnd, f, new SolidBrush(Color.Black), 110, 300);
            e.Graphics.DrawString(yrd, f, new SolidBrush(Color.Black), 160, 300);
             
        }

        public class ManaButton : UserControl, GameElement
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
            private int color;

            public ManaButton(int c)
            {
                Size = new Size(40, 40);
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

            public int getColor()
            {
                return color;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics graphics = e.Graphics;
                graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (state == FILLED)
                {
                    graphics.FillEllipse(brushes[color], 0, 0, 39, 39);
                }
                else if (state == HOLLOW)
                {
                    graphics.DrawEllipse(pens[color], thickness - 2, thickness - 2, 39 - thickness, 39 - thickness);
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

    public class PlayerButton : Button, GameElement
    {
        private Player p;

        public PlayerButton()
        {
            
        }

        public void setPlayer(Player player)
        {
            p = player;
        }

        public Player getPlayer()
        {
            return p;
        }
    }
}
