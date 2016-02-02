using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{
    public class MainMenuPanel : DisplayPanel
    {
        public SPanel loginPanel { get; private set; }
        public SPanel startGamePanel { get; private set; }

        private TextBox usernameBox;

        public MainMenuPanel()
        {
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

            Button deckEditorButton = new Button();
            deckEditorButton.Location = new Point(20, 60);
            deckEditorButton.Text = "Deck Editor";
            startGamePanel.Controls.Add(deckEditorButton);
            deckEditorButton.Click += (a, aa) =>
            {
                GUI.transitionToDeckEditor();
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
}