using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFFloatWin
{
    /// <summary>
    /// TagGenerator.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class TagGenerator : Window
    {
        int index = 0;
        public TagGenerator()
        {
            InitializeComponent();
            this.TagC.Opacity = 0;
            DoubleAnimation Anim = new DoubleAnimation();
            Anim.From = 0;
            Anim.To = 1;
            Anim.Duration = TimeSpan.FromMilliseconds(1000);
            this.TagC.BeginAnimation(Canvas.OpacityProperty, Anim);
            NowText.Text = TagWindow.functions.Keys.ElementAt(index);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            this.Topmost = true;
            this.Left = 0.0;
            this.Top = 0.0;
            this.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            TagWindow tagw=((MainWindow)Application.Current.MainWindow).CreateTagWindow();
            ((MainWindow)Application.Current.MainWindow).CreateAndAddTag(NowText.Text, "", tagw);
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta>0)
            {
                index++;
                if (index >= TagWindow.functions.Keys.Count)
                    index = 0;
            }
            else
            {
                index--;
                if (index < 0)
                    index = TagWindow.functions.Keys.Count - 1;
            }
            this.NowText.Text = TagWindow.functions.Keys.ElementAt(index);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).IsExistTagGen = false;
        }
    }
}
