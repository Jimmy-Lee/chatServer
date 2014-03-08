
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace chatServer
{
    public class setting
    {
        public static String serverIP = "140.112.249.97";
        public static int port = 11000;
    }

    public delegate String StringHandler(String str);

    public class chatSocket
    {
        // handler
        public Socket socket;
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;
        public StringHandler strHandler;
        public EndPoint remoteEndPoint;
        public bool active = true;

        // constructor
        public chatSocket(Socket s)
        {
            socket = s;
            stream = new NetworkStream(s);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            remoteEndPoint = socket.RemoteEndPoint;
        }

        // send receive ~50
        public chatSocket sendMessage(String line)
        {
            writer.WriteLine(line);
            writer.Flush();
            return this;
        }

        public String receiveMessage()
        {
            return reader.ReadLine();
        }

        // connection ~85
        public static chatSocket connect()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(setting.serverIP), setting.port);
            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            newSocket.Connect(ipep);
            return new chatSocket(newSocket);
        }

        public Thread newListener(StringHandler handler)
        {
            strHandler = handler;

            Thread listener = new Thread(new ThreadStart(listen));
            listener.Start();
            return listener;
        }

        public void listen()
        {
            try
            {
                while (true)
                {
                    String line = receiveMessage();
                    strHandler(line);
                }
            }
            catch (Exception e)
            {
                active = false;
                Console.WriteLine(e.Message);
            }
        }
    }
}
