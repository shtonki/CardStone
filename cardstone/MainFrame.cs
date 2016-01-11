using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Resources;
using System.Windows.Forms;

namespace stonekart
{
    

    public partial class MainFrame : Form
    {

        public MainMenuPanel mainMenuPanel { get; private set; }
        
        public GamePanel gamePanel { get; private set; }
        
        public DisplayPanel deckEditorPanel { get; private set; }

        public FriendPanel friendPanel { get; private set; }

        private DisplayPanel activePanel;
        private Button labelx;

        public MainFrame()
        {
            labelx = new Button();
            labelx.Size = new Size(0, 0);
            labelx.KeyDown += (sender, args) =>
            {
                handleGlobalKeyDown(args.KeyCode);
            };

            //InitializeComponent();
            BackColor = Color.Fuchsia;
            Application.AddMessageFilter(new GlobalMouseHandler());

            FormClosed += (sender, args) => { Environment.Exit(0); };

            Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;


            //setupMainMenuPanel();
            mainMenuPanel = new MainMenuPanel();

            //setupGamePanel();
            gamePanel = new GamePanel();

            setupDeckEditorPanel();
            
            friendPanel = new FriendPanel();
            friendPanel.Location = new Point(10, 880);

            Controls.Add(labelx);       //todo(seba) figure out why the fuck the game just actually stops working if you add this last instead of first like what the fuck is even going on at this point microsoft
            Controls.Add(gamePanel);
            Controls.Add(mainMenuPanel);
            Controls.Add(friendPanel);
            Controls.Add(deckEditorPanel);
            Controls.Add(deckEditorPanel);

            friendPanel.BringToFront();
            friendPanel.Hide();
            

            GUI.frameLoaded.Set();
        }

        public void handleGlobalKeyDown(Keys key)
        {
            activePanel?.handleKeyPress(key);
        }

        private void setupDeckEditorPanel()
        {
            deckEditorPanel = new DeckEditorPanel();
            deckEditorPanel.Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            deckEditorPanel.Visible = false;
        }

        public void transitionTo(DisplayPanel p)
        {
            activePanel = p;
            if (activePanel.InvokeRequired)
            {
                activePanel.Invoke(new Action(() =>
                {
                    activePanel.Visible = false;
                }));
            }
            else
            {
                activePanel.Visible = false;
            }
            if (p.InvokeRequired)
            {
                p.Invoke(new Action(() =>
                {
                    p.Visible = true;
                }));
            }
            else
            {
                p.Visible = true;
            }


            Invalidate();
        }

        public void showTell(string user, string message)
        {
            friendPanel.getWhisper(user, message);
        }

        public void clearFocus()
        {
            if (!labelx.Focus()) { throw new MissingSatelliteAssemblyException(); }
        }

        public bool focusCleared()
        {
            return labelx.Focused;
        }

        public void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.setObserver(gamePanel.heroPanel);
            villain.setObserver(gamePanel.villainPanel);

            hero.getHand().setObserver(gamePanel.handPanel);

            stack.setObserver(gamePanel.stackPanel);

