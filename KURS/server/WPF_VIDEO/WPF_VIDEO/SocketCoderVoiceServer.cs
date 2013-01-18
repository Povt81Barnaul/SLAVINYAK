using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Windows.Controls;

namespace Sockets4me
{


    public class SocketTextChatServer
    {
        static ArrayList ClientsList = new ArrayList();
        static Socket Listener_Socket;
        static SocketCoderClient Newclient;

        // Запуск сервера
        public string Start_A_Server_On(int Port)
        {
            try
            {
                IPAddress[] AddressAr = null;
                String ServerHostName = "";

                try
                {
                    ServerHostName = Dns.GetHostName();
                    IPHostEntry ipEntry = Dns.GetHostByName(ServerHostName);
                    AddressAr = ipEntry.AddressList;
                }
                catch (Exception) { }

                if (AddressAr == null || AddressAr.Length < 1)
                {
                    return "Unable to get local address ... Error";
                }

                Listener_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Listener_Socket.Bind(new IPEndPoint(AddressAr[0], Port));
                Listener_Socket.Listen(-1);

                Listener_Socket.BeginAccept(new AsyncCallback(EndAccept), Listener_Socket);

                return (AddressAr[0].ToString() + ":" + Port);

            }
            catch (Exception ex) { return ex.Message; }
        }
        public string ShutDown()
        {
            try
            {
                Listener_Socket.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return ("Shuted down ...");
            }
            catch (Exception ex) { return (ex.Message); }

        }
        // Подключение клиентов
        private static void EndAccept(IAsyncResult ar)
        {
            try
            {
                Listener_Socket = (Socket)ar.AsyncState;
                AddClient(Listener_Socket.EndAccept(ar));
                Listener_Socket.BeginAccept(new AsyncCallback(EndAccept), Listener_Socket);
            }
            catch (Exception) { }
        }

        // Добавление клиента
        private static void AddClient(Socket sockClient)
        {
            Newclient = new SocketCoderClient(sockClient);

            ClientsList.Add(Newclient);

            
            Newclient.SetupRecieveCallback();
        }

        // Передача данным всем в комнате
        private static void OnRecievedData(IAsyncResult ar)
        {
            SocketCoderClient client = (SocketCoderClient)ar.AsyncState;



            byte[] aryRet = client.GetRecievedData(ar);


            if (aryRet.Length < 1)
            {
                // вышел из комнаты
                client.ReadOnlySocket.Close();
                ClientsList.Remove(client);
                return;
            }

            

            foreach (SocketCoderClient clientSend in ClientsList)
            {
                //if (client != clientSend)
                    try
                    {
                        clientSend.ReadOnlySocket.NoDelay = true;
                        clientSend.ReadOnlySocket.Send(aryRet);
                    }
                    catch
                    {
                        clientSend.ReadOnlySocket.Close();
                        ClientsList.Remove(client);
                        return;
                    }
            }
            client.SetupRecieveCallback();
        }

        internal class SocketCoderClient
        {
            // клиент

            private Socket New_Socket;
            private byte[] buffer = null;
//            public string _login = "";


            public SocketCoderClient(Socket PassedSock)
            {
                New_Socket = PassedSock;
                New_Socket.NoDelay = true;
            }

            public Socket ReadOnlySocket
            {
                get { return New_Socket; }
            }

