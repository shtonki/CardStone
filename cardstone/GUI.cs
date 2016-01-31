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
    public static class GUI
    {
        private static MainFrame frame;

        public static ManualResetEvent frameLoaded = new ManualResetEvent(false);

        public static void createFrame()
        {
            Action a = () =>
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                frame = new MainFrame();
                Application.Run(frame);
            };
            Thread t = new Thread(new ThreadStart(a));
            t.Start();

            frameLoaded.WaitOne();
        }   

        public static void transitionToGame()
        {
            frame.transitionTo(frame.gamePanel);
        }

        public static void transitionToMainMenu()
        {
            frame.transitionTo(frame.mainMenuPanel);
        }

        public static void transitionToDeckEditor()
        {
            frame.transitionTo(frame.deckEditorPanel);
        }

        public static void setStep(int s, bool a)
        {
            frame.setStep(s, a);
        }

        public static void setMessage(string s)
        {
            frame.setMessage(s);
        }

        public static void clear()
        {
            setMessage("");
            showButtons(ButtonPanel.NOTHING);
        }

        public static void showButtons(int i)
        {
            frame.showButtons(i);
        }

        public static void showAddMana(bool b)
        {
            frame.showAddMana(b);
        }

        public static WindowedPanel showWindow(Control p, string barTitle, bool closeable, Action closeCallback)
        {
            WindowedPanel v = null;

            Action a = () =>
            {
                v = new WindowedPanel(p, barTitle);
                frame.Controls.Add(v);
                v.BringToFront();
            };

            if (frame.InvokeRequired)
            {
                frame.Invoke(new Action(() =>
                {
                    try
                    {
                        
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.ToString());
                    }
                }));
            }
            else
            {
                v = new WindowedPanel(p, barTitle);
                frame.Controls.Add(v);
                v.BringToFront();
            }

            return v;
        }

        public static WindowedPanel showWindow(Control p)
        {
            return showWindow(p, "", false, null);
        }

        public static void showTell(string f, string m)
        {
            frame.showTell(f, m);
        }

        public static void setObservers(Player h, Player v, Pile s)
        {
            frame.setObservers(h, v, s);
        }

        public static void showLoginBox()
        {
            frame.showLoginBox();
        }
    }

    public partial class MainFrame : Form
    {
        //todo(jasin) sholdn't be const fam
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;



        #region mainmenu
        public DisplayPanel mainMenuPanel { get; private set; }

        private Panel loginPanel;
        private Panel kappaPanel;

        private TextBox usernameBox;
        #endregion

        #region gamepanel
        public DisplayPanel gamePanel { get; private set; }

        private TextBox inputBox, outputBox;
        private CardPanel handPanel;
        private PlayerPanel heroPanel, villainPanel;
        private ButtonPanel buttonPanel;
        private CardBox stackPanel;
        private FieldPanel heroFieldPanel, villainFieldPanel;
        private TurnPanel turnPanel;
        #endregion

        public DisplayPanel deckEditorPanel { get; private set; }

        private FriendPanel friendPanel;

        private Panel activePanel;
        
        public MainFrame()
        {
            //InitializeComponent();
            BackColor = Color.Black;
            Application.AddMessageFilter(new GlobalMouseHandler());

            FormClosed += (sender, args) => { Environment.Exit(0); };

            Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;


            setupMainMenuPanel();
            setupGamePanel();
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
            gamePanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);

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

        private void setupMainMenuPanel()
        {
            mainMenuPanel = new DisplayPanel();
            mainMenuPanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            mainMenuPanel.BackColor = Color.DarkRed;

            loginPanel = new Panel();
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
                Console.WriteLine("i'm not entirely sure what this button is supposed to do");
            };

            kappaPanel = new Panel();
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
                GameController.newGame(new DummyConnection());
            };

            loginPanel.Controls.Add(usernameLabel);
            loginPanel.Controls.Add(usernameBox);
            loginPanel.Controls.Add(b);
            loginPanel.Controls.Add(c);
            

            mainMenuPanel.Controls.Add(kappaPanel);
            mainMenuPanel.Controls.Add(loginPanel);

            mainMenuPanel.Visible = false;
        }

        private void setupDeckEditorPanel()
        {
            deckEditorPanel = new DeckEditorPanel();
            deckEditorPanel.Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            deckEditorPanel.Visible = false;
        }


        public void transitionTo(Panel p)
        {
            Action a = () =>
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
            };

            if (InvokeRequired)
            {
                Invoke(a);
            }
            else
            {
                a();
            }
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
        
        public void loginWithName(string x)
        {
            //todo(seba) move this to network
            if (!Network.login(x))
            {
                System.Console.WriteLine("couldn't log in");
                return;
            }

            Network.sendRaw(Network.SERVER, "friend", "");

            loginPanel.Hide();
            friendPanel.Show();
        }

        public void showLoginBox()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => { loginPanel.Visible = true; }));
            }
            else
            {
                loginPanel.Visible = true;
            }
        }
    }

    public class WindowedPanel : Panel
    {
        private bool dragging, drawContent = true, closed;
        private Size xd;
        private Control content;

        public WindowedPanel(Control c, string barTitle)
        {
            content = c;

            Size = c.Size + new Size(0, 20);
            c.Location = new Point(0, 20);

            Panel bar = new Panel();
            bar.Size = new Size(Size.Width - 20, 20);
            bar.Location = new Point(0, 0);
            bar.BackColor = Color.DarkOrchid;
            bar.Text = barTitle;

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
                close();
            };

            t.Click += (a, b) =>
            {
                drawContent = !drawContent;

                if (drawContent)
                {
                    Size = c.Size + new Size(0, 20);
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
            Controls.Add(c);
        }

        public void close()
        {
            closed = true;
            Parent.Controls.Remove(this);
        }

        public Control getContent()
        {
            return content;
        }

        public bool isClosed()
        {
            return closed;
        }
    }

    class GlobalMouseHandler : IMessageFilter
    {

        private const int left = 0x201, right = 0x205;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == left || m.Msg == right)
            {
                
            }
            return false;
        }
    }

    public class DisplayPanel : Panel
    {
        
    }
    
}
