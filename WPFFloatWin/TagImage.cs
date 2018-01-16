using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Globalization;
using System.IO;
using System.Windows.Interop;

namespace WPFFloatWin
{
    public class TagImage:TagBase
    {
        Canvas _cv = null;
        Image _img = null;
        TextBox _label = null;
        ScaleTransform _stf = null;
        TranslateTransform _ttf = null;
        Point mouseXY;
        double scale = 1.0d;
        static int MaxSize = 600;
        string _filepath = null;
        bool _modified = false;
        public TagImage(ref TagWindow host) : base(ref host)
        {

        }

        public override bool CanCreateFromFile
        {
            get
            {
                return true;
            }
        }
        public override void OnTransfer(TagWindow new_window)
        {
            base.OnTransfer(new_window);

        }

        public override void OnLoad(string data)
        {
            base.OnLoad(data);
            _filepath = data;
            try
            {
                System.Drawing.Bitmap bi = null;
                bi = new System.Drawing.Bitmap(_filepath);
                BitmapSource bs;
                bs=Imaging.CreateBitmapSourceFromHBitmap(bi.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                InitImage(bs, Path.GetFileNameWithoutExtension(_filepath));
            }
            catch(Exception e)
            {
                Log.ErrorLog("读取图片"+_filepath+"时发生异常");
                Log.ErrorLog("原因：" + e.ToString());
            }

        }
        public override string OnSave()
        {
            string res = _filepath;
            if (res == null || (res!=null && !File.Exists(res)))
            {
                res=_label.Text + ".jpeg";
                BitmapEncoder be = new JpegBitmapEncoder();
                BitmapSource bs = (BitmapSource)_img.Source;
                be.Frames.Add(BitmapFrame.Create(bs));
                FileStream fs = new FileStream(res, FileMode.Create);
                be.Save(fs);
                fs.Close();
            }
            else if(_modified)
            {
                string newfilename = Path.GetDirectoryName(res) + _label.Text + Path.GetExtension(res);
                File.Move(res,newfilename);
                res = newfilename;
            }
            return res;
        }
        public override void OnCreateFromFile(string filedata)
        {
            ReadFromFile(filedata);
        }
        public override void OnCreateNew()
        {
            window.Hide();
            ScreenCapturer sc = new ScreenCapturer(this);
            sc.Show();
            sc.Focus();
        }

        public override string GetName()
        {
            return "Image";
        }

        public override void OnInit()
        {
            base.OnInit();
        }
        public override bool RightClickActive
        {
            get
            {
                return true;
            }
        }
        public void ReadFromScreenShot(BitmapSource bmp)
        {
            IsCreated = true;
            InitImage(bmp,"Temporary File "+ DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
            window.tw_content.Children.Add(_label);
            window.tw_content.Children.Add(_cv);
        }

        private void ReadFromFile(string filepath)
        {
            _filepath = filepath;
            IsCreated = true;
        }

        private void InitImage(BitmapSource bmp, string title)
        {
            _cv = new Canvas();
            _stf = new ScaleTransform();
            _img = new Image();
            _ttf = new TranslateTransform();
            TransformGroup _g = new TransformGroup();
            _g.Children.Add(_stf);
            _g.Children.Add(_ttf);
            _img.RenderTransform = _g;
            _img.MouseWheel += img_MouseWheel;
            _img.MouseMove += img_MouseMove;
            _img.MouseLeftButtonDown += img_MouseLeftButtonDown;
            _img.Source = bmp;
            int height = bmp.PixelHeight;
            int width = bmp.PixelWidth;
            if (height >= width && height > MaxSize)
                scale = (double)MaxSize / height;
            else if (width > height && width > MaxSize)
                scale = (double)MaxSize / width;
            _img.Height = height * scale;
            _img.Width = width * scale;
            _img.Stretch = Stretch.Uniform;
            _label = new TextBox();
            _label.Text = title;
            _label.FontFamily = new FontFamily("Agency FB");
            _label.FontSize = 30;
            _label.Background = null;
            _label.BorderBrush = null;
            _label.Foreground = Brushes.White;
            _label.TextChanged += (o, e) => { _modified = true; };
            _label.TextWrapping = TextWrapping.NoWrap;
            _label.AcceptsReturn = false;
            FormattedText textContext = new FormattedText(title, CultureInfo.CurrentCulture,
                             FlowDirection.LeftToRight,new Typeface("Agency FB"), _label.FontSize, Brushes.Black);
            _label.Width = Math.Max((textContext.Width+5),(_img.Width));
            _label.Height = 50;
            _cv.Height = _img.Height;
            _cv.Width = _img.Width;
            _cv.ClipToBounds = true;
            _cv.Children.Add(_img);
            Canvas.SetTop(_cv, _label.Height);
            if (_img.Width < _label.Width)
                Canvas.SetLeft(_cv, _label.Width/2-_img.Width/2);
            Height = (int)(_label.Height + _img.Height+20);
            Width = (int)(_label.Width+20);
        }

        public override FrameworkElement[] DragMoveInvaild
        {
            get
            {
                if (_cv!=null && _stf!=null && _stf.ScaleX>1)
                    return new FrameworkElement[] { _cv };
                return new FrameworkElement[] { };
            }
        }


        private void img_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point centerPoint = e.GetPosition(_cv);
            _stf.CenterX = centerPoint.X;
            _stf.CenterY = centerPoint.Y;
            if (e.Delta > 0)
            {
                _stf.ScaleX += (double)e.Delta / 2000;
                _stf.ScaleY += (double)e.Delta / 2000;
            }
            else
            {
                var oldsx = _stf.ScaleX;
                var oldsy = _stf.ScaleY;
                _stf.ScaleX += (double)e.Delta / 2000;
                _stf.ScaleY += (double)e.Delta / 2000;
                _ttf.X *= (_stf.ScaleX-1)/oldsx;
                _ttf.Y *= (_stf.ScaleY-1)/oldsy;
            }
            if (_stf.ScaleX < 1) { _stf.ScaleX = 1;_stf.ScaleY = 1; };
        }

        private void img_MouseLeftButtonDown(object sender,MouseButtonEventArgs e)
        {
            mouseXY = e.GetPosition(_cv);
        }
        private void img_MouseMove(object sender, MouseEventArgs e)
        {
            if(e.LeftButton==MouseButtonState.Pressed && _stf.ScaleX>1)
            {
                var position = e.GetPosition(_cv);
                var dx = mouseXY.X - position.X;
                var dy = mouseXY.Y - position.Y;
                Point topleft = _img.TranslatePoint(new Point(0, 0), _cv);
                Point botright = _img.TranslatePoint(new Point(_cv.ActualWidth, _cv.ActualHeight), _cv);
                if (dx < 0)
                {
                    if (dx < topleft.X) dx = topleft.X;
                }
                else
                {
                    if (botright.X-dx <= _cv.ActualWidth)
                        dx = botright.X-_cv.ActualWidth;
                }
                if(dy < 0)
                {
                    if (dy < topleft.Y) dy = topleft.Y;
                }
                else
                {
                    if (botright.Y - dy <= _cv.ActualHeight)
                        dy = botright.Y - _cv.ActualHeight;
                }
                _ttf.X -= dx;
                _ttf.Y -= dy;
                mouseXY = position;
            }
        }

        public override void OnShow()
        {
            base.OnShow();
            if(_label!=null && _cv!=null)
            {
                window.tw_content.Children.Add(_label);
                window.tw_content.Children.Add(_cv);
            }
        }
    }
}
