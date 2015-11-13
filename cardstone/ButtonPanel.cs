using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class ButtonPanel : Panel
    {
        private FooButton
            cancel,
            accept;

        public const int
            NOTHING = 0,
            CANCEL = 1,
            ACCEPT = 2;

        private static Size 
            HIDE = new Size(0, 0),
            SHOW = new Size(90, 30);

        public ButtonPanel()
        {
            BackColor = Color.CornflowerBlue;
            Size = new Size(300, 200);

            accept = new FooButton(ACCEPT);
            accept.Size = HIDE;
            accept.BackColor = Color.GhostWhite;
            accept.Text = "Accept";
            accept.Font = new Font(new FontFamily("Comic Sans MS"), 12);
            accept.Location = new Point(40, 100);
            accept.Click += (sender, args) =>
            {
                buttonPressed(accept);
            };

            cancel = new FooButton(CANCEL);
            cancel.Size = HIDE;
            cancel.BackColor = Color.GhostWhite;
            cancel.Text = "Cancel";
            cancel.Font = new Font(new FontFamily("Comic Sans MS"), 12);
            cancel.Location = new Point(140, 100);
            cancel.Click += (sender, args) =>
            {
                buttonPressed(cancel);
            };

            Controls.Add(accept);
            Controls.Add(cancel);
        }

        public void showButtons(int i)
        {
            accept.Size = (i & ACCEPT) != 0 ? SHOW : HIDE;
            cancel.Size = (i & CANCEL) != 0 ? SHOW : HIDE;
        }

        private void buttonPressed(FooButton b)
        {
            GameController.fooPressed(b);
        }
    }

    class FooButton : Button, Foo
    {
        private int type;

        public FooButton(int i)
        {
            type = i;
        }

        public int getType()
        {
            return type;
        }
    }
}