            public void SetupRecieveCallback()
            {
                try
                {
                    buffer = new byte[1024+256];
                    AsyncCallback recieveData = new AsyncCallback(SocketTextChatServer.OnRecievedData);
                    New_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, recieveData, this);
                }
                catch (Exception)
                {
                }
            }
            public byte[] GetRecievedData(IAsyncResult ar)
            {
                int nBytesRec = 0;
                try
                {
                    nBytesRec = New_Socket.EndReceive(ar);
                }
                catch { }
                byte[] byReturn = new byte[nBytesRec];
                Array.Copy(buffer, byReturn, nBytesRec);
                return byReturn;
            }
        }

    }


    public class SocketCoderVoiceServer
    {
        public static SynchronizationContext obj { get; set; }
        public static ListBox lb1 { get; set; }
        static ArrayList ClientsList = new ArrayList();
        static Socket Listener_Socket;
        static SocketCoderClient Newclient;

        // Запуск сервера
        public string Start_A_Server_On(int Port)
        {
            try
            {
                IPAddress[] AddressAr = null;
                String ServerHostName = "";
                
                try
                {
                    ServerHostName = Dns.GetHostName();
                    IPHostEntry ipEntry = Dns.GetHostByName(ServerHostName);
                    AddressAr = ipEntry.AddressList;
                }
                catch (Exception) {}

                if (AddressAr == null || AddressAr.Length < 1)
                {
                    return "Unable to get local address ... Error";
                }

                Listener_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                Listener_Socket.Bind(new IPEndPoint(AddressAr[0], Port));
                Listener_Socket.Listen(-1);

                Listener_Socket.BeginAccept(new AsyncCallback(EndAccept), Listener_Socket);

                return (AddressAr[0].ToString() +":"+ Port);

            }
            catch (Exception ex) { return ex.Message; }
        }
        public string ShutDown()
        {
            try
            {
                Listener_Socket.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return ("Shuted down ...");
            }
            catch (Exception ex) { return (ex.Message); }

        }
        // Подключение клиентов
        private static void EndAccept(IAsyncResult ar)
        {
            try
            {
                Listener_Socket = (Socket)ar.AsyncState;
                AddClient(Listener_Socket.EndAccept(ar));
                Listener_Socket.BeginAccept(new AsyncCallback(EndAccept), Listener_Socket);
            }
            catch (Exception) { }
        }

        // Добавление пользователя в ClientList
        private static void AddClient(Socket sockClient)
        {
            Newclient = new SocketCoderClient(sockClient);

            ClientsList.Add(Newclient);

            
            Newclient.SetupRecieveCallback();
        }

        // Отправка данных
        private static void OnRecievedData(IAsyncResult ar)
        {
            SocketCoderClient client = (SocketCoderClient)ar.AsyncState;


            
                byte[] aryRet = client.GetRecievedData(ar);


                if (aryRet.Length < 1)
                {
                    
                    client.ReadOnlySocket.Close();
                    ClientsList.Remove(client);
                    return;
                }

                
                if (client._login == "")
                {
                    string login = "";
                    for (int i = 0; i < 256; i++)
                    {
                        char c =Convert.ToChar(aryRet[i]);
                        if(c!='\0')
                        {
                            login+=c;
                        }
                        else
                        {
                            break;
                        }
                    }
                    client._login = login;
                }
                obj.Post(new SendOrPostCallback((parameter) =>
                {

                bool val = true;

                
                    
                
                for (int io = 0; io < lb1.Items.Count; io++)
                {
                    if ((lb1.Items[io] as MyUser).U_Login == client._login)
                        if ((lb1.Items[io] as MyUser).STATUS == false)
                        {
                            val = false;
                            break;
                        }
                }

                if (val)
                {
                foreach (SocketCoderClient clientSend in ClientsList)
                {
                    if (client != clientSend)
                        try
                        {
                            clientSend.ReadOnlySocket.NoDelay = true;
                            clientSend.ReadOnlySocket.Send(aryRet);
                        }
                        catch
                        {
                            clientSend.ReadOnlySocket.Close();
                            ClientsList.Remove(client);
                            return;
                        }
                }
                    
            }
                }), null);
            client.SetupRecieveCallback();
        }

        internal class SocketCoderClient
        {
            

            private Socket New_Socket;
            private byte[] buffer = null;
            public string _login = "";

            public SocketCoderClient(Socket PassedSock)
            {
                New_Socket = PassedSock;
                New_Socket.NoDelay = true;
            }

            public Socket ReadOnlySocket
            {
                get { return New_Socket; }
            }

            public void SetupRecieveCallback()
            {
                try
                {
                    buffer = new byte[15840];
                    AsyncCallback recieveData = new AsyncCallback(SocketCoderVoiceServer.OnRecievedData);
                    New_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, recieveData, this);
                }
                catch (Exception)
                {
                }
            }
            public  byte[] GetRecievedData(IAsyncResult ar)
            {
                int nBytesRec = 0;
                try
                {
                    nBytesRec = New_Socket.EndReceive(ar);
                }
                catch { }
                byte[] byReturn = new byte[nBytesRec];
                Array.Copy(buffer, byReturn, nBytesRec);
                return byReturn;
            }
        }

    }
}
