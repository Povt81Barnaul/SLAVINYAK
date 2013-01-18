using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Windows.Data;
using System.IO;
using System.Windows.Controls;
using System.Threading;


namespace Sockets4me
{
    public class MyUser
    {
        public string U_Login { get; set; }
        public string IP_adr { get; set; }
        public string Port { get; set; }
        public bool STATUS
        {
            get;
            set;
        }
        public Socket so { get; set; }

        
    }

    public class SocketCoderVideoServer
    {

        public static SynchronizationContext obj { get; set; }
        static string SIGN = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        public static ArrayList ClientsList = new ArrayList();
        static Socket Listener_Socket;
        static SocketCoderClient Newclient;
        public static ListBox lb1 { get; set; }
        // Старт сервера
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

                return (AddressAr[0].ToString());

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
                return ("Выключение ...");
            }
            catch (Exception ex) { return (ex.Message); }

        }
        // подключение клиентов
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

        // Создание сокета для каждого клиента и добавление в ClientList
        private static void AddClient(Socket sockClient)
        {
            Newclient = new SocketCoderClient(sockClient);

            

            // логинимся
            Newclient.LetsLogin();
        }



        private static void OnRecievedLoginData(IAsyncResult ar)
        {
            string login = "";
            bool itog = true;
            SocketCoderClient client = (SocketCoderClient)ar.AsyncState;


            byte[] aryRet = client.GetRecievedData(ar);



            if (aryRet.Length < 1)
            {
                itog = false;
                // client.Sock.RemoteEndPoint - "has left the room"
                client.ReadOnlySocket.Close();
                return;
                //ClientsList.Remove(client);
                
            }
            else
            {
                
                
                
                byte[] b = aryRet;
                char[] c = new char[b.Length];
                login="";
                for (int i = 0; i < c.Length; i++)
                {
                    c[i] = Convert.ToChar(b[i]);
                    login+=c[i];
                }

                //login = Convert.ToString(c.ToString();
                login=login.Trim().ToUpper();
                foreach (SocketCoderClient clientSend in ClientsList)
                {
                    if (clientSend._login == login)
                    {
                        itog = false;
                    }
                }
                
            }
            if (itog)
            {
                string val = "OK";


                char[] c = val.ToCharArray();
                byte[] b = new byte[c.Length];
                for (int i = 0; i < c.Length; i++)
                    b[i] = Convert.ToByte(c[i]);

                client.ReadOnlySocket.NoDelay = true;
                client.ReadOnlySocket.Send(b);

                client._login = login;
                client._status = true;
                ClientsList.Add(client);
                SendListDataTo(client);

                obj.Post(new SendOrPostCallback((parameter) =>
                {

                    MyUser mu = new MyUser();
                    mu.U_Login = login;
                    mu.IP_adr = client.ReadOnlySocket.RemoteEndPoint.ToString();
                    mu.Port = client.ReadOnlySocket.RemoteEndPoint.ToString();
                    
                    mu.STATUS = client._status;
                    

                    mu.so = client.ReadOnlySocket;
                    lb1.Items.Add(mu);

                }), null);
                
                
                SendDataFrom(client, new byte[160512], 1); // вошёл в комнату
                
                client.SetupRecieveCallback();
            }
            else
            {
                try
                {
                    string val = "NO";


                    char[] c = val.ToCharArray();
                    byte[] b = new byte[c.Length];
                    for (int i = 0; i < c.Length; i++)
                        b[i] = Convert.ToByte(c[i]);

                    client.ReadOnlySocket.NoDelay = true;
                    client.ReadOnlySocket.Send(b);
                }
                catch
                {
                    client.ReadOnlySocket.Close();
                    //ClientsList.Remove(client);
                    return;
                }
                client.LetsLogin();
            }
        }

        private static void SendDataFrom(SocketCoderClient client, byte[] aryRet, byte type)
        {
            try
            {
                foreach (SocketCoderClient clientSend in ClientsList)
                {
                    if (client != clientSend)
                        try
                        {

                            char[] sigcs = SIGN.ToCharArray();
                            byte[] sig = new byte[32];
                            for (int i = 0; i < 32; i++)
                            {
                                if (i < sigcs.Length)
                                    sig[i] = Convert.ToByte(sigcs[i]);
                                else
                                    sig[i] = Convert.ToByte('\0');
                            }

                            int len = client._login.Length;
                            if (len > 256)
                                len = 256;
                            char[] c = client._login.ToCharArray();



                            byte[] b = new byte[256];
                            for (int i = 0; i < 256; i++)
                            {
                                if (i < c.Length)
                                    b[i] = Convert.ToByte(c[i]);
                                else
                                    b[i] = Convert.ToByte('\0');
                            }

                            byte[] valtype = new byte[1];
                            valtype[0] = type;// 0 - трансляция видео; 1 - пользователь вошёл; 2 - user вышел;
                            /*
                            List<ArraySegment<byte>> lb = new List<ArraySegment<byte>>();
                            ArraySegment<byte> asb0 = new ArraySegment<byte>(sig);//Сигнатура
                            ArraySegment<byte> asb1 = new ArraySegment<byte>(b);//Логин
                            ArraySegment<byte> asb1_5 = new ArraySegment<byte>(valtype);//Код типа сообшения
                            ArraySegment<byte> asb2 = new ArraySegment<byte>(aryRet);//Данные
                            lb.Add(asb0);
                            lb.Add(asb1);
                            lb.Add(asb1_5);
                            lb.Add(asb2);
                            /**/
                            byte[] BigBuf = new byte[sig.Length + b.Length + valtype.Length + aryRet.Length];
                            Array.Copy(sig, 0, BigBuf, 0, sig.Length);
                            Array.Copy(b, 0, BigBuf, sig.Length, b.Length);
                            Array.Copy(valtype, 0, BigBuf, sig.Length + b.Length, valtype.Length);
                            Array.Copy(aryRet, 0, BigBuf, sig.Length + b.Length + valtype.Length, aryRet.Length);
                            /**/

                            clientSend.ReadOnlySocket.NoDelay = true;
                            clientSend.ReadOnlySocket.Send(BigBuf);
                        }
                        catch
                        {
                            clientSend.ReadOnlySocket.Close();
                            ClientsList.Remove(clientSend);
                            for (int io=0; io<lb1.Items.Count ;io++ )
                            {
                                if ((lb1.Items[io] as MyUser).so == clientSend.ReadOnlySocket)
                                {
                                    lb1.Items.RemoveAt(io);
                                    break;
                                }
                            }
                            SendDataFrom(clientSend, new byte[160512], 2);
                            return;
                        }
                }
            }
            catch
            { }
        }



        private static void SendListDataTo(SocketCoderClient client)
        {
            
                byte[] aryRet = new byte[160512];

                
                    try
                    {

                        aryRet[0] = 0;
                        int point = 1;
                        for(int io=0;io<ClientsList.Count;io++)
                        {

                            if (client != (ClientsList[io] as SocketCoderClient))
                            {
                                aryRet[0]++;

                                string valx = (ClientsList[io] as SocketCoderClient)._login;

                                char[] valxc = valx.ToCharArray();

                                for (int k = 0; k < valxc.Length; k++)
                                {
                                    
                                    aryRet[point++] = Convert.ToByte(valxc[k]);
                                    if (point >= aryRet.Length)
                                    {
                                        io = ClientsList.Count;
                                        break; 
                                    }
                                }
                                if (point < aryRet.Length)
                                aryRet[point++] = 216;// разделительный символ
                            }
                        }
                        char[] sigcs = SIGN.ToCharArray();
                        byte[] sig = new byte[32];
                        for (int i = 0; i < 32; i++)
                        {
                            if (i < sigcs.Length)
                                sig[i] = Convert.ToByte(sigcs[i]);
                            else
                                sig[i] = Convert.ToByte('\0');
                        }

                        int len = client._login.Length;
                        if (len > 256)
                            len = 256;
                        char[] c = client._login.ToCharArray();



                        byte[] b = new byte[256];
                        for (int i = 0; i < 256; i++)
                        {
                            if (i < c.Length)
                                b[i] = Convert.ToByte(c[i]);
                            else
                                b[i] = Convert.ToByte('\0');
                        }

                        byte[] valtype = new byte[1];
                        valtype[0] = 3;// 0 - трансляция видео; 1 - пользователь вошёл; 2 - user вышел;
                        /*
                        List<ArraySegment<byte>> lb = new List<ArraySegment<byte>>();
                        ArraySegment<byte> asb0 = new ArraySegment<byte>(sig);//Сигнатура
                        ArraySegment<byte> asb1 = new ArraySegment<byte>(b);//Логин
                        ArraySegment<byte> asb1_5 = new ArraySegment<byte>(valtype);//Код типа сообшения
                        ArraySegment<byte> asb2 = new ArraySegment<byte>(aryRet);//Данные
                        lb.Add(asb0);
                        lb.Add(asb1);
                        lb.Add(asb1_5);
                        lb.Add(asb2);
                        /**/
                        byte[] BigBuf = new byte[sig.Length + b.Length + valtype.Length + aryRet.Length];
                        Array.Copy(sig, 0, BigBuf, 0, sig.Length);
                        Array.Copy(b, 0, BigBuf, sig.Length, b.Length);
                        Array.Copy(valtype, 0, BigBuf, sig.Length + b.Length, valtype.Length);
                        Array.Copy(aryRet, 0, BigBuf, sig.Length + b.Length + valtype.Length, aryRet.Length);
                        /**/

                        client.ReadOnlySocket.NoDelay = true;
                        client.ReadOnlySocket.Send(BigBuf);
                    }
                    catch
                    {
                        client.ReadOnlySocket.Close();
                        
                        for (int io = 0; io < lb1.Items.Count; io++)
                        {
                            if ((lb1.Items[io] as MyUser).so == client.ReadOnlySocket)
                            {
                                lb1.Items.RemoveAt(io);
                                break;
                            }
                        }
                        /**/
                        ClientsList.Remove(client);
                        
                        return;
                    }
            
        }


        // (4) Отправка полученных данных всем клиентам в комнате
        private static void OnRecievedData(IAsyncResult ar)
        {
            SocketCoderClient client = (SocketCoderClient)ar.AsyncState;
            
            
                byte[] aryRet = client.GetRecievedData(ar);


                if (aryRet.Length < 1)
                {
                    
                    SendDataFrom(client, new byte[160512], 2); // вышел из комнаты

                    obj.Post(new SendOrPostCallback((parameter) =>
                    {

                        for (int io = 0; io < lb1.Items.Count; io++)
                        {
                            if ((lb1.Items[io] as MyUser).so == client.ReadOnlySocket)
                            {

                                lb1.Items.RemoveAt(io);
                                break;
                            }
                        }/**/    

                    }), null);
                    
                    client.ReadOnlySocket.Close();
                    ClientsList.Remove(client);
                    return;
                }
                bool val = true;
                for (int io = 0; io < lb1.Items.Count; io++)
                {
                    if ((lb1.Items[io] as MyUser).so == client.ReadOnlySocket)
                        if ((lb1.Items[io] as MyUser).STATUS == false)
                        {
                            val = false;
                            break;
                        }
                }

                if (val)
                {
                SendDataFrom(client, aryRet, 0);
            }
            client.SetupRecieveCallback();
        }

        internal class SocketCoderClient
        {
            // Сокет отдельного клиента

            public bool _status=false;
            public string _login = "";
            
            private Socket New_Socket;
            private byte[] buffer = null;

            public SocketCoderClient(Socket PassedSock)
            {
                New_Socket = PassedSock;
                New_Socket.NoDelay = true;
            }

            public Socket ReadOnlySocket
            {
                get { return New_Socket; }
            }



            public void LetsLogin()
            {
                try
                {
                    buffer = new byte[1024];
                    AsyncCallback recieveData = new AsyncCallback(SocketCoderVideoServer.OnRecievedLoginData);
                    New_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, recieveData, this);
                }
                catch (Exception)
                {
                }
            }


            public void SetupRecieveCallback()
            {
                try
                {
                    
                    buffer = new byte[160512];
                    AsyncCallback recieveData = new AsyncCallback(SocketCoderVideoServer.OnRecievedData);
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
                catch(Exception e)
                {
                    string ss = e.Message;
                }
                byte[] byReturn = new byte[nBytesRec];
                Array.Copy(buffer, byReturn, nBytesRec);
                return byReturn;
            }
        }

    }
}
