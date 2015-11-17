using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public partial class MainFrame : Form
    {
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;


        private static Panel loginPanel;

        private static TextBox usernameBox;


        private static Panel gamePanel;

        private static TextBox inputBox, outputBox;
        private static CardPanel handPanel;
        private static PlayerPanel heroPanel;
        private static ButtonPanel buttonPanel;
        private static CardBox stackPanel;
        private static FieldPanel heroFieldPanel, villainFieldPanel;

        
        public MainFrame()
        {
            InitializeComponent();

            FormClosed += (sender, args) => { Environment.Exit(0); };

            Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;


            setupMainMenuPanel();
            setupGamePanel();
            

            Controls.Add(gamePanel);
            Controls.Add(loginPanel);
        }

        private static void setupGamePanel()
        {
            gamePanel = new Panel();
            gamePanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);

            inputBox = new TextBox();
            inputBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) { return; }

                handleCommand(inputBox.Text);
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

            Button c = new CardButton();
            Button d = new CardButton();

            textPanel.Controls.Add(outputBox);
            textPanel.Controls.Add(inputBox);

            handPanel = new CardPanel();
            handPanel.Location = new Point(400, 750);

            textPanel.Location = new Point(1550, 500);

            buttonPanel = new ButtonPanel();
            buttonPanel.Location = new Point(20, 300);

            heroPanel = new PlayerPanel();
            heroPanel.Location = new Point(20, 525);

            stackPanel = new CardBox(190, 500);
            stackPanel.Location = new Point(400, 20);

            heroFieldPanel = new FieldPanel();
            heroFieldPanel.Location = new Point(600, 330);

            villainFieldPanel = new FieldPanel();
            villainFieldPanel.Location = new Point(600, 10);

<<<<<<< Updated upstream
            Controls.Add(buttonPanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            Controls.Add(textPanel);
            Controls.Add(stackPanel);
            Controls.Add(heroFieldPanel);
            Controls.Add(villainFieldPanel);
=======
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

            gamePanel.Visible = false;
        }


        private static void setupMainMenuPanel()
        {
            loginPanel = new Panel();
            loginPanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            loginPanel.BackColor = Color.DarkRed;
>>>>>>> Stashed changes


            Panel panel = new Panel();
            panel.Size = new Size(700, 400);
            panel.BackColor = Color.Silver;
            panel.Location = new Point((FRAMEWIDTH - 700)/2, 200);

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
                    login();
                }
            };
            
            Button b = new Button();
            b.Location = new Point(300, 220);
            b.Size = new Size(100, 50);
            b.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            b.Text = "Login";
            b.Click += (sender, args) =>
            {
                login();
            };

            panel.Controls.Add(usernameLabel);
            panel.Controls.Add(usernameBox);
            panel.Controls.Add(b);


            loginPanel.Controls.Add(panel);
        }

        private static void login()
        {
            if (Network.login(usernameBox.Text))
            {
                loginPanel.Visible = false;
            }
        }


        public static void showButtons(int i)
        {
            buttonPanel.showButtons(i);
        }

        public static void showAddMana()
        {
            heroPanel.showAddMana();
        }

        public static void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.setObserver(heroPanel);
            hero.getHand().setObserver(handPanel);
            stack.setObserver(stackPanel);
            hero.getField().setObserver(heroFieldPanel);
        }

        public static void handleCommand(string s)
        {
            Console.writeLine("] " + s);
        }

        public static void printLine(string sgfs)
        {
            if (outputBox == null) { return; }
            outputBox.AppendText(sgfs + Environment.NewLine);
        }
    }

    public static class Console
    {

        public static void writeLine(object s)
        {
            MainFrame.printLine(s.ToString());
        }
    }
}
