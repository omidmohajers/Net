using PA.Net.Clients;
using System;
using System.Net;
using System.Net.Sockets;

namespace PA.Server.Cores
{
    public class TcpConnection : NetConnection
    {
        TcpListener server = null;
        public TcpConnection(int port)
        {
            ServerPort = port;
            ServerIP = IPAddress.Any;
            // TcpListener server = new TcpListener(port);
            server = new TcpListener(ServerIP, ServerPort);

            // Start listening for client requests.
            server.Start();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Listen()
        {
            
            try
            {
                // Buffer for reading data
                byte[] bytes = new byte[256];
              //  byte[] data = null;

                // Enter the listening loop.
                while (!Cancel)
                {
                    // Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    //  Console.WriteLine("Connected!");
                    GC.KeepAlive(client);
                    RemoteTcpClient oum = new RemoteTcpClient(client);
                    RaiseOldClientConnected(oum);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }


           // Console.WriteLine("\nHit enter to continue...");
          //  Console.Read();
        }

        public override void SendExistanceCommand(IPAddress senderIP, bool isExixsts)
        {
 
        }
    }
}
