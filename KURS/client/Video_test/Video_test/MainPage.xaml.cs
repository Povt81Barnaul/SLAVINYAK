using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Threading;


namespace Video_test
{
    public partial class MainPage : UserControl
    {
        MemoryStreamVideoSink msv;
        ChatConnect tchc;
        CaptureSource source;
        bool state = false;
        public MainPage()
        {
            InitializeComponent();
            
            
            ImageBrush tmp= new ImageBrush();
            tmp.ImageSource = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));
            ecran.Fill = tmp;
            /**/
            MemoryStreamVideoSink.MyUsers = MyUsers;
            MemoryStreamVideoSink.obj = SynchronizationContext.Current;
            MyUsers.img = MyImage;
            MyImage.Source = new BitmapImage(new Uri(@"unknown-user.jpg", UriKind.Relative));

            
            
            
        }

        private void cw1_Closed(object sender, EventArgs e)
        {
            
            if ((sender as ChildWindow1).DialogResult == false)
            {
                Application.Current.MainWindow.Close();
            }
            else
            {

                if (source != null)
                {
                    source.Stop();
                }

                source = new CaptureSource();
                source.VideoCaptureDevice = CaptureDeviceConfiguration.GetDefaultVideoCaptureDevice();
                source.AudioCaptureDevice = CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();
                VideoBrush video = new VideoBrush();
                ///////////////////
                tchc = new ChatConnect();
                //////////////////////////
                tchc.obj = SynchronizationContext.Current;
                AudioFormat desiredAudioFormat = null;
                try
                {
                    foreach (AudioFormat audioFormat in source.AudioCaptureDevice.SupportedFormats)
                    {
                        if (audioFormat.SamplesPerSecond == 8000 && audioFormat.BitsPerSample == 16 && audioFormat.Channels == 1 && audioFormat.WaveFormat == WaveFormatType.Pcm)
                        {
                            desiredAudioFormat = audioFormat;
                        }
                    }
                }
                catch { }

                if (desiredAudioFormat == null)
                {
                    MessageBox.Show("Ошибка: микрофон отсутствует или не поддерживает требуемый аудиоформат!");
                }
                else                
                source.AudioCaptureDevice.DesiredFormat = desiredAudioFormat;
                
                
                
                ///////////////////////////

                video.SetSource(source);
                


                msv = new MemoryStreamVideoSink();

                msv.CaptureSource = source;
                msv.mediaElement1 = mediaElement1;

                msv.ac = new AudioConnect();
                msv.ac.obj = SynchronizationContext.Current;
                msv.ac._source = source;
                msv.ac.mediaElement1 = mediaElement1;
                msv.tchc = tchc;
                tchc.In=MyIn;
                tchc.Out = MyOut;

                try
                {
                    if (CaptureDeviceConfiguration.AllowedDeviceAccess || CaptureDeviceConfiguration.RequestDeviceAccess())
                    {
                        msv.Login = (sender as ChildWindow1).textBox1.Text;
                        MyLogin.Text = (sender as ChildWindow1).textBox1.Text;
                        msv.Conncet((sender as ChildWindow1).textBox2.Text, (sender as ChildWindow1));
                        source.Start();
                        ecran.Fill = video;
                        state = true;
                    }
                }
                catch (Exception eee)
                {
                    MessageBox.Show(eee.Message);
                }




                
            }

        }

        
        
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            
            ChildWindow1 cw1 = new ChildWindow1();

            cw1.Closed += new EventHandler(cw1_Closed);
            cw1.Show();
        }
        private void Send()
        {
            if (MyIn.Text.Trim().Length > 0)
            {
                tchc.SendBuffer();
            }
        }
        private void b_send_Click(object sender, RoutedEventArgs e)
        {
            Send();
        }

        private void MyIn_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.Enter)
                {
                    Send();
                    e.Handled = true;
                    return;
                }
            }

            if ((sender as TextBox).Text.Length >= 256)
            {
                e.Handled = true;
            }
        }

        private void button_out_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();
        }
    }
}
