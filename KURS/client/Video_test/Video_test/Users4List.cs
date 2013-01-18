using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Video_test
{
    public class Users4List
    {
        public string U_Login { get; set; }
        public BitmapImage U_Image { get; set; }
    }


    public class ChatMessage
    {
        public string U_Login { get; set; }
        public string U_Message { get; set; }

        public string U_Data { get { return date.ToShortDateString(); } }
        public string U_Time { get { return date.ToLongTimeString(); } }

        public DateTime date = DateTime.Now;
        
    }
}
