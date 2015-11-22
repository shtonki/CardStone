using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class FriendPanel : SPanel
    {
        public List<string> friendList { get; set; }
        private static Dictionary<string, ConversationPanel> conv = new Dictionary<string, ConversationPanel>(); 

        private Label friendCount;
        private Button addFriendButton;

        public FriendPanel()
        {
            friendList = new List<string>();
            Size = new Size(250, 80);
            BackColor = Color.Silver;

            friendCount = new Label();
            friendCount.Size = new Size(70, 70);
            friendCount.Location = new Point(5, 5);
            friendCount.Text = "0";
            friendCount.Font = new Font(new FontFamily("Comic Sans MS"), 40);
            friendCount.Click += (sender, args) =>
            {
                foreach (var v in friendList)
                {
                    System.Console.WriteLine(v);
                }
            };
            friendCount.Click += (sender, args) =>
            {
                Label p = new Label();
                p.Size = new Size(150, 250);
                p.BackColor = Color.DarkGreen;
                String s = "";
                foreach (string x in friendList)
                {
                    s += x + '\n';
                }
                p.Font = new Font(new FontFamily("Comic Sans MS"), 20);
                p.Text = s;
            };

            addFriendButton = new Button();
            addFriendButton.Size = new Size(70, 70);
            addFriendButton.Location = new Point(75, 5);
            addFriendButton.Font = new Font(new FontFamily("Comic Sans MS"), 40);
            addFriendButton.Text = "+";
            addFriendButton.Click += (sender, args) =>
            {
                Panel p = new Panel();
                p.Size = new Size(150, 250);
                p.BackColor = Color.DarkGreen;
                MainFrame.showPopupPanel(p);
            };
            
            Controls.Add(friendCount);
            Controls.Add(addFriendButton);
        }

        public void addFriend(string s)
        {
            friendList.Add(s);
        }

        public void addFriends(string[] s)
        {
            foreach (var v in s)
            {
                addFriend(v);
            }

            friendCount.Invoke(new Action(() =>
            {
                friendCount.Text = friendList.Count.ToString();
            }));
        }

        public void getWhisper(string from, string message)
        {
            if (conv.ContainsKey(from))
            {
                conv[from].appendMessage(from + ": " + message + "\n");
                return;
            }

            var p = new ConversationPanel(from);
            openConversation(from, p);
            MainFrame.showWindow(p);

            p.appendMessage(from + ": " + message + "\n");
        }

        public static void closeConversation(string s)
        {
            conv.Remove(s);
        }

        public static void openConversation(string s, ConversationPanel p)
        {
            if (conv.ContainsKey(s)) { throw new Exception("no buendo"); }
            conv.Add(s, p);
        }

        public static void sendMessage(ConversationPanel conversation, string message)
        {
            conversation.appendMessage("you: " + message + "\n");
        }

        internal class ConversationPanel : Panel
        {
            public string partner { get; private set; }

            private RichTextBox l;

            public ConversationPanel(string p)
            {
                partner = p;

                Size = new Size(200, 233);

                l = new RichTextBox();
                l.Multiline = true;
                l.WordWrap = true;
                l.Size = new Size(200, 200);
                l.ReadOnly = true;


                l.Font = new Font(new FontFamily("Comic Sans MS"), 14);

                var i = new TextBox();
                i.Size = new Size(200, 50);
                i.Location = new Point(0, 200);
                i.Font = new Font(new FontFamily("Comic Sans MS"), 14);
                i.KeyDown += (sender, args) =>
                {
                    if (args.KeyCode != Keys.Enter) { return; }

                    sendMessage(this, i.Text);
                    i.Clear();
                };

                Controls.Add(l);
                Controls.Add(i);

                Disposed += (sender, args) =>
                {
                    closeConversation(partner);
                };
            }

            public void appendMessage(string s)
            {
                Invoke(new Action(() =>
                {
                    l.AppendText(s);
                }));
            }
        }
    }
}
