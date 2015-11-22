using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public partial class MainFrame : Form
    {
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;

        public static ManualResetEvent x = new ManualResetEvent(false);

        private static MainFrame frame;


        private static Panel mainMenuPanel;

        private static SPanel loginPanel;
        private static SPanel kappaPanel;

        private static TextBox usernameBox;


        private static Panel gamePanel;

        private static TextBox inputBox, outputBox;
        private static CardPanel handPanel;
        private static PlayerPanel heroPanel, villainPanel;
        private static ButtonPanel buttonPanel;
        private static CardBox stackPanel;
        private static FieldPanel heroFieldPanel, villainFieldPanel;
        private static TurnPanel turnPanel;


        private static FriendPanel friendPanel;


        private static Panel activePanel;
        private static Control popupPanel;
        
        public MainFrame()
        {
            InitializeComponent();

            frame = this;

            FormClosed += (sender, args) => { Environment.Exit(0); };

            Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            //FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;


            setupMainMenuPanel();
            setupGamePanel();
            
            friendPanel = new FriendPanel();
            friendPanel.Location = new Point(10, 880);


            this.Click += (sender, args) =>
            {
                System.Console.WriteLine("xd");
            };

            Controls.Add(gamePanel);
            Controls.Add(mainMenuPanel);
            Controls.Add(friendPanel);

            friendPanel.BringToFront();
            friendPanel.Hide();

            transitionTo(mainMenuPanel);

            //Network.connect();
            x.Set();
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

            heroFieldPanel = new FieldPanel();
            heroFieldPanel.Location = new Point(600, 330);

            villainFieldPanel = new FieldPanel();
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

            //gamePanel.Visible = false;
            gamePanel.Size = new Size(0, 0);
        }

        private static void setupMainMenuPanel()
        {
            mainMenuPanel = new Panel();
            mainMenuPanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            mainMenuPanel.BackColor = Color.DarkRed;

            loginPanel = new SPanel();
            loginPanel.Size = new Size(700, 400);
            loginPanel.BackColor = Color.Silver;
            loginPanel.Location = new Point((FRAMEWIDTH - 700) / 2, 200);
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
                    loginWithName(usernameBox.Text);
                }
            };
            
            Button b = new Button();
            b.Location = new Point(300, 220);
            b.Size = new Size(100, 50);
            b.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            b.Text = "Login";
            b.Click += (sender, args) =>
            {
                loginWithName(usernameBox.Text);
            };

            Button c = new Button();
            c.Location = new Point(300, 320);
            c.Size = new Size(100, 50);
            c.Font = new Font(new FontFamily("Comic Sans MS"), 20);
            c.Text = "Nyet";
            c.Click += (sender, args) =>
            {
                asd();
            };

            kappaPanel = new SPanel();
            kappaPanel.BackColor = Color.DarkKhaki;
            kappaPanel.Size = new Size(200, 200);
            kappaPanel.Location = new Point(400, 400);
            kappaPanel.Hide();

            Button d = new Button();
            d.Location = new Point(20, 20);
            d.Text = "pls";
            kappaPanel.Controls.Add(d);
            d.Click += (a,aa) =>
            {
                GameController.newGame();
            };

            loginPanel.Controls.Add(usernameLabel);
            loginPanel.Controls.Add(usernameBox);
            loginPanel.Controls.Add(b);
            loginPanel.Controls.Add(c);
            

            mainMenuPanel.Controls.Add(kappaPanel);
            mainMenuPanel.Controls.Add(loginPanel);

            //mainMenuPanel.Visible = false;
            mainMenuPanel.Size = new Size(0, 0);
        }

        public static void login()
        {
            if (Settings.username == null)
            {
                loginPanel.Show();
                waitForLogin.WaitOne();
            }
            else
            {
                Thread.Sleep(100);
                loginWithName(Settings.username);
            }

            loginPanel.Hide();
            kappaPanel.Show();
        }

        private static void asd()
        {
            waitForLogin.Set();
        }

        private static AutoResetEvent waitForLogin = new AutoResetEvent(false);

        private static void loginWithName(string x)
        {
            if (!Network.login(x))
            {
                System.Console.WriteLine("soeiroj");
                return;
            }
            System.Console.WriteLine(x);
            var s = Network.getFriends();

            friendPanel.addFriends(s);

            loginPanel.Hide();
            friendPanel.Show();
            waitForLogin.Set();
        }

        public static void showPopupPanel(Control p)
        {
            if (popupPanel != null) { closePopupPanel(); }

            popupPanel = p;
            popupPanel.Visible = true;
            //System.Console.WriteLine((MousePosition.X - frame.Location.X) + " " + MousePosition.Y);
            popupPanel.Location = new Point((MousePosition.X - frame.Location.X - p.Size.Width/2), (MousePosition.Y - frame.Location.Y - p.Size.Height));
            frame.Controls.Add(popupPanel);
            popupPanel.BringToFront();



            popupPanel.MouseLeave += (sender, args) =>
            {
                closePopupPanel();
            };
        }

        public static void closePopupPanel()
        {
            frame.Controls.Remove(popupPanel);
            popupPanel.Visible = false;
            popupPanel = null;
        }

        private static void transitionTo(Panel p)
        {
            if (frame.InvokeRequired)
            {
                frame.Invoke(new Action(() =>
                {
                    if (activePanel != null) { activePanel.Size = new Size(0, 0); }
                    activePanel = p;
                    p.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
                }));
            }
            else
            {
                if (activePanel != null) { activePanel.Size = new Size(0, 0); }
                activePanel = p;
                p.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            }
        }

        public static void transitionToGame()
        {
            transitionTo(gamePanel);
        }

        public static void transitionToMainMenu()
        {
            transitionTo(mainMenuPanel);
        }

        public static void advanceStep()
        {
            turnPanel.advanceStep();
        }

        public static void setMessage(string s)
        {
            buttonPanel.setText(s);
        }

        public static void clear()
        {
            setMessage("");
            showButtons(ButtonPanel.NOTHING);
        }

        public static void showButtons(int i)
        {
            buttonPanel.showButtons(i);
        }

        public static void showAddMana()
        {
            heroPanel.showAddMana();
        }

        public static void showWindow(Panel p)
        {
            if (frame.InvokeRequired)
            {
                frame.Invoke(new Action(() =>
                {
                    try
                    {
                        var v = new WindowedPanel(p);
                        frame.Controls.Add(v);
                        v.BringToFront();
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }
                }));
            }
            else
            {
                var v = new WindowedPanel(p);
                frame.Controls.Add(v);
                v.BringToFront();
            }
        }

        public static void getTell(string user, string message)
        {
            friendPanel.getWhisper(user, message);
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

    class SPanel : Panel
    {
        private Size xd;

        public new void Hide()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    xd = Size;
                    Size = new Size(0, 0);
                }));
            }
            else
            {
                xd = Size;
                Size = new Size(0, 0);
            }
        }

        public new void Show()
        {
            Invoke(new Action(() =>
            {
                Size = xd;
            }));
        }
    }

    class WindowedPanel : Panel
    {
        private bool dragging, drawContent = true;
        private Size xd;

        public WindowedPanel(Panel content)
        {
            Size = content.Size + new Size(0, 20);
            content.Location = new Point(0, 20);

            Panel bar = new Panel();
            bar.Size = new Size(Size.Width - 20, 20);
            bar.Location = new Point(0, 0);
            bar.BackColor = Color.DarkOrchid;

            Button x = new Button();
            x.Size = new Size(20, 20);
            x.Location = new Point(Size.Width - 20, 0);
            x.BackColor = Color.Red;

            Button t = new Button();
            t.Size = new Size(20, 20);
            t.Location = new Point(Size.Width - 40, 0);
            t.BackColor = Color.Orange;

            x.Click += (a, b) =>
            {
                Parent.Controls.Remove(this);
                content.Dispose();
            };

            t.Click += (a, b) =>
            {
                drawContent = !drawContent;

                if (drawContent)
                {
                    Size = content.Size + new Size(0, 20);
                }
                else
                {
                    Size = new Size(Size.Width, 20);
                }
            };

            bar.MouseDown += (sender, args) =>
            {
                dragging = true;
                xd = new Size(args.X, args.Y);
            };

            bar.MouseUp += (asd, sdf) =>
            {
                dragging = false;
            };

            bar.MouseMove += (sender, args) =>
            {
                if (!dragging) { return; }
                Location = Location - xd + new Size(args.X, args.Y);
            };

            Controls.Add(x);
            Controls.Add(t);
            Controls.Add(bar);
            Controls.Add(content);
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
