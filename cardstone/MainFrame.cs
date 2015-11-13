using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public partial class MainFrame : Form
    {
        public const int FRAMEWIDTH = 1800, FRAMEHEIGHT = 1000;

        private static TextBox inputBox, outputBox;
        private static HandPanel handPanel;
        private static PlayerPanel heroPanel;
        private static ButtonPanel buttonPanel;
        private static CardBox stackPanel;

        public static bool ready;

        public MainFrame()
        {
            InitializeComponent();

            FormClosed += (sender, args) => { Environment.Exit(0); };

            Size = new Size(FRAMEWIDTH, FRAMEHEIGHT);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            handPanel = new HandPanel();

            inputBox = new TextBox();
            inputBox.KeyDown += (sender, args) =>
            {
                if (args.KeyCode != Keys.Enter) { return; }

                handleCommand(inputBox.Text);
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

            Button c = new CardButton();
            Button d = new CardButton();

            textPanel.Controls.Add(outputBox);
            textPanel.Controls.Add(inputBox);

            handPanel.Location = new Point(400, 640);
            textPanel.Location = new Point(1550, 500);

            buttonPanel = new ButtonPanel();
            buttonPanel.Location = new Point(20, 300);

            heroPanel = new PlayerPanel();
            heroPanel.Location = new Point(20, 525);

            stackPanel = new CardBox(190, 500);
            stackPanel.Location = new Point(400, 20);

            Controls.Add(buttonPanel);
            Controls.Add(heroPanel);
            Controls.Add(handPanel);
            Controls.Add(textPanel);
            Controls.Add(stackPanel);



            ready = true;
        }


        public static void showButtons(int i)
        {
            buttonPanel.showButtons(i);
        }

        public static void showAddMana()
        {
            heroPanel.showAddMana();
        }

        public static void setObservers(Player hero, Player villain, Pile stack)
        {
            hero.setObserver(heroPanel);
            hero.getHand().setObserver(handPanel);
            stack.setObserver(stackPanel);
        }

        public static void handleCommand(string s)
        {
            Console.writeLine("] " + s);
            GameController.handleCommand(s);
        }

        public static void printLine(string sgfs)
        {
            if (outputBox == null) { return; }
            outputBox.AppendText(sgfs + Environment.NewLine);
        }
    }

    public static class Console
    {

        public static void writeLine(object s)
        {
            MainFrame.printLine(s.ToString());
        }
    }
}
