using System;
using System.Drawing;
using System.Windows.Forms;

namespace stonekart
{

    class ChoicePanel : Panel
    {
        private GameInterface gameInterface;

        private ChoiceButton[] buttons = new ChoiceButton[BUTTONS];

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

            for (int i = 0; i < buttons.Length; i++)
            {
                ChoiceButton b = new ChoiceButton();
                b.BackColor = Color.GhostWhite;
                b.Font = new Font(new FontFamily("Comic Sans MS"), 12);
                b.Location = new Point(5 + (i % 3) * 80, 100 + 50*(i/3));
                b.Click += (sender, args) =>
                {
                    buttonPressed(b);
                };
                buttons[i] = b;
                Controls.Add(b);
            }

            Controls.Add(textLabel);


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

        public void showButtons(Choice[] cs)
        {
            int i = 0;
            for (; i < cs.Length; i++)
            {
                if (cs[i] == Choice.PADDING) { continue; }
                ChoiceButton b = buttons[i];
                b.choice = cs[i];
                safeSetText(b, cs[i].ToString());
                setVisibleSafe(buttons[i], true);
            }
            for (; i < BUTTONS; i++)
            {
                setVisibleSafe(buttons[i], false);
            }
        }

        private static void safeSetText(Button b, string s)
        {
            if (b.InvokeRequired)
            {
                b.Invoke(new Action(() => b.Text = s));
            }
            else
            {
                b.Text = s;
            }
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

        private const int BUTTONS = 6;
    }


    class ChoiceButton : Button, GameUIElement
    {
        public Choice choice { get; set; }

        public GameElement getElement()
        {
            return new GameElement(choice);
        }

        public ChoiceButton()
        {
            Size = new Size(80, 40);
        }

    }
    

    public enum Choice
    {
        PADDING,
        Accept,
        Cancel,
        Pass,
        Yes,
        No,
    }
}