            hero.getField().setObserver(gamePanel.heroFieldPanel);
            villain.getField().setObserver(gamePanel.villainFieldPanel);
        }
        
    }

    public class MainMenuPanel : DisplayPanel
    {
        public SPanel loginPanel { get; private set; }
        public SPanel startGamePanel { get; private set; }

        private TextBox usernameBox;

        public MainMenuPanel()
        {
            Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            BackColor = Color.DarkRed;

            loginPanel = new SPanel();
            loginPanel.Size = new Size(700, 400);
            loginPanel.BackColor = Color.Silver;
            loginPanel.Location = new Point((GUI.FRAMEWIDTH - 700) / 2, 200);
            loginPanel.Hide();

            Label usernameLabel = new Label();
            usernameLabel.Size = new Size(400, 50);
            usernameLabel.Location = new Point(280, 50);
            usernameLabel.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            usernameLabel.Text = "Username";


            usernameBox = new TextBox();
            usernameBox.Font = new Font(new FontFamily("Comic Sans MS"), 30);
            usernameBox.Size = new Size(400, 5);
            usernameBox.Location = new Point(150, 100);
            usernameBox.TextAlign = HorizontalAlignment.Center;

            usernameBox.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    GUI.loginWithName(usernameBox.Text);
                }
            };

            Button loginButton = new Button();
            loginButton.Location = new Point(300, 220);
            loginButton.Size = new Size(100, 50);
            loginButton.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            loginButton.Text = "Login";
            loginButton.Click += (sender, args) =>
            {
                GUI.loginWithName(usernameBox.Text);
            };

            Button playOfflineButton = new Button();
            playOfflineButton.Size = new Size(200, 50);
            playOfflineButton.Location = new Point(250, 320);
            playOfflineButton.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            playOfflineButton.Text = "Play Offline";
            playOfflineButton.Click += (sender, args) =>
            {
                GUI.loginWithName(null);
            };

            startGamePanel = new SPanel();
            startGamePanel.BackColor = Color.DarkKhaki;
            startGamePanel.Size = new Size(200, 200);
            startGamePanel.Location = new Point(400, 400);
            startGamePanel.Hide();

            Button botGameButton = new Button();
            botGameButton.Location = new Point(20, 20);
            botGameButton.Text = "Play against no one";
            startGamePanel.Controls.Add(botGameButton);
            botGameButton.Click += (a, aa) =>
            {
                GameController.newGame(new DummyConnection());
            };

            loginPanel.Controls.Add(usernameLabel);
            loginPanel.Controls.Add(usernameBox);
            loginPanel.Controls.Add(loginButton);
            loginPanel.Controls.Add(playOfflineButton);
            
            Controls.Add(startGamePanel);
            Controls.Add(loginPanel);

            Visible = false;
        }
        
        public override void handleKeyPress(Keys key)
        {
            Console.WriteLine("Pressed {0} in main menu", key);
        }
        
    }
    


    class Xd : Panel
    {
        private const bool x = false;
        private Point[] ps;

        private readonly byte[] types = {
            0,
            1,
            1,
            1,
            1,
            1,
            1, 
            1, 
            1, 
            1, 
            };

        int wfactor = 10;

        int wtf = 20;

        public Xd()
        {
            BackColor = Color.Fuchsia;
        }
        

        public void setStartAndEnd(Point start, Point end)
        {
            //setStartAndEnd(start.X, start.Y, end.X, end.Y);
        }

        private static void rotatePointAround(ref Point p, int x, int y, double angle)
        {
            angle *= 0.0174533;

            int px = p.X, py = p.Y;

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            px -= x;
            py -= y;

            int nx = (int)Math.Round(px * cos + py * sin);
            int ny = (int)Math.Round(py * cos + px * sin);

            p.X = nx + x;
            p.Y = ny + y;
        }

        //public void setStartAndEnd(int x1, int y1, int x2, int y2)
        public void setStartAndEnd(double angle)
        {
            Size = new Size(500, 500);

            int length = 70;
            Point[] pts = {
            new Point(0, 0),
            new Point(wfactor, 0),
            new Point(length - wfactor, length - 2*wfactor),
            new Point(length - wfactor, length - wfactor - wtf),
            new Point(length, length - wfactor - wtf),
            new Point(length, length) ,
            new Point(length - wfactor - wtf, length),
            new Point(length - wfactor - wtf, length - wfactor),
            new Point(length - 2*wfactor, length - wfactor),
            new Point(0, wfactor),
            };

            for (int i = 0; i < pts.Length; i++)
            {
                pts[i].X += 100;
                pts[i].Y += 100;
                rotatePointAround(ref pts[i], 100, 100, angle);
            }

            GraphicsPath path = new GraphicsPath(pts, types);
            this.Region = new Region(path);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (x)
            {
                Font f = DefaultFont;
                Brush b = new SolidBrush(Color.Black);
                base.OnPaint(e);
                for (int i = 0; i < ps.Length; i++)
                {
                    e.Graphics.DrawString(i.ToString(), f, b, ps[i]);
                }
            }
        }
    }
    

    public class GamePanel : DisplayPanel
    {
        private TextBox inputBox, outputBox;
        public CardPanel handPanel;
        public PlayerPanel heroPanel, villainPanel;
        private ButtonPanel buttonPanel;
        public CardBox stackPanel;
        public FieldPanel heroFieldPanel;
        public FieldPanel villainFieldPanel;
        private TurnPanel turnPanel;

        public GamePanel()
        {
            BackColor = Color.Silver;
            Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);

            inputBox = new TextBox();
            inputBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) { return; }

                Console.WriteLine("user typed in chat box: " + inputBox.Text);
                inputBox.Clear();
            };
            inputBox.Size = new Size(200, 40);

            outputBox = new TextBox();
            outputBox.ReadOnly = true;
            outputBox.Location = new Point(100, 100);
            outputBox.AcceptsReturn = true;
            outputBox.Multiline = true;
            outputBox.Size = new Size(200, 400);
            outputBox.ScrollBars = ScrollBars.Vertical;

            FlowLayoutPanel textPanel = new FlowLayoutPanel();
            textPanel.FlowDirection = FlowDirection.LeftToRight;
            textPanel.Size = new Size(200, 440);
            textPanel.Location = new Point(1550, 100);

            textPanel.Controls.Add(outputBox);
            textPanel.Controls.Add(inputBox);

            handPanel = new CardPanel();
            handPanel.Location = new Point(400, 660);


            buttonPanel = new ButtonPanel();
            buttonPanel.Location = new Point(20, 370);

            heroPanel = new PlayerPanel();
            heroPanel.Location = new Point(20, 525);

            villainPanel = new PlayerPanel();
            villainPanel.Location = new Point(20, 10);

            stackPanel = new CardBox(190, 500);
            stackPanel.Location = new Point(400, 20);

            heroFieldPanel = new FieldPanel(true);
            heroFieldPanel.Location = new Point(600, 330);

            villainFieldPanel = new FieldPanel(false);
            villainFieldPanel.Location = new Point(600, 10);
            

            xd = new Xd();
            xd.Location = new Point(300, 300);
            //xd.Size = new Size(600, 600);
            //xd.Location = new Point(600, 600);
            //xd.setStartAndEnd(30, 30, 200, 321);
            Timer t = new Timer();
            t.Tick += (_, __) =>
            {
                xd.setStartAndEnd(d++);
                Invalidate();
            };
            t.Interval = 25;
            t.Start();

            turnPanel = new TurnPanel();
            turnPanel.Location = new Point(325, 200);

            
            Controls.Add(xd);
            Controls.Add(buttonPanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            Controls.Add(textPanel);
            Controls.Add(stackPanel);
            Controls.Add(heroFieldPanel);
            Controls.Add(villainFieldPanel);
            Controls.Add(turnPanel);
            Controls.Add(villainPanel);
            Visible = false;
        }

        private static Xd xd;
        private static double d = 0;
        public void setStep(int s, bool a)
        {
            turnPanel.setStep(s, a);
        }

        public void setMessage(string s)
        {
            buttonPanel.setText(s);
        }

        public void showButtons(int i)
        {
            buttonPanel.showButtons(i);
        }

        public void showAddMana(bool b)
        {
            heroPanel.showAddMana(b);
        }

        class ButtonPanel : Panel
        {
            private ChoiceButton
                cancel,
                accept;
            
            private Label textLabel;

            public ButtonPanel()
            {
                BackColor = Color.CornflowerBlue;
                Size = new Size(300, 140);

                textLabel = new Label();
                textLabel.Size = new Size(280, 40);
                textLabel.Location = new Point(10, 10);
                textLabel.Font = new Font(new FontFamily("Comic Sans MS"), 14);

                accept = new ChoiceButton(GUI.ACCEPT);
                accept.Visible = false;
                accept.BackColor = Color.GhostWhite;
                accept.Text = "Accept";
                accept.Font = new Font(new FontFamily("Comic Sans MS"), 12);
                accept.Location = new Point(40, 100);
                accept.Click += (sender, args) =>
                {
                    buttonPressed(accept);
                };

                cancel = new ChoiceButton(GUI.CANCEL);
                cancel.Visible = false;
                cancel.BackColor = Color.GhostWhite;
                cancel.Text = "Cancel";
                cancel.Font = new Font(new FontFamily("Comic Sans MS"), 12);
                cancel.Location = new Point(140, 100);
                cancel.Click += (sender, args) =>
                {
                    buttonPressed(cancel);
                };

                Controls.Add(textLabel);
                Controls.Add(accept);
                Controls.Add(cancel);


            }

            public void setText(string s)
            {
                if (s == null)
                {
                    return;
                }
                Invoke(new Action(() => { textLabel.Text = s; }));

            }

            public void showButtons(int i)
            {
                Invoke(new Action(() =>
                {
                    accept.Visible = (i & GUI.ACCEPT) != 0;
                    cancel.Visible = (i & GUI.CANCEL) != 0;
                }));

            }

            private void buttonPressed(ChoiceButton b)
            {
                GameController.currentGame.fooPressed(b);
            }
        }
    }

    

    class ChoiceButton : Button, GameElement
    {
        private int type;

        public ChoiceButton(int i)
        {
            type = i;
            Size = new Size(80, 40);
        }

        public int getType()
        {
            return type;
        }
    }

    public class DisplayPanel : Panel
    {
        public virtual void handleKeyPress(Keys key)
        {
            
        }
    }

}