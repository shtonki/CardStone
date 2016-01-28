using System;
using System.Collections.Generic;
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
        
        //public GamePanel gamePanel { get; private set; }

        private List<GamePanel> gamePanels = new List<GamePanel>(); 

        public DisplayPanel deckEditorPanel { get; private set; }

        public FriendPanel friendPanel { get; private set; }

        public DisplayPanel activePanel { get; private set; }
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
            //gamePanel = new GamePanel();

            setupDeckEditorPanel();
            
            friendPanel = new FriendPanel();
            friendPanel.Location = new Point(10, 880);

            Controls.Add(labelx);       //todo(seba) figure out why the fuck the game just actually stops working if you add this last instead of first like what the fuck is even going on at this point microsoft
            //Controls.Add(gamePanel);
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

        /// <summary>
        /// Creates and binds a new GamePanel to the given GameInterface
        /// </summary>
        /// <param name="g"></param>
        public void createGamePanel(GameInterface g)
        {
            GamePanel r = new GamePanel(g);
            g.setPanel(r);
            Invoke(new Action(() => { Controls.Add(r); }));
        }
        
        public void transitionTo(DisplayPanel p)
        {
            if (activePanel != null)
            {

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
            }

            activePanel = p;

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

            clearFocus();

            Invalidate();
        }

        public void showTell(string user, string message)
        {
            friendPanel.getWhisper(user, message);
        }

        public void clearFocus()
        {
            if (labelx.InvokeRequired)
            {
                labelx.Invoke(new Action(_clearFocus));
            }
            else
            {
                _clearFocus();
            }
        }

        private void _clearFocus()
        {
            if (!labelx.Focus()) {  }
        }

        public bool focusCleared()
        {
            return labelx.Focused;
        }

        public void updateResolution()
        {
            
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
            
        int width = 10;

        int wingThickness = 20;

        public Xd()
        {
            BackColor = Color.Fuchsia;
            Size = new Size(2000, 1500);
            Enabled = false;
        }


        public void setStartAndEnd(Point start, Point end)
        {
            setStartAndEnd(start.X, start.Y, end.X, end.Y);
        }

        private static void rotatePointAround(ref Point p, int x, int y, double angle)
        {
            int px = p.X, py = p.Y;

            double sin = Math.Sin(angle);
            double cos = Math.Cos(angle);

            px -= x;
            py -= y;

            int nx = (int)Math.Round(px * cos - py * sin);
            int ny = (int)Math.Round(py * cos + px * sin);

            p.X = nx + x;
            p.Y = ny + y;
        }
        
        //todo(seba) make this not cover enitire sceen
        public void setStartAndEnd(int x1, int y1, int x2, int y2)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;
            double angle = Math.Atan2(dy, dx);
            int length = (int)(Math.Sqrt(dx*dx + dy*dy)*0.70710678118);
            
            Point[] pts = {
            new Point(0, 0),
            new Point(width, 0),
            new Point(length - width, length - 2*width),
            new Point(length - width, length - width - wingThickness),
            new Point(length, length - width - wingThickness),
            new Point(length, length) ,
            new Point(length - width - wingThickness, length),
            new Point(length - width - wingThickness, length - width),
            new Point(length - 2*width, length - width),
            new Point(0, width),
            };

            

            for (int i = 0; i < pts.Length; i++)
            {
                rotatePointAround(ref pts[i], 0, 0, angle - 0.785398f);
                pts[i].X += x1;
                pts[i].Y += y1;
            }

            GraphicsPath path = new GraphicsPath(pts, types);
            this.Region = new Region(path);
        }
        
    }

    class ArrowPanel : Xd
    {
        public ArrowPanel()
        {
            
        }
    }

    public class GamePanel : DisplayPanel, Resolutionable
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
        private List<ArrowPanel> arrows = new List<ArrowPanel>();   //todo(seba) allow the arrow to move when what it's pointing to/from moves

        public string message { get { return choicePanel.Text; } set { choicePanel.Text = value; } }

        public GamePanel(GameInterface g)
        {
            gameInterface = g;
            BackColor = Color.Silver;
            //Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            /*
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
            */
            
            
            handPanel = new CardPanel(20, ()=>new CardButton(g), new LayoutArgs(0, 1, false, false));
            handPanel.Location = new Point(400, 660);


            choicePanel = new ChoicePanel(g);
            choicePanel.Location = new Point(20, 370);

            heroPanel = new PlayerPanel(g);
            heroPanel.Location = new Point(20, 525);

            villainPanel = new PlayerPanel(g);
            villainPanel.Location = new Point(20, 10);
            
            //stackPanel = new CardBox(g, 190, 500);
            stackPanel = new CardPanel(20, () => new CardButton(g), new LayoutArgs(1, 0, true, false));
            //stackPanel.Location = new Point(400, 20);
            //stackPanel.Size = new Size(190, 500);

            //heroFieldPanel = new FieldPanel(g, true);
            heroFieldPanel = new CardPanel(20, () => new SnapCardButton(g, -40), new LayoutArgs(0, 1, false, false));
            heroFieldPanel.BackColor = Color.MediumSeaGreen;
            //heroFieldPanel.Location = new Point(600, 330);
            
            villainFieldPanel = new CardPanel(20, () => new SnapCardButton(g, 40), new LayoutArgs(0, 1, false, false));
            villainFieldPanel.BackColor = Color.Maroon;



            turnPanel = new TurnPanel();
            turnPanel.Location = new Point(325, 200);

            
            Controls.Add(choicePanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            //Controls.Add(textPanel);
            Controls.Add(stackPanel);
            Controls.Add(heroFieldPanel);
            Controls.Add(villainFieldPanel);
            Controls.Add(turnPanel);
            Controls.Add(villainPanel);
            Visible = false;

            updateResolution();
        }


        public void updateResolution()
        {
            Size = new Size(
                Resolution.get(ElementDimensions.FrameWidth),
                Resolution.get(ElementDimensions.FrameHeight));
            
            stillRetardedLambda(handPanel, ElementDimensions.HandPanelPaddingX);
            stillRetardedLambda(heroFieldPanel, ElementDimensions.HeroFieldPanelPaddingX);
            stillRetardedLambda(villainFieldPanel, ElementDimensions.VillainFieldPanelPaddingX);
            stillRetardedLambda(stackPanel, ElementDimensions.StackPanelPaddingX);

            imRetardedLambda(handPanel, ElementDimensions.HandPanelLocationX);
            imRetardedLambda(heroFieldPanel, ElementDimensions.HeroFieldPanelLocationX);
            imRetardedLambda(villainFieldPanel, ElementDimensions.VillainFieldPanelLocationX);
            imRetardedLambda(stackPanel, ElementDimensions.StackPanelLocationX);

        }

        private void imRetardedLambda(Control c, ElementDimensions d)
        {
            int b = (int)d;
            int lx = Resolution.getHack(b++);
            int ly = Resolution.getHack(b++);
            int sx = Resolution.getHack(b++);
            int sy = Resolution.getHack(b++);
            
            c.Location = new Point(lx, ly);
            c.Size = new Size(sx, sy);
        }

        private void stillRetardedLambda(CardPanel c, ElementDimensions d)
        {
            int b = (int)d;
            int px = Resolution.getHack(b++);
            int py = Resolution.getHack(b++);
            int pl = Resolution.getHack(b++);
            int pt = Resolution.getHack(b++);
            
            c.placeButtons(new PlacementArgs(px, py, pl, pt));
        }


        public void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.setObserver(heroPanel);
            villain.setObserver(villainPanel);

            hero.hand.setObserver(handPanel);

            stack.setObserver(stackPanel);

            hero.field.setObserver(heroFieldPanel);
            villain.field.setObserver(villainFieldPanel);
        }

        public void setStep(int s, bool a)
        {
            turnPanel.setStep(s, a);
        }
        

        public void showButtons(uint i)
        {
            choicePanel.showButtons(i);
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


    public class DisplayPanel : Panel
    {
        public virtual void handleKeyPress(Keys key)
        {
            
        }
    }

}