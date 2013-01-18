using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.Text;
using FluxJpeg.Core;
using FluxJpeg.Core.Encoder;

using VoiceEncoder;


namespace Video_test
{

    public class ChatConnect
    {
        public ListBox Out { get; set; }

        public TextBox In { get; set; }

        public string Login { get; set; }
        
        



        public SynchronizationContext obj { get; set; }
        
        private Socket _client_socket;
        private int BufferSize = 1024;
        


        #region Socket Methods

        public void Connect(string IP_Address)
        {
            _client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP_Address), 4532)
            };
            
            socketEventArg.Completed += OnConncetCompleted;
            _client_socket.ConnectAsync(socketEventArg);
            
        }
        void Send_Bytes(byte[] buffer)
        {
            _client_socket.NoDelay = true;

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnSendCompleted;
            socketEventArg.SetBuffer(buffer, 0, buffer.Length);
            _client_socket.SendAsync(socketEventArg);

        }
        void StartReceiving()
        {
            byte[] response = new byte[BufferSize + 256];
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnReceiveCompleted;
            socketEventArg.SetBuffer(response, 0, response.Length);
            _client_socket.ReceiveAsync(socketEventArg);
        }
        void OnConncetCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                
                StartReceiving();
            }
            else
            {
                
            }

        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {

        }

        void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            obj.Post(new SendOrPostCallback((parameter) =>
            {
                PostBuffer(e.Buffer);
            }), null);
        }

        #endregion Socket Methods

        #region Encoding/Decoding Methods

        
        public void SendBuffer()
        {

            byte[] ba = (new UTF8Encoding().GetBytes(In.Text));
            In.Text = "";

            

            
            byte[] Encoded = new byte[BufferSize + 256];
            for (int io = 256; io < BufferSize + 256; io++)
            {
                if (io-256 < ba.Length)
                {
                    Encoded[io] = ba[io - 256];
                }
            }
            char[] c = Login.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if(i<256)
                Encoded[i] = Convert.ToByte(c[i]);
            }

                Send_Bytes(Encoded);



        }

        private void PostBuffer(byte[] Encodedbuffer)
        {

            try
            {
                ChatMessage cm =new ChatMessage();
                cm.U_Login = "";

                for (int i = 0; i < 256; i++)
                {
                    char c = Convert.ToChar(Encodedbuffer[i]);
                    if (c != '\0')
                        cm.U_Login += c;
                }
                
                cm.U_Message = (new UTF8Encoding().GetString(Encodedbuffer,256,1024));

                Out.Items.Insert(0,cm);
                
                
                
                ////////////////////////////
                
            }
            catch (Exception) { }

            StartReceiving();
        }
        


        #endregion Encoding/Decoding Methods

    }

    public class AudioConnect
    {
        public MediaElement mediaElement1 { get; set; }
        public CaptureSource _source { get; set; }
        public SynchronizationContext obj { get; set; }
        private PcmToWave _pcm = new PcmToWave();
        private MemoryAudioSink _sink;
        private Socket _client_socket;
        private int BufferSize = 8000;
        private bool _isRecording;
        public string Login {get; set;}

        
        #region Socket Methods

        public void Connect(string IP_Address)
        {
            _client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP_Address), 4530)
            };
            socketEventArg.Completed += OnConncetCompleted;
            _client_socket.ConnectAsync(socketEventArg);
        }
        void Send_Bytes(byte[] buffer)
        {
            _client_socket.NoDelay = true;

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnSendCompleted;
            socketEventArg.SetBuffer(buffer, 0, buffer.Length);
            _client_socket.SendAsync(socketEventArg);

        }
        void StartReceiving()
        {
            byte[] response = new byte[BufferSize+256];
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnReceiveCompleted;
            socketEventArg.SetBuffer(response, 0, response.Length);
            _client_socket.ReceiveAsync(socketEventArg);
        }
        void OnConncetCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                StartRecording();
                StartReceiving();
                _sink.StartSending = true;
                
            }
            else
            {
                
            }

        }
        void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {

        }
        
        void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            obj.Post(new SendOrPostCallback((parameter) =>
            {
                PlayReceivedBuffer(e.Buffer);
            }), null);   
        }

        #endregion Socket Methods

        #region Encoding/Decoding Methods

        void StartRecording()
        {
            _sink = new MemoryAudioSink();
            _sink.OnBufferFulfill += new EventHandler(SendVoiceBuffer);
            _sink.CaptureSource = _source;
            _isRecording = true;
        }
        void SendVoiceBuffer(object VoiceBuffer, EventArgs e)
        {
            

            byte[] PCM_Buffer = (byte[])VoiceBuffer;
            //////////////////////

            byte[] Encoded = new byte[BufferSize + 256];
            for (int io = 256; io < BufferSize + 256; io++)
            {
                if (io - 256 < PCM_Buffer.Length)
                {
                    Encoded[io] = PCM_Buffer[io - 256];
                }
            }

            char[] c = Login.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (i < 256)
                    Encoded[i] = Convert.ToByte(c[i]);
            }

            //byte[] Encoded = PCM_Buffer;
            //byte[] Encoded = VoiceEncoder.G711Audio.ALawEncoder.ALawEncode(PCM_Buffer);
            ///////////////////////
            Send_Bytes(Encoded);



        }
        void StopRecording()
        {
            if (_source.State == CaptureState.Started)
            {
                _source.Stop();
                _isRecording = false;
                _sink.StartSending = false;
            }
        }
        private void PlayReceivedBuffer(byte[] Encodedbuffer)
        {

            try
            {
                //Decode to Wave Format Then Play
                byte[] DecodedBuffer = new byte[BufferSize];
                ////////////////////////////
                //Encodedbuffer.CopyTo(DecodedBuffer, 256);
                for (int io = 0; io < BufferSize; io++)
                {
                    DecodedBuffer[io] = Encodedbuffer[io + 256];
                }
                //DecodedBuffer = Encodedbuffer;

                /*
             
                 */
                //VoiceEncoder.G711Audio.ALawDecoder.ALawDecode(Encodedbuffer, out DecodedBuffer); ;
                ///////////////////////////
                PlayWave(DecodedBuffer);

            }
            catch (Exception es)
            {

            }
            finally
            {
                StartReceiving();
            }
        }
        void PlayWave(byte[] PCMBytes)
        {
            MemoryStream ms_PCM = new MemoryStream(PCMBytes);
            MemoryStream ms_Wave = new MemoryStream();
            /////////////////////////
            //ms_Wave = ms_PCM;
            _pcm.SavePcmToWav(ms_PCM, ms_Wave, 16, 8000, 1);
            /////////////////////////////
            WaveMediaStreamSource WaveStream = new WaveMediaStreamSource(ms_Wave);

            mediaElement1.SetSource(WaveStream);
            mediaElement1.Play();
        }
        

        #endregion Encoding/Decoding Methods

    }


    public class MemoryStreamVideoSink : VideoSink
    {
        public string ip_addr;

        public ChatConnect tchc { get; set; }
        public static SynchronizationContext obj { get; set; }
        

        public static BitmapImage ZGL = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));
        public VideoFormat CapturedFormat { get; private set; }
        public MemoryStream CapturedVideo { get; private set; }
        //public ImageBrush MyImage { get; set; }
        public MediaElement mediaElement1 { get; set; }
        public ChildWindow1 nsv;
        public AudioConnect ac { get; set; }
        public static ListOfUsers MyUsers { get; set; }
        
        public Socket client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public bool Success = false;
        public string Login = "";

        #region Encoding/Decoding Methods
        public static void EncodeJpeg(WriteableBitmap bmp, Stream dstStream)
        {
            // Init buffer in FluxJpeg format
            int w = bmp.PixelWidth;
            int h = bmp.PixelHeight;
            int[] p = bmp.Pixels;
            byte[][,] pixelsForJpeg = new byte[3][,]; // RGB colors
            pixelsForJpeg[0] = new byte[w, h];
            pixelsForJpeg[1] = new byte[w, h];
            pixelsForJpeg[2] = new byte[w, h];

            // Copy WriteableBitmap data into buffer for FluxJpeg
            int i = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int color = p[i++];
                    pixelsForJpeg[0][x, y] = (byte)(color >> 16); // R
                    pixelsForJpeg[1][x, y] = (byte)(color >> 8);  // G
                    pixelsForJpeg[2][x, y] = (byte)(color);       // B
                }
            }
            
            //Encode Image as JPEG
            var jpegImage = new FluxJpeg.Core.Image(new ColorModel { colorspace = ColorSpace.RGB }, pixelsForJpeg);
            var encoder = new JpegEncoder(jpegImage, 95, dstStream);
            encoder.Encode();
        }



        public static void EncodeJpeg(byte[] p, int h_in,int w_in, Stream dstStream)
        {
            // Init buffer in FluxJpeg format
            int w = w_in;
            int h = h_in;
            
            byte[][,] pixelsForJpeg = new byte[3][,]; // RGB colors
            pixelsForJpeg[0] = new byte[w, h];
            pixelsForJpeg[1] = new byte[w, h];
            pixelsForJpeg[2] = new byte[w, h];

            // Copy WriteableBitmap data into buffer for FluxJpeg
            int i = 0;
            
            for (int y = h-1; y >=0 ; y--)
            {
                for (int x = 0; x < w; x++)
                {   
                        
                        pixelsForJpeg[2][x, y] = p[i++]; // R
                        pixelsForJpeg[1][x, y] = p[i++]; // G
                        pixelsForJpeg[0][x, y] = p[i++]; // B
                        i++;
                }
            }

            //Encode Image as JPEG
            var jpegImage = new FluxJpeg.Core.Image(new ColorModel { colorspace = ColorSpace.RGB }, pixelsForJpeg);
            var encoder = new JpegEncoder(jpegImage, 95, dstStream);
            encoder.Encode();
        }



        public static WriteableBitmap DecodeJpeg(Stream srcStream)
        {
            
            // Decode JPEG
            var decoder = new FluxJpeg.Core.Decoder.JpegDecoder(srcStream);
            var jpegDecoded = decoder.Decode();
            var img = jpegDecoded.Image;
            img.ChangeColorSpace(ColorSpace.RGB);

            // Init Buffer
            int w = img.Width;
            int h = img.Height;
            var result = new WriteableBitmap(w, h);
            int[] p = result.Pixels;
            byte[][,] pixelsFromJpeg = img.Raster;
            // Copy FluxJpeg buffer into WriteableBitmap
            int i = 0;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    p[i++] = (0xFF << 24) // A
                                | (pixelsFromJpeg[0][x, y] << 16) // R
                                | (pixelsFromJpeg[1][x, y] << 8)  // G
                                | pixelsFromJpeg[2][x, y];       // B
                }
            }

            return result;
        }
        
        private void StartEncoding()
        {
            try
            {

                //WriteableBitmap bmp = new WriteableBitmap(rectVideo, null);
                //MemoryStream ms = new MemoryStream();
                //EncodeJpeg(bmp, ms);
                //Send_Bytes(ms.GetBuffer());
            }
            catch (Exception) { }
        }
        private void ViewReceivedImage(byte[] buffer)
        {
            try
            {
                /*
                MemoryStream ms = new MemoryStream(buffer);
                BitmapImage bi = new BitmapImage();
                bi.SetSource(ms);
                MyImage.Source = bi;
                ms.Close();
                /**/
            }
            catch (Exception) { }
            finally
            {
                //StartReceiving();
            }
        }
        #endregion Encoding/Decoding Methods


        
        public void Conncet(string IP_Address, ChildWindow1 cw)
        {
            ip_addr = IP_Address;
            nsv = cw;
            client_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs()
            {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP_Address), 4531)
            };
            
            
            socketEventArg.Completed += OnConncetCompleted;
            client_socket.ConnectAsync(socketEventArg);
        }


        void StartReceiving()
        {
            byte[] response = new byte[160768 + 32 + 1];
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnReceiveCompleted;
            socketEventArg.SetBuffer(response, 0, response.Length);
            client_socket.ReceiveAsync(socketEventArg);
        }


        public void OnConncetCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                SendLoginBytes();
            }
            else
            {
                Success = false;


                obj.Post(new SendOrPostCallback((parameter) =>
                {
                    MessageBox.Show("Подключиться к серверу не удалось! " + e.SocketError);
                    nsv.Show();    
                }), null);
                
            }

        }


        public void SendLoginBytes()
        {
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnSendLoginCompleted;

            
            
            Login = Login.Trim().ToUpper();

            char[] c = Login.ToCharArray();
            byte[] b = new byte[c.Length];
            for (int i = 0; i < c.Length; i++)
                b[i] = Convert.ToByte(c[i]);
            socketEventArg.SetBuffer(b, 0, b.Length);
            client_socket.SendAsync(socketEventArg);
        }
        public void OnSendLoginCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.Dispose();
            StartReceivingLogResult();
            // Show Message or something ...
            //this.Dispatcher.BeginInvoke(new ShowMessagedelegate(ShowMessageBox), "Sent Successfully!");
        }

        void StartReceivingLogResult()
        {

            byte[] response = new byte[2];
            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnReceiveLoginCompleted;
            socketEventArg.SetBuffer(response, 0, response.Length);
            client_socket.ReceiveAsync(socketEventArg);

        }
        void OnReceiveLoginCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                /*
                obj.Post(new SendOrPostCallback((parameter) =>
                {
                    //MessageBox.Show("-=1=-");
                }), null);
                /**/

                bool good=false;
                byte[] b = e.Buffer;
                char[] c = new char[b.Length];
                string res = "";
                for (int i = 0; i < c.Length; i++)
                {
                    
                    c[i] = Convert.ToChar(b[i]);
                    if (c[i]!='\0')
                    res += c[i];
                }
                res = res.Trim().ToUpper();
                if (res == "OK")
                    good = true;
            if (good)
            {

                

                try
                {
                tchc.Login = Login;
                tchc.Connect(ip_addr);
                ac.Login = Login;
                ac.Connect(ip_addr);
                }catch(Exception es)
                {
                    obj.Post(new SendOrPostCallback((parameter) =>
                    {
                        MessageBox.Show("Ошибка! " + es.Message);

                    }), null);
                }

                Success = true;
                StartReceiving();
            }
            else
            {
                client_socket.Close();
                obj.Post(new SendOrPostCallback((parameter) =>
                {
                    MessageBox.Show("Пользователь с таким логином уже в сети!");
                    nsv.Show();
                }), null);
                
                
                //SendLoginBytes();
            }
            }
            catch (Exception) { }
            
        }




        
        void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            byte[] buffer=e.Buffer;
            try
            {

                char[] sc = new char[32];
                string sig = "";
                for (int i = 0; i < sc.Length; i++)
                {

                    sc[i] = Convert.ToChar(buffer[i]);
                    if (sc[i] != '\0')
                        sig += sc[i];
                }
                sig = sig.Trim().ToUpper();


                char[] c = new char[256];
                string res = "";
                if (sig == "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA")
                {
                    for (int i = 0; i < c.Length; i++)
                    {
                        c[i] = Convert.ToChar(buffer[i + 32]);
                        if (c[i] != '\0')
                            res += c[i];
                    }
                    res = res.Trim().ToUpper();

                    byte valtp = new byte();

                    valtp = buffer[256 + 32];

                    if (valtp == 0)
                    {
                        

                        

                            obj.Post(new SendOrPostCallback((parameter) =>
                            {
                                MemoryStream ms = new MemoryStream(buffer, 256 + 32 + 1, buffer.Length - 256 - 32 - 1);
                        bool finded = false;
                        for (int io = 0; io < MyUsers.MyList.Items.Count; io++)
                        {
                            if ((MyUsers.MyList.Items[io] as Users4List).U_Login == res)
                            {
                                (MyUsers.MyList.Items[io] as Users4List).U_Image.SetSource(ms);
                                finded = true;
                                break;
                            }
                        }
                            
                        if (!finded)
                        {
                            
                            Users4List tmp = new Users4List();
                            tmp.U_Login = res;
                            tmp.U_Image = new BitmapImage();
                            tmp.U_Image.SetSource(ms);
                            MyUsers.MyList.Items.Add(tmp);
                            //                        UsersList.Add(tmp);
                            

                        }
                        ms.Close();
                            }), null);

                        //BitmapImage bi = new BitmapImage();
                        //bi.SetSource(ms);

                        //MyImage.Source = bi;
//                        buffer = null;
                        
                    }
                    else
                    {
                        if (valtp == 1)// Добавлен user
                        {


                            obj.Post(new SendOrPostCallback((parameter) =>
                            {
                            Users4List tmp = new Users4List();
                            tmp.U_Login = res;
                            tmp.U_Image = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));
                            MyUsers.MyList.Items.Add(tmp);
                            }), null);
                        }
                        else
                            if (valtp == 2)// Удалён user
                            {
                                obj.Post(new SendOrPostCallback((parameter) =>
                                {
                                    
                                
                                for (int io = 0; io < MyUsers.MyList.Items.Count; io++)
                                {
                                    if ((MyUsers.MyList.Items[io] as Users4List).U_Login == res)
                                    {
                                        if (MyUsers.MyList.SelectedIndex == io) // меняем выбранный элемент
                                        {
                                            MyUsers.MyList.SelectedIndex--;

                                        }
                                        if (MyUsers.MyList.SelectedIndex < 0)
                                        {
                                            MyUsers.img.Source = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));
                                        }
                                        MyUsers.MyList.Items.RemoveAt(io);
                                    }
                                }
                                }), null);
                            }
                            else
                        if (valtp == 3)// Список пользователей
                        {
                            ///////////////////УДАЛИТЬ ВСЁ ЭТО///////////////////////////////////////////////
                            /*
                            Users4List tmp2 = new Users4List();
                            tmp2.U_Login = "SYNOPTIC";
                            tmp2.U_Image = ZGL;
                            obj.Post(new SendOrPostCallback((parameter) =>
                            {
                                MyUsers.MyList.Items.Add(tmp2);
                            }), null);
                            /**/
                            ////////////////////////////////////////////////////////////////////////////////

                            obj.Post(new SendOrPostCallback((parameter) =>
                                    {
                            int ix = 256 + 32 + 1;
                            char tmpc;
                            string str = "";
                            int col = Convert.ToInt32(buffer[ix]);
                            ix++;
                            if(col>0)
                            for (int io = 0; ix < buffer.Length; ix++)
                            {
                                str = "";
                                for (; buffer[ix] != 216; ix++)
                                {
                                    tmpc = Convert.ToChar(buffer[ix]);
                                    str += tmpc;
                                }

                                if (str != "")
                                {

                                    bool finded = false;
                                    for (int iox = 0; iox < MyUsers.MyList.Items.Count; iox++)
                                    {
                                        if (str == (MyUsers.MyList.Items[iox] as Users4List).U_Login)
                                        {
                                            finded = true;
                                            break;
                                        }
                                    }
                                    if (!finded)
                                    {
                                        Users4List tmp = new Users4List();
                                        tmp.U_Login = str;
                                        tmp.U_Image = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));
                                        MyUsers.MyList.Items.Add(tmp);
                                    }
                                    
                                    io++;
                                    if (io == col)
                                        break;
                                }
                            }
                                    }), null);
                        }
                    }
                }
                else
                {
                    
                }
            }

            catch (Exception eeeee) 
            {
                string ssss = eeeee.Message;
            }
            finally
            {
                StartReceiving();
            }
        }

        

        public void Send_Bytes(byte[] buffer)
        {

            SocketAsyncEventArgs socketEventArg = new SocketAsyncEventArgs();
            socketEventArg.Completed += OnSendCompleted;
            socketEventArg.SetBuffer(buffer, 0, buffer.Length);
            client_socket.SendAsync(socketEventArg);
            
        }

        public void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            Success = true;
            e.Dispose();
            // Show Message or something ...
            //this.Dispatcher.BeginInvoke(new ShowMessagedelegate(ShowMessageBox), "Sent Successfully!");
        }
        protected override void OnCaptureStarted()
        {
            CapturedVideo = new MemoryStream();
        }
        protected override void OnCaptureStopped()
        {
        }
        protected override void OnFormatChange(VideoFormat videoFormat)
        {
            if (CapturedFormat != null)
            {   
                throw new InvalidOperationException("Ошибка!");
            }
            this.CapturedFormat = new VideoFormat(PixelFormatType.Format32bppArgb, 640, 480, videoFormat.FramesPerSecond);
            
        }
        
        protected override void OnSample(long sampleTime, long frameDuration, byte[] sampleData)
        {
            if (Success)
            {
//                Success = false;
                MemoryStream ms = new MemoryStream();
                EncodeJpeg(sampleData, this.CapturedFormat.PixelHeight, this.CapturedFormat.PixelWidth, ms);
                sampleData = null;
                Send_Bytes(ms.GetBuffer());
            }
        }
    }

}
