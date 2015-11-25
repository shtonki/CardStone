using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;

class Connection
{
    public delegate void DataReceived(Connection c, SMessage m);
    public delegate void ConnectionClosed(Connection c);

    private Socket socket;
    private DataReceived receivedCallback;
    private ConnectionClosed closedCallback;

    private const byte HEADER = 2, TAILER = 3, SEPARATOR = 1;

    private Queue<SMessage> messageQueue;

    AutoResetEvent xdd = new AutoResetEvent(false);

    public Connection(Socket s)
    {
        socket = s;
        socket.NoDelay = true;
        messageQueue = new Queue<SMessage>();

        Thread t = new Thread(mine);
        t.Start();
    }

    public Connection(string s) : this(new TcpClient(s, 6969).Client)
    {
    }

    private void setCallback(DataReceived r, ConnectionClosed c)
    {
        receivedCallback = r;
        closedCallback = c;
    }

    public void startAsync(DataReceived r, ConnectionClosed c)
    {
        setCallback(r, c);
    }

    public void sendMessage(SMessage m)
    {
        StringBuilder b = new StringBuilder(4 + m.from.Length + m.to.Length + m.header.Length + (m.message == null ? 0 : m.message.Length));
        b.Append((char)HEADER);
        b.Append(m.to);
        b.Append((char)SEPARATOR);
        b.Append(m.from);
        b.Append((char)SEPARATOR);
        b.Append(m.header);
        b.Append((char)SEPARATOR);

        if (m.message != null) { b.Append(m.message); }

        b.Append((char)TAILER);

        sendString(b.ToString());
    }

    private void sendString(String s)
    {
        try
        {
            byte[] bytes = new byte[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                bytes[i] = (byte)s[i];
            }

            socket.Send(bytes);
        }
        catch (System.Net.Sockets.SocketException)
        {
            closedCallback(this);
        }
    }

    public SMessage waitForMessage()
    {
        if (messageQueue.Count == 0)
        {
            xdd.WaitOne();
        }
        return messageQueue.Dequeue();
    }

    private void mine()
    {
        byte[] bs = new byte[0];

        try
        {
            while (true)
            {
                socket.Receive(bs, 0, SocketFlags.None);

                byte[] bx = new byte[socket.Available];
                socket.Receive(bx, bx.Length, SocketFlags.None);

                X x = new X(bx);

                while (true)
                {
                    x.assertNext(HEADER);
                    string to = x.toNext(SEPARATOR);
                    string from = x.toNext(SEPARATOR);
                    string header = x.toNext(SEPARATOR);
                    string message = x.atEnd() ? "" : x.toNext(TAILER);

                    handleMessage(new SMessage(to, from, header, message));

                    if (x.atEnd()) { break; }
                }

            }
        }
        catch (SocketException)
        {
            closedCallback(this);
        }
    }

    private void handleMessage(SMessage m)
    {
        if (receivedCallback == null)
        {
            messageQueue.Enqueue(m);
            xdd.Set();
        }
        else
        {
            receivedCallback(this, m);
        }
    }

    private class X
    {
        private byte[] bs;
        private int c;

        public X(byte[] b)
        {
            bs = b;
            c = 0;
        }

        public byte getNext()
        {
            return bs[c++];
        }

        public String toNext(byte c)
        {
            StringBuilder b = new StringBuilder();

            byte h;
            while ((h = getNext()) != c)
            {
                b.Append((char)h);
            }

            return b.ToString();
        }

        public void assertNext(byte c)
        {
            if (getNext() != c) { Console.WriteLine("XDDDDD"); }
        }

        public bool at(byte b)
        {
            return bs[c] == b;
        }

        public bool atEnd()
        {
            return c == bs.Length;
        }
    }
}

public class SMessage
{
    public SMessage(string t, string f, string h, string m)
    {
        from = f;
        to = t;
        header = h;
        message = m ?? "";
    }

    /*
    public SMessage(string t, string f, string h)
    {
        from = f;
        to = t;
        header = h;
        message = "";
    }*/

    public override string ToString()
    {
        return from + "->" + to + '\'' + header + ';' + message + '\'';
    }

    public string from;
    public string to;
    public string header;
    public string message;
}
