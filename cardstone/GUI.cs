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

        private static Dictionary<Card, CardButton> killMeNow = new Dictionary<Card, CardButton>(); 

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
            frame.gamePanel.setStep(s, a);
        }

        public static void setMessage(string s)
        {
            frame.gamePanel.setMessage(s);
        }

        public static void clear()
        {
            setMessage("");
            showButtons(NOTHING);
        }

        public static void showButtons(int i)
        {
            frame.gamePanel.showButtons(i);
        }

        public static void showAddMana(bool b)
        {
            frame.gamePanel.showAddMana(b);
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
                        Console.WriteLine(e.ToString());
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

        public static void setCardButton(Card c, CardButton b)
        {
            killMeNow[c] = b;
        }

        public static CardButton getCardButtonByCard(Card c)
        {
            if (!killMeNow.ContainsKey(c)) { return null; }
            return killMeNow[c];
        }

        private static WaitFor<string> logInAs = new WaitFor<string>(); 

        public static bool login()
        {
            if (Settings.username != null)
            {
                if (Network.attemptLogin(Settings.username))
                {
                    frame.friendPanel.setVisible(true);
                    return true;
                }
            }

            string s;

            while (true)
            {
                frame.mainMenuPanel.loginPanel.setVisible(true);

                s = logInAs.wait();

                if (s == null)
                {
                    frame.mainMenuPanel.loginPanel.setVisible(false);
                    return false;
                }

                if (Network.attemptLogin(s))
                {
                    frame.mainMenuPanel.loginPanel.setVisible(false);
                    frame.friendPanel.setVisible(true);
                    return true;
                }
            }
        }

        public static void loginWithName(string x)
        {
            logInAs.signal(x);
        }
        

        public static void showPlayPanel()
        {
            frame.mainMenuPanel.startGamePanel.setVisible(true);
        }
        
        public static void globalEscape()
        {
            if (!frame.focusCleared())
            {
                frame.clearFocus();
            }
            else
            {
                frame.handleGlobalKeyDown(Keys.Escape);
            }
        }
        

        public const int
            NOTHING = 0,
            CANCEL = 1,
            ACCEPT = 2;

        //todo(jasin) sholdn't be const fam
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;
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

        private const int left = 0x201, right = 0x205, keydown = 0x0100;

        private const int escape = 0x1B;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg == keydown)
            {
                int v = m.WParam.ToInt32();
                //Console.WriteLine(v);
                if (v == escape)
                {
                    GUI.globalEscape();
                    return true;
                }
            }

            else if (m.Msg == left || m.Msg == right)
            {
                
            }
            return false;
        }
    }



    //todo(seba) overhaul so that everything uses SPanel
    public class SPanel : Panel
    {
        public void setVisible(bool b)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Visible = b;
                }));
            }
            else
            {
                Visible = b;
            }
        }
    }
}
