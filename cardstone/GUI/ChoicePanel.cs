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
            accept;

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
            Size = new Size(300, 140);

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

            Controls.Add(textLabel);
            Controls.Add(accept);
            Controls.Add(cancel);


        }

        private void setText(string s)
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
                accept.Visible = (i & (int)Choice.ACCEPT) != 0;
                cancel.Visible = (i & (int)Choice.CANCEL) != 0;
            }));

        }

        private void buttonPressed(ChoiceButton b)
        {
            gameInterface.gameElementPressed(b);
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
        //... = 4, 8, 16
    }
}
