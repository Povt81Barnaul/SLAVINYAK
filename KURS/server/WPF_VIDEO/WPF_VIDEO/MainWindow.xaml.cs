using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Sockets4me;

namespace WPF_VIDEO
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SocketCoderVoiceServer VoiceServer;
        SocketCoderVideoServer VideoServer;
        SocketTextChatServer TCServer;

        public MainWindow()
        {
            InitializeComponent();
            button2.IsEnabled = false;
            SocketCoderVoiceServer.obj = SynchronizationContext.Current;
            SocketCoderVoiceServer.lb1 = this.listBox1;
        }
        
        
        

        private void button1_Click(object sender, RoutedEventArgs e)
        {

            button1.IsEnabled = false;
            button2.IsEnabled = true;

            VoiceServer = new SocketCoderVoiceServer();
            VoiceServer.Start_A_Server_On(4530);
            checkBox1.IsChecked = true;

            VideoServer = new SocketCoderVideoServer();
            SocketCoderVideoServer.lb1 = listBox1;
            label4.Content = VideoServer.Start_A_Server_On(4531);
            checkBox2.IsChecked = true;
            SocketCoderVideoServer.obj = SynchronizationContext.Current;
            
            TCServer = new SocketTextChatServer();
            TCServer.Start_A_Server_On(4532);
            checkBox3.IsChecked = true;

            
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            label4.Content = "---.---.---.---";
            VoiceServer.ShutDown();
            VideoServer.ShutDown();
            TCServer.ShutDown();
            checkBox1.IsChecked = false;
            checkBox2.IsChecked = false;
            checkBox3.IsChecked = false;
            button1.IsEnabled = true;
            button2.IsEnabled = false;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
                
        }

        

        

    }
}
