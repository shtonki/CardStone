using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class ResHacker : Form
    {
        private static ResHacker hacker;
        

        public static void open()
        {
            Thread t = new Thread(show);
            t.Start();
        }

        private static void show()
        {
            hacker = new ResHacker();

            Application.Run(hacker);
        }

        private const int HEIGHT = 500, WIDTH = 500;
        private ResHacker()
        {
            Size = new Size(WIDTH + 15, HEIGHT);
            BackColor = Color.LimeGreen;

            Panel f = new Panel();
            f.Size = new Size(WIDTH + 25, HEIGHT + 25);
            f.AutoScroll = true;

            Button sb = new Button();
            sb.Size = new Size(100, 25);
            sb.Text = "Save";
            sb.MouseClick += (_, __) =>
            {
                Resolution.save();
            };
            f.Controls.Add(sb);

            for (int i = 0; i < Enum.GetNames(typeof (ElementDimensions)).Count(); i++)
            {

                Panel p = new Panel();
                p.Size = new Size(WIDTH, 25);
                p.Location = new Point(0, i*25 + 25);
                p.BackColor = i%2 == 0 ? Color.Olive : Color.NavajoWhite;
                
                TextBox av = new TextBox();
                av.Size = new Size(40, 25);
                av.Location = new Point(WIDTH/2 - 65, 3);
                av.Text = Resolution.get((ElementDimensions)i).ToString();


                Label l = new Label();
                l.Size = new Size(WIDTH/2 - 20, 20);
                l.Text = ((ElementDimensions)i).ToString();

                TextBox m = new TextBox();
                m.Size = new Size(25, 25);
                m.Location = new Point(WIDTH - 25, 0);
                m.Text = "1";

                TrackBar r = new TrackBar();
                r.Size = new Size(WIDTH/2, 20);
                r.Location = new Point(WIDTH/2 - 25, 0);
                r.Maximum = 19;
                r.TickFrequency = r.Size.Width/42;
                int i1 = i;
                int v = 10;
                r.Value = v;
                r.ValueChanged += delegate
                {
                    int x = r.Value - v;
                    v = r.Value;
                    int mp;
                    if (x == 0 || !Int32.TryParse(m.Text, out mp))
                    {
                        return;
                    }
                    Resolution.setRelative((ElementDimensions)i1, x*mp);
                    GUI.updateAll();
                    av.Text = Resolution.get((ElementDimensions)i1).ToString();
                };
                r.MouseUp += delegate
                {
                    v = 10;
                    r.Value = v;
                    f.Focus();
                };

                av.KeyDown += delegate(object _, KeyEventArgs __)
                {
                    if (__.KeyCode == Keys.Enter)
                    {
                        int mp;
                        if (!Int32.TryParse(av.Text, out mp))
                        {
                            return;
                        }
                        {
                            Resolution.set((ElementDimensions)i1, mp);
                            GUI.updateAll();
                        }
                    }
                };

                p.Controls.Add(av);
                p.Controls.Add(l);
                p.Controls.Add(r);
                p.Controls.Add(m);

                f.Controls.Add(p);

            }

            Controls.Add(f);
            f.Focus();
            Size = f.Size;
        }
    }
}
