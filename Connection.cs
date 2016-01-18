using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Console = System.Console;

class Connection
{

    public const string SERVERIP = "46.239.124.155";
    public const string SERVER = "server";

    public delegate void DataReceived(Connection c, SMessage m);
    public delegate void ConnectionClosed(Connection c);

    private Socket socket;
    private DataReceived receivedCallback;
    private ConnectionClosed closedCallback;

    private const byte HEADER = 2, TAILER = 3, SEPARATOR = 1;

    private Queue<SMessage> messageQueue;

    AutoResetEvent xdd = new AutoResetEvent(false);

    /// <summary>
    /// Creates a new Connection from a socket.
    /// </summary>
    /// <param name="s"></param>
    public Connection(Socket s)
    {
        socket = s;
        socket.NoDelay = true;
        messageQueue = new Queue<SMessage>();

        Thread t = new Thread(mine);
        t.Start();
    }
    
    /// <summary>
    /// Creates a new connection to the host host which times out after to seconds
    /// </summary>
    /// <param name="host">The remote host to which to connect</param>
    /// <param name="to">The timeout for the connection</param>
    public Connection(string host, int to = 1) : this(anonymousFunction(host, to).Client)
    {
    }

    private static TcpClient anonymousFunction(string s, int to)
    {
        TcpClient r = new TcpClient();
        IAsyncResult rs = r.BeginConnect(s, 6969, null, null);

        bool success = rs.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(to));

        if (!success)
        {
            throw new Exception("Failed to connect.");
        }

        // we have connected
        return r;
    }

    private void setCallback(DataReceived r, ConnectionClosed c)
    {
        receivedCallback = r;
        closedCallback = c;
    }

    /// <summary>
    /// Starts asynchronously handling incoming data from the remote connection
    /// </summary>
    /// <param name="r">The function called when data is received</param>
    /// <param name="c">The function called when the connection is closed</param>
    public void startAsync(DataReceived r, ConnectionClosed c)
    {
        setCallback(r, c);
    }

    /// <summary>
    /// Sends a message over the connection.
    /// </summary>
    /// <param name="m">The message which is to be sent</param>
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

    /// <summary>
    /// Gets the next avaiable message in the buffer. If no messages are available it blocks until a message is available.
    /// </summary>
    /// <returns>The next available message in the buffer</returns>
    public SMessage waitForMessage()
    {
        if (messageQueue.Count == 0)
        {
            xdd.WaitOne();
        }
        return messageQueue.Dequeue();
    }
    
    /// <summary>
    /// I'm pretty sure that this does important things
    /// </summary>
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

                MessageParser x = new MessageParser(bx);

                try
                {

                    while (true)
                    {
                        x.assertNext(HEADER);
                        string to = x.toNext(SEPARATOR);
                        string from = x.toNext(SEPARATOR);
                        string header = x.toNext(SEPARATOR);
                        string message = x.atEnd() ? "" : x.toNext(TAILER);

                        handleMessage(new SMessage(to, from, header, message));

                        if (x.atEnd())
                        {
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine(x);
                    break;
                }
            }
        }
        catch (SocketException)
        {
            if (closedCallback != null)
            {
                closedCallback(this);                
            }
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

    /// <summary>
    /// I'm not even going to comment this mess. It werks.
    /// </summary>
    private class MessageParser
    {
        private byte[] bs;
        private int c;

        public MessageParser(byte[] b)
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

        public override string ToString()
        {
            return Encoding.UTF8.GetString(bs);
        }
    }
}

public class SMessage
{
    /// <summary>
    /// Creates a new SMessage
    /// </summary>
    /// <param name="t">The name of the recipient</param>
    /// <param name="f">The name of the sender</param>
    /// <param name="h">The header of the message</param>
    /// <param name="m">The content of the message</param>
    public SMessage(string t, string f, string h, string m) : this(t, f, h, Encoding.ASCII.GetBytes(m ?? ""))
    {
    }

    public SMessage(string t, string f, string h, byte[] m)
    {
        bfrom = Encoding.ASCII.GetBytes(f);
        bto = Encoding.ASCII.GetBytes(t);
        bheader = Encoding.ASCII.GetBytes(h);
        bmessage = m;
    }
    
    public override string ToString()
    {
        return from + "->" + to + '\'' + header + ';' + message + '\'';
    }
    

    public string from => ASCIIEncoding.ASCII.GetString(bfrom);
    public string to => ASCIIEncoding.ASCII.GetString(bto);
    public string header => ASCIIEncoding.ASCII.GetString(bheader);
    public string message => ASCIIEncoding.ASCII.GetString(bmessage);

    
    public readonly byte[] bfrom;
    public readonly byte[] bto;
    public readonly byte[] bheader;
    public readonly byte[] bmessage;
    
}
