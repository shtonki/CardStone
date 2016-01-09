using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


//todo this class shouldn't be a thing
namespace stonekart
{
    class ButtonPanel : Panel
    {
        private ChoiceButton
            cancel,
            accept;

        private static Size
            SHOW = new Size(90, 30);

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

    class ChoiceButton : Button, GameElement
    {
        private int type;

        public ChoiceButton(int i)
        {
            type = i;
        }

        public int getType()
        {
            return type;
        }
    }
}