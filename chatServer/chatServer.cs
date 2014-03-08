
using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace chatServer
{
    class chatServer
    {
        List<chatSocket> clientList = new List<chatSocket>();
        List<String> clientIDList = new List<string>();
        
        public static void Main(String[] args)
        {
            chatServer server = new chatServer();
            server.run();
        }

        public void run()
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, setting.port);
            Socket newSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newSocket.Bind(ipep);
            newSocket.Listen(100);

            while (true)
            {
                Socket socket = newSocket.Accept();
                Console.WriteLine("-- WAITING FOR CONNECTIONS --\n");
                chatSocket client = new chatSocket(socket);

                try
                {
                    clientList.Add(client);
                    client.newListener(processMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public String processMessage(String msg)
        {
            Console.WriteLine("RECEIVED：" + msg);
            if (msg.IndexOf("Welcome: ") == 0)
            {
                String ID = msg.Substring(9);
                if (clientIDList.Contains(ID))
                {
                    clientList.Remove(clientList.Last());
                    return "";
                }
                else
                {
                    clientIDList.Add(ID);
                    broadCast(msg);
                    String command = "IDListUpdate:";
                    for (int i = 0, L = clientIDList.Count; i < L; ++i)
                    {
                        if (i == 0)
                            command += ' ' + clientIDList[i];
                        else
                            command += ':' + clientIDList[i];
                    }
                    broadCast(command);
                    return "";
                }
            }
            String nameAndTime = msg.Substring(0, msg.IndexOf(':') - 1) + " ─ " + DateTime.Now.ToLongTimeString();
            String message = msg.Substring(msg.IndexOf(':') + 2);
            broadCast(nameAndTime + "\n→  " + message);
            return "";
        }

        public void broadCast(String msg)
        {
            Console.WriteLine("BROADCAST：\"" + msg + "\" to " + clientList.Count + " people");
            foreach (chatSocket client in clientList)
            {
                if (client.active)
                {
                    Console.WriteLine("  Send to " + client.remoteEndPoint.ToString());
                    client.sendMessage(msg);
                }
            }
            Console.WriteLine("");
        }
    }
}
