using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Video_test
{
    public partial class ListOfUsers : UserControl
    {
        public Image img { get; set; }
        public ListOfUsers()
        {
            InitializeComponent();
        }

        private void MyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.MyList.SelectedIndex >= 0)
            {
                img.Source = (this.MyList.SelectedItem as Users4List).U_Image;
            }

        }

    }
}
