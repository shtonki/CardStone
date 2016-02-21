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

        public DisplayPanel draftPanel { get; private set; }

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
            //FormBorderStyle = FormBorderStyle.FixedSingle;
            //MaximizeBox = false;


            //setupMainMenuPanel();
            mainMenuPanel = new MainMenuPanel();

            draftPanel = new DraftPanel();
            draftPanel.Size = new Size(0, 0);

            //setupGamePanel();
            //gamePanel = new GamePanel();

            setupDeckEditorPanel();
            
            friendPanel = new FriendPanel();
            friendPanel.Location = new Point(10, 880);

            Controls.Add(labelx);
            //Controls.Add(gamePanel);
            Controls.Add(mainMenuPanel);
            Controls.Add(friendPanel);
            Controls.Add(deckEditorPanel);
            Controls.Add(deckEditorPanel);
            Controls.Add(draftPanel);

            friendPanel.BringToFront();
            friendPanel.Hide();

            Size = new Size(1800, 1000);

            GUI.frameLoaded.Set();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (activePanel != null)
            {
                activePanel.Size = ClientRectangle.Size;
            }
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
            r.Size = Size;
            g.setPanel(r);
            Invoke(new Action(() => { Controls.Add(r); }));
        }

        private void xdhelper(DisplayPanel p, bool b)
        {
            p.Size = ClientRectangle.Size;
            p.Visible = b;
        }

        private void xdlambda(DisplayPanel p, bool b)
        {
            if (p.InvokeRequired)
            {
                p.Invoke(new Action(() => xdhelper(p, b)));
            }
            else
            {
                xdhelper(p, b);
            }
        }

        public void transitionTo(DisplayPanel p)
        {
            if (activePanel != null)
            {
                xdlambda(activePanel, false);
            }
            activePanel = p;
            xdlambda(p, true);

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


    public class DisplayPanel : Panel
    {
        public virtual void handleKeyPress(Keys key)
        {
            
        }
    }

}