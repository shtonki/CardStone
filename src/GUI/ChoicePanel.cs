using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{

    class ChoicePanel : Panel
    {
        private GameInterface gameInterface;

        private ChoiceButton
            cancel,
            accept,
            pass;

        private Label textLabel;

        public override string Text
        {
            get { return textLabel.Text; }
            set { setText(value); }
        }

        public ChoicePanel(GameInterface g)
        {
            gameInterface = g;

            BackColor = Color.CornflowerBlue;
            //Size = new Size(300, 140);

            textLabel = new Label();
            textLabel.Size = new Size(280, 40);
            textLabel.Location = new Point(10, 10);
            textLabel.Font = new Font(new FontFamily("Comic Sans MS"), 14);

            accept = new ChoiceButton(Choice.ACCEPT);
            accept.Visible = false;
            accept.BackColor = Color.GhostWhite;
            accept.Text = "Accept";
            accept.Font = new Font(new FontFamily("Comic Sans MS"), 12);
            accept.Location = new Point(40, 100);
            accept.Click += (sender, args) =>
            {
                buttonPressed(accept);
            };

            cancel = new ChoiceButton(Choice.CANCEL);
            cancel.Visible = false;
            cancel.BackColor = Color.GhostWhite;
            cancel.Text = "Cancel";
            cancel.Font = new Font(new FontFamily("Comic Sans MS"), 12);
            cancel.Location = new Point(140, 100);
            cancel.Click += (sender, args) =>
            {
                buttonPressed(cancel);
            };

            pass = new ChoiceButton(Choice.PASS);
            pass.Visible = false;
            pass.BackColor = Color.GhostWhite;
            pass.Text = "Pass";
            pass.Font = new Font(new FontFamily("Comic Sans MS"), 12);
            pass.Location = new Point(140, 100);
            pass.Click += (sender, args) =>
            {
                buttonPressed(pass);
            };

            Controls.Add(textLabel);
            Controls.Add(accept);
            Controls.Add(cancel);
            Controls.Add(pass);


        }

        private void setText(string s)
        {
            if (s == null)
            {
                return;
            }
            if (InvokeRequired)
            {
                Invoke(new Action(() => { textLabel.Text = s; }));
            }
            else
            {
                textLabel.Text = s;
            }

        }

        public void showButtons(uint i)
        {
            setVisibleSafe(accept, (i & (int)Choice.ACCEPT) != 0);
            setVisibleSafe(cancel, (i & (int)Choice.CANCEL) != 0);
            setVisibleSafe(pass, (i & (int)Choice.PASS) != 0);
        }

        private static void setVisibleSafe(Control c, bool v)
        {
            if (c.InvokeRequired)
            {
                c.Invoke(new Action(() => c.Visible = v));
            }
            else
            {
                c.Visible = v;
            }
        }

        private void buttonPressed(ChoiceButton b)
        {
            gameInterface.gameElementPressed(b.getElement());
        }
    }


    class ChoiceButton : Button, GameUIElement
    {
        private Choice choice;

        public GameElement getElement()
        {
            return new GameElement(choice);
        }

        public ChoiceButton(Choice c)
        {
            choice = c;
            Size = new Size(80, 40);
        }

    }
    

    public enum Choice
    {
        ACCEPT = 1,
        CANCEL = 2,
        PASS   = 4,
        //... = 4, 8, 16
    }
}
