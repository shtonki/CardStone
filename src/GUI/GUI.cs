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

        public static void transitionToGame(GameInterface g)
        {
            frame.transitionTo(g.gamePanel);
        }

        public static void transitionToMainMenu()
        {
            frame.transitionTo(frame.mainMenuPanel);
        }

        public static void transitionToDeckEditor()
        {
            frame.transitionTo(frame.deckEditorPanel);
        }

        /// <summary>
        /// Creates a GameInterface and binds it to a new GamePanel created
        /// in the current frame.
        /// </summary>
        public static GameInterface createGameUI()
        {
            GameInterface r = new GameInterface();
            frame.createGamePanel(r);
            return r;
        }

        /*
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

        public static WindowedPanel showWindow(Panel p)
        {
            return showWindow(p, "", false, null);
        }
        */
        

        public static WindowedPanel showWindow(Panel p)
        {
            WindowedPanel w =  frame.Invoke(new Func<WindowedPanel>(() =>
            {
                var v = _showWindow(p);
                frame.Controls.Add(v);
                v.BringToFront();
                return v;
            })) as WindowedPanel;
            return w;
        }

        private static WindowedPanel _showWindow(Panel p)
        {
            WindowedPanelArgs a = new WindowedPanelArgs("", true, true, false);   
            return _showWindow(p, a);
        }

        private static WindowedPanel _showWindow(Panel p, WindowedPanelArgs a)
        {
            WindowedPanel w = new WindowedPanel(p, a);
            frame.Controls.Add(w);
            return w;
        }

        public static void showTell(string f, string m)
        {
            frame.showTell(f, m);
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
                //Application.Exit();
                frame.handleGlobalKeyDown(Keys.Escape);
            }
        }
        
        
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;
        

        public static IEnumerable<Control> getAll()
        {
            return getAll(frame);
        } 

        private static IEnumerable<Control> getAll(Control control)
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => getAll(ctrl))
                                      .Concat(controls);
        }
    }
    

    public struct WindowedPanelArgs
    {
        public readonly string title;
        public readonly bool closeable;
        public readonly bool minimizable;
        public readonly bool resizable;

        public WindowedPanelArgs(string title, bool closeable, bool minimizable, bool resizable)
        {
            this.title = title;
            this.closeable = closeable;
            this.minimizable = minimizable;
            this.resizable = resizable;
        }
    }

    public class WindowedPanel : Panel
    {
        private bool dragging, drawContent = true, closed;
        private Size xd;
        private Control content;

        public WindowedPanel(Control c, WindowedPanelArgs args)
        {
            content = c;

            Size = c.Size + new Size(0, 20);
            c.Location = new Point(0, 20);

            Label bar = new Label();
            bar.Size = new Size(Size.Width - 20, 20);
            bar.Location = new Point(0, 0);
            bar.BackColor = Color.DarkOrchid;
            bar.Text = args.title ?? "";

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

            bar.MouseDown += (sender, ags) =>
            {
                dragging = true;
                xd = new Size(ags.X, ags.Y);
            };

            bar.MouseUp += (asd, sdf) =>
            {
                dragging = false;
            };

            bar.MouseMove += (sender, ags) =>
            {
                if (!dragging) { return; }
                Location = Location - xd + new Size(ags.X, ags.Y);
            };

            Controls.Add(x);
            Controls.Add(t);
            Controls.Add(bar);
            Controls.Add(c);
        }

        public void close()
        {
            closed = true;
            if (Parent.InvokeRequired)
            {
                Parent.Invoke(new Action(() => Parent.Controls.Remove(this)));
            }
            else
            {
                Parent.Controls.Remove(this);
            }
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
