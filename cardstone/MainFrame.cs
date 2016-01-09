using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    

    public partial class MainFrame : Form
    {
        //todo(jasin) sholdn't be const fam
        
        public MainMenuPanel mainMenuPanel { get; private set; }
        
        public GamePanel gamePanel { get; private set; }
        
        public DisplayPanel deckEditorPanel { get; private set; }

        public FriendPanel friendPanel { get; private set; }

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

    }

}