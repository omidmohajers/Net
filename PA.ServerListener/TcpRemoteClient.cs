using PA.Net.Clients;
using PA.Net.Core;
using PA.StockMarket.Data;
using System;
using System.Net;
using System.Net.Sockets;

namespace PA.VideoServer.Win.Core
{
    public class TcpRemoteClient : TcpNetClient
    {
        public Account UserProfile { get; set; }
        public void SendExistanceCommand(bool isExixsts,IPAddress serverIP)
        {
            Package pak = new Package(CommandType.IsNameExists, this.IP, serverIP, isExixsts.ToString(), null);
            pak.SenderIP = serverIP;
            Send(Package.ToByteArray(pak));
        }
        public void SendClientList(string val, IPAddress serverIP)
        {
            Package pak = new Package(CommandType.SendClientList, this.IP, serverIP, val, null);
            pak.SenderIP = serverIP;
            Send(Package.ToByteArray(pak));
        }
        public FileTransferController SendFileOrder(long fileID, string fileName, long fileSize, IPAddress serverIP)
        {
            FileTransferController ftp = new FileTransferController();
            ftp.FileID = fileID;
            ftp.FileSize = fileSize;
            ftp.FileName = fileName;
            ftp.TransferSide = TransferSide.UploadToServer;
            ftp.Data = null;
            ftp.Start = 0;
            ftp.End = 0;
            

            Package pak = new Package(CommandType.FileControl, this.IP, serverIP, "", null);
            pak.Data = FileTransferController.ToByteArray(ftp);
            pak.SenderIP = serverIP;
            Send(Package.ToByteArray(pak));

            return ftp;
        }
        public TcpRemoteClient(TcpClient client)
        {
            //ID = id;
            Client = client;
           // CreateChannel(securityType);
            // ---------------------------------- Channel Events-----------------------------------
            channel.DataReceived += Channel_DataReceived;
            // ------------------------------------------------------------------------------------
            Start();
        }

        public override bool Disconnect()
        {
            if (Client != null && Client.Connected)
            {
                try
                {
                    Client.Client.Disconnect(true);
                    if (Client.Connected)
                    {
                        Client.GetStream().Close();
                        Client.Close();
                    }
                    return true;
                }
                catch(Exception ex)
                {
                    Log.Add(ReportType.Error, ClientName, string.Format("خطا در زمان قطع اتصال از سرور:{0}", ex.Message), ex, DateTime.Now);

                    RaiseDisconnectFaild();
                    return false;
                }
            }
            else
            {
                RaiseDisconnectSuceeded();
                return true;
            }
        }
    }
}
