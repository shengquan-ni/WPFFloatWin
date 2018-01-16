using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFFloatWin
{
    /// <summary>
    /// ScreenCapturer.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenCapturer : Window
    {
        private readonly RisCaptureLib.ScreenCaputre screenCaputre = new RisCaptureLib.ScreenCaputre();
        private TagImage tag;
        public ScreenCapturer(TagImage t)
        {
            InitializeComponent();
            screenCaputre.ScreenCaputred += OnScreenCaputred;
            screenCaputre.ScreenCaputreCancelled += OnScreenCaputreCancelled;
            tag = t;
        }

        private void OnScreenCaputreCancelled(object sender, System.EventArgs e)
        {
            tag.Window.Show();
            Close();
        }
        private void OnScreenCaputred(object sender, RisCaptureLib.ScreenCaputredEventArgs e)
        {
            tag.ReadFromScreenShot(e.Bmp);
            tag.Window.Show();
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Hide();
            screenCaputre.StartCaputre(null);
        }
    }
}
