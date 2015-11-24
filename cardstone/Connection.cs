using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

class Connection
{
    public delegate void DataReceived(Connection c, byte [] b);
    public delegate void ConnectionClosed(Connection c);

    private Socket socket;
    private DataReceived receivedCallback;
    private ConnectionClosed closedCallback;

    private string you, me;


    public Connection(Socket s)
    {
        socket = s;
        socket.NoDelay = true;
    }

    public Connection(string s) : this(new TcpClient(s, 6969).Client)
    {
    }

    public void setCallback(DataReceived r, ConnectionClosed c)
    {
        receivedCallback = r;
        closedCallback = c;
    }

    public void start()
    {
        Thread t = new Thread(mine);
        t.Start();
    }

    public void sendString(String s)
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

    public String waitForString()
    {
        byte[] bs = new byte[0];
        socket.Receive(bs, 0, SocketFlags.None);    
        bs = new byte[socket.Available];
        socket.Receive(bs, bs.Length, SocketFlags.None);
        return System.Text.Encoding.Default.GetString(bs);
    }

    private void mine()
    {
        byte[] bs = new byte[0];

        try
        {
            while (true)
            {
                //wait for data
                socket.Receive(bs, 0, SocketFlags.None);

                Byte[] r = new byte[socket.Available];
                socket.Receive(r, r.Length, SocketFlags.None);

                receivedCallback(this, r);
            }
        }
        catch (SocketException)
        {
            closedCallback(this);
        }
    }
}
