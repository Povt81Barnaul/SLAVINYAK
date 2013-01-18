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
using System.Windows.Data;
using System.Threading;

namespace Video_test
{
    public partial class ChildWindow1 : ChildWindow
    {
        public string TextBefore="";
        public int selStart = 0;
        public int selLen = 0;
        public string TextBefore2 = "";
        public int selStart2 = 0;
        public int selLen2 = 0;
        
        public ChildWindow1()
        {
            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (textBox1.Text.Trim().Length==0)
            {
                MessageBox.Show("Введите логин!");
                return;
            }
            if (textBox2.Text.Trim().Length == 0)
            {
                MessageBox.Show("Введите IP адрес сервера!");
                return;
            }
            this.DialogResult = true;
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TextBefore = textBox1.Text;
            selStart = textBox1.SelectionStart;
            selLen = textBox1.SelectionLength;
        }

        private void textBox1_TextInput(object sender, TextCompositionEventArgs e)
        {
        }

        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender != null)
            {
                int val = (sender as TextBox).SelectionStart;
                
                string s = (sender as TextBox).Text;
                for (int i = 0; i < s.Length; i++)
                {

                    if (!(s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] >= '0' && s[i] <= '9'))
                    {
                        textBox1.Text = TextBefore;
                        textBox1.SelectionStart = selStart;
                        textBox1.SelectionLength = selLen;
                        return;
                    }
                }
            
                
                
            }
            /**/
            //MessageBox.Show("TC");
        }

        private void textBox1_TextInputStart(object sender, TextCompositionEventArgs e)
        {
            /*
            string s = e.Text;
            for (int i = 0; i < s.Length; i++)
            {

                if (!(s[i] >= 'A' && s[i] <= 'Z' || s[i] >= 'a' && s[i] <= 'z' || s[i] >= '0' && s[i] <= '9'))
                {
                    e.Handled = true;
                }
            }/**/
        }

        private void textBox1_TextInputUpdate(object sender, TextCompositionEventArgs e)
        {

         
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {

            TextBefore2 = textBox2.Text;
            selStart2 = textBox2.SelectionStart;
            selLen2 = textBox2.SelectionLength;
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender != null)
            {
                int val = (sender as TextBox).SelectionStart;

                string s = (sender as TextBox).Text;
                for (int i = 0; i < s.Length; i++)
                {

                    if (!(s[i] == '.'  || s[i] >= '0' && s[i] <= '9'))
                    {
                        textBox2.Text = TextBefore2;
                        textBox2.SelectionStart = selStart2;
                        textBox2.SelectionLength = selLen2;
                        return;
                    }
                }
            }
        }
    }
}

