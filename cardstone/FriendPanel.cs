using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace stonekart
{
    class FriendPanel : Panel
    {
        public List<string> friendList { get; set; }

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
                MainFrame.showPopupPanel(p);
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

            friendCount.Text = friendList.Count.ToString();
        }
    }
}
