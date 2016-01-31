using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    public class FriendPanel : SPanel
    {
        public List<string> friendList { get; set; }
        private static Dictionary<string, WindowedPanel> conv = new Dictionary<string, WindowedPanel>();
        private static WindowedPanel friendListWindow;
        private static int friendListOffset;

        private Label friendCount;
        private Panel friendListPanel;
        private Button[] friendListButtons;
        private Label[] friendListLabels;
        private TextBox challengeName;

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

            friendListPanel = new Panel();
            friendListPanel.Size = new Size(150, 240);
            friendListPanel.BackColor = Color.DarkGreen;
            friendListButtons = new Button[6];
            friendListLabels = new Label[6];
            for (int i = 0; i < 6; i++)
            {
                Button b = new Button();
                Label l = new Label();
                //
                l.Size = new Size(120, 40);
                l.Location = new Point(0, i * 40);
                l.Text = i.ToString();
                friendListLabels[i] = l;
                l.Click += (_, __) =>
                {
                    getConversation(l.Text);
                };

                b.Size = new Size(30, 30);
                b.Location = new Point(115, 5 + i * 40);
                friendListButtons[i] = b;
                var i1 = i;
                b.Click += (_, __) =>
                {
                    removeFriend(l.Text);
                };

                friendListPanel.Controls.Add(b);
                friendListPanel.Controls.Add(l);

            }
            friendListPanel.Font = new Font(new FontFamily("Comic Sans MS"), 20);


            friendCount.Click += (sender, args) =>
            {
                if (friendListWindow == null || friendListWindow.isClosed()) { updateFriendList(); friendListWindow = GUI.showWindow(friendListPanel); }
            };

            var addFriendButton = new Button();
            addFriendButton.Size = new Size(70, 70);
            addFriendButton.Location = new Point(75, 5);
            addFriendButton.Font = new Font(new FontFamily("Comic Sans MS"), 40);
            addFriendButton.Text = "+";
            addFriendButton.Click += (sender, args) =>
            {
                Panel p = new Panel();

                TextBox x = new TextBox();
                x.Font = FontLoader.getFont(FontLoader.MPLATIN, 16);
                x.KeyDown += (xx, xd) =>
                {
                    if (xd.KeyCode != Keys.Enter) { return; }

                    addFriend(x.Text);
                };
                p.Size = x.Size;
                x.BackColor = Color.Red;

                p.Controls.Add(x);

                GUI.showWindow(p);
            };

            challengeName = new TextBox();
            challengeName.Location = new Point(150, 10);
            challengeName.Size = new Size(80, 40);
            challengeName.Visible = true;
            challengeName.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    string s = challengeName.Text;
                    challengeName.Text = "";
                    Network.challenge(s);
                }
            };


            Controls.Add(challengeName);
            Controls.Add(friendCount);
            Controls.Add(addFriendButton);
        }

        private void removeFriend(string text)
        {
            friendList.Remove(text);
            Network.removeFriend(text);
            Invalidate();
        }

        public void addFriend(string s)
        {
            friendList.Add(s);
            Network.addFriend(s);
            Invalidate();
        }

        public void setFriends(string[] s)
        {
            foreach (var v in s)
            {
                friendList.Add(v);
            }

            friendCount.Invoke(new Action(() =>
            {
                friendCount.Text = friendList.Count.ToString();
            }));
        }

        public void getWhisper(string from, string message)
        {
            getConversation(from).appendLine(from + ": " + message);

        }

        public static ConversationPanel getConversation(string user)
        {
            ConversationPanel p = null;

            if (conv.ContainsKey(user) && !conv[user].isClosed())
            {
                p = (ConversationPanel)conv[user].getContent();
            }
            else
            {
                conv.Remove(user);
                p = new ConversationPanel(user);
                conv.Add(user, GUI.showWindow(p));
            }

            return p;
        }

        public static void closeConversation(string s)
        {
            conv.Remove(s);
        }

        public static void sendMessage(ConversationPanel conversation, string message)
        {
            //todo(jasin) make this a buttonx
            if (message == "/challenge")
            {
                Network.challenge(conversation.partner);
                return;
            }
            Network.sendTell(conversation.partner, message);
            conversation.appendLine("you: " + message);
        }

        private void updateFriendList()
        {
            Action a = () =>
            {
                int i = friendListOffset;
                for (; i < friendListOffset + 6 && i < friendList.Count; i++)
                {
                    friendListLabels[i].Visible = true;
                    friendListLabels[i].Text = friendList[i];
                    friendListButtons[i].Visible = true;
                }

                for (; i < friendListOffset + 6; i++)
                {
                    friendListLabels[i].Visible = false;
                    friendListButtons[i].Visible = false;
                }
            };

            if (InvokeRequired)
            {
                Invoke(a);
            }
            else
            {
                a();
            }
        }

        public class ConversationPanel : Panel
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
                l.TextChanged += (xd, xdd) =>
                {
                    l.SelectionStart = l.Text.Length;
                    l.ScrollToCaret();
                };
                l.Font = FontLoader.getFont(FontLoader.MAIANDRA, 16);

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

            public void appendLine(string s)
            {
                Invoke(new Action(() =>
                {
                    l.AppendText('\n' + s);
                }));
            }

            public void forceOpen()
            {
                
            }
        }
    }
}
