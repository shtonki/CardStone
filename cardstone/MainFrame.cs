using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    

    public partial class MainFrame : Form
    {
        //todo(jasin) sholdn't be const fam
        
        public MainMenuPanel mainMenuPanel { get; private set; }


        #region gamepanel
        public DisplayPanel gamePanel { get; private set; }
        /*
        private TextBox inputBox, outputBox;
        private CardPanel handPanel;
        private PlayerPanel heroPanel, villainPanel;
        private ButtonPanel buttonPanel;
        private CardBox stackPanel;
        private FieldPanel heroFieldPanel, villainFieldPanel;
        private TurnPanel turnPanel;
        */
        #endregion

        public DisplayPanel deckEditorPanel { get; private set; }

        private FriendPanel friendPanel;

        private Panel activePanel;
        
        public MainFrame()
        {
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


            Controls.Add(gamePanel);
            Controls.Add(mainMenuPanel);
            Controls.Add(friendPanel);
            Controls.Add(deckEditorPanel);

            friendPanel.BringToFront();
            friendPanel.Hide();
            

            GUI.frameLoaded.Set();
        }

        private void setupGamePanel()
        {
            gamePanel = new DisplayPanel();
            gamePanel.Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);

            inputBox = new TextBox();
            inputBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) { return; }

                Console.WriteLine(inputBox.Text);
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

            textPanel.Controls.Add(outputBox);
            textPanel.Controls.Add(inputBox);

            handPanel = new CardPanel();
            handPanel.Location = new Point(400, 660);

            textPanel.Location = new Point(1550, 500);

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

            gamePanel.Controls.Add(buttonPanel);
            gamePanel.Controls.Add(heroPanel);
            gamePanel.Controls.Add(handPanel);
            gamePanel.Controls.Add(textPanel);
            gamePanel.Controls.Add(stackPanel);
            gamePanel.Controls.Add(heroFieldPanel);
            gamePanel.Controls.Add(villainFieldPanel);
            
            turnPanel = new TurnPanel();
            turnPanel.Location = new Point(325, 200);

            gamePanel.Controls.Add(buttonPanel);
            gamePanel.Controls.Add(heroPanel);
            gamePanel.Controls.Add(handPanel);
            gamePanel.Controls.Add(textPanel);
            gamePanel.Controls.Add(stackPanel);
            gamePanel.Controls.Add(heroFieldPanel);
            gamePanel.Controls.Add(villainFieldPanel);
            gamePanel.Controls.Add(turnPanel);
            gamePanel.Controls.Add(villainPanel);

            gamePanel.Visible = false;
        }

        /*
        private void setupMainMenuPanel()
        {
            mainMenuPanel = new DisplayPanel();
            mainMenuPanel.Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            mainMenuPanel.BackColor = Color.DarkRed;

            loginPanel = new Panel();
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
            
            Button b = new Button();
            b.Location = new Point(300, 220);
            b.Size = new Size(100, 50);
            b.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            b.Text = "Login";
            b.Click += (sender, args) =>
            {
                GUI.loginWithName(usernameBox.Text);
            };

            Button c = new Button();
            c.Location = new Point(300, 320);
            c.Size = new Size(100, 50);
            c.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            c.Text = "Nyet";
            c.Click += (sender, args) =>
            {
                Console.WriteLine("i'm not entirely sure what this button is supposed to do");
            };

            startGamePanel = new Panel();
            startGamePanel.BackColor = Color.DarkKhaki;
            startGamePanel.Size = new Size(200, 200);
            startGamePanel.Location = new Point(400, 400);
            startGamePanel.Hide();

            Button botGameButton = new Button();
            botGameButton.Location = new Point(20, 20);
            botGameButton.Text = "Play against no one";
            startGamePanel.Controls.Add(botGameButton);
            botGameButton.Click += (a,aa) =>
            {
                GameController.newGame(new DummyConnection());
            };

            loginPanel.Controls.Add(usernameLabel);
            loginPanel.Controls.Add(usernameBox);
            loginPanel.Controls.Add(b);
            loginPanel.Controls.Add(c);
            

            mainMenuPanel.Controls.Add(startGamePanel);
            mainMenuPanel.Controls.Add(loginPanel);

            mainMenuPanel.Visible = false;
        }
        */

        private void setupDeckEditorPanel()
        {
            deckEditorPanel = new DeckEditorPanel();
            deckEditorPanel.Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);
            deckEditorPanel.Visible = false;
        }

        public void transitionTo(Panel p)
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

        public void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.setObserver(heroPanel);
            villain.setObserver(villainPanel);

            hero.getHand().setObserver(handPanel);

            stack.setObserver(stackPanel);

            hero.getField().setObserver(heroFieldPanel);
            villain.getField().setObserver(villainFieldPanel);
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
                GUI.loginOffline();
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

    }

    public class GamePanel : DisplayPanel
    {
        private TextBox inputBox, outputBox;
        private CardPanel handPanel;
        private PlayerPanel heroPanel, villainPanel;
        private ButtonPanel buttonPanel;
        private CardBox stackPanel;
        private FieldPanel heroFieldPanel, villainFieldPanel;
        private TurnPanel turnPanel;

        public GamePanel()
        {
            Size = new Size(GUI.FRAMEWIDTH, GUI.FRAMEHEIGHT);

            inputBox = new TextBox();
            inputBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) { return; }

                Console.WriteLine(inputBox.Text);
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

            textPanel.Controls.Add(outputBox);
            textPanel.Controls.Add(inputBox);

            handPanel = new CardPanel();
            handPanel.Location = new Point(400, 660);

            textPanel.Location = new Point(1550, 500);

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

            Controls.Add(buttonPanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            Controls.Add(textPanel);
            Controls.Add(stackPanel);
            Controls.Add(heroFieldPanel);
            Controls.Add(villainFieldPanel);

            turnPanel = new TurnPanel();
            turnPanel.Location = new Point(325, 200);

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
    }

    public class DisplayPanel : Panel
    {

    }

}