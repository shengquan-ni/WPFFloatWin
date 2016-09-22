﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

namespace WPFFloatWin
{
    /// <summary>
    /// Tag.xaml 的交互逻辑
    /// </summary>
    public partial class TagWindow : Window
    {
        public struct POINT
        {
            public int X;
            public int Y;
            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        public static Dictionary<string, Type> functions = new Dictionary<string, Type> { { "Text", typeof(TagText) } };
        private static int WindowWidth = 351;
        private static int WindowHeight = 50;
        private static int ShowHideAnimationDuration = 300;
        public static int CtrlButtonSize = 50;
        private double _ContentWidth=300;
        private double _ContentHeight=50;
        public double WinHeight
        {
            set
            {
                if (value > _ContentHeight)
                    Height = value;
                else if (!IsCtrlBtnFolded)
                    Height = Math.Max((data.Count + 1) * CtrlButtonSize, _ContentHeight);
                else
                    Height = _ContentHeight;
            }
            get
            {
                return Height;
            }
        }
        public double WinWidth
        {
            set
            {
                if (value > _ContentWidth)
                    Width = value;
            }
            get
            {
                return Width;
            }
        }

        public double ContentWidth
        {
            set
            {
                if (Width < value + MinWidth)
                    ChangeSize(value + MinWidth, -1);
                ChangeBorderMargin(value, -1);
                _ContentWidth = value+MinWidth;
            }
            get
            {
                return _ContentWidth;
            }
        }
        public double ContentHeight
        {
            set
            {
                if (Height < value)
                    ChangeSize(-1, value);
                ChangeBorderMargin(-1, value);
                _ContentHeight = value;
            }
            get
            {
                return _ContentHeight;
            }
        }

        byte[] BorderColor = { 0, 0, 0 };
        byte[] FontColor = { 255, 255, 255 };
        byte[] SelectColor = { 255, 0, 0 };
        public bool NeedHide = false;
        bool IsPlaying = false;
        bool ShowedWindow = false;
        bool FoldFlag = false;
        BackgroundWorker backgroundWorker=null;
        List<TagBase> data;
         System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
        TagBase nowdata=null;
        public TagBase Nowdata
        {
            set
            {
                nowdata = value;
            }
        }
        bool DragFlag = false;
        bool IsCtrlBtnFolded = true;
        [StructLayout(LayoutKind.Sequential)]
        //Struct for Marshalling the lParam
        public struct MoveRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

        }
        
        public TagWindow(int posx,int posy)
        {
            InitializeComponent();
            data = new List<TagBase>();
            if (posx != -1 && posy != -1)
            {
                Left = posx;
                Top = posy;
                WindowStartupLocation = WindowStartupLocation.Manual;
            }
        }
        public void SetColorTheme(byte[] bordercolor, byte[] fontcolor, byte[] selectcolor)
        {
            BorderColor = bordercolor;
            FontColor = fontcolor;
            SelectColor = selectcolor;
        }
        public void ApplyColorTheme()
        {
            tw_border.Background = MainWindow.MakeBrush(BorderColor);
            Brush fc = MainWindow.MakeBrush(FontColor);
            Brush sc = MainWindow.MakeBrush(SelectColor);
            foreach (var item in tw_canvas.Children)
            {
                if (item is System.Windows.Shapes.Path)
                    ((System.Windows.Shapes.Path)item).Stroke = fc;
            }
            tw_ctrlbutton.Foreground = fc;
            tw_ctrlbutton.Background = sc;
            foreach (var item in data)
            {
                if (item.cv != null)
                {
                    
                    foreach (var cvitem in item.cv.Children)
                    {
                        if (cvitem is System.Windows.Shapes.Path)
                            ((System.Windows.Shapes.Path)cvitem).Stroke = fc;
                        else
                        {
                            ((Button)cvitem).Foreground = fc;
                            if (item == nowdata) ((Button)cvitem).Background = sc;
                        }
                    }
                }
            }
        }


        private void UpdateAll(TagBase newtag)
        {
            UpdateWindow(newtag);
            UpdateCtrlBtnContent();
        }
        public void ClearContent()
        {
            tw_content.Children.Clear();
            ContentWidth = 300;
            ContentHeight = 50;
            WinHeight = WindowHeight;
            WinWidth = WindowWidth;
        }
        public void UpdateWindow(TagBase newtag)
        {
            if (nowdata != null)
                ClearContent();
            nowdata = newtag;
            if (nowdata.IsCreated)
                nowdata.OnShow();
            else
            {
                tw_content.IsHitTestVisible = true;
                nowdata.CreateButtonShow();
            }
        }
        private void UpdateCtrlBtnContent()
        {
            if (data.Count == 1)
            {
                tw_ctrlbutton.FontSize = 18;
                tw_ctrlbutton.Content = nowdata.GetName();
            }
            else
            {
                tw_ctrlbutton.FontSize = 30;
                tw_ctrlbutton.Content = data.Count.ToString();
            }
        }
        public bool AddTag(TagBase tag)
        {
            try
            {
                tag.OnInit();
                data.Add(tag);
                UpdateAll(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void FoldWindow()
        {
            tw_border.Visibility = Visibility.Collapsed;
            tw_grid.Opacity = 0.5;
            FoldFlag = true;
        }

        private void UnFoldWindow()
        {
            tw_border.Visibility = Visibility.Visible;
            tw_grid.Opacity = 1;
            tw_canvas.Margin = new Thickness(0, 0, 0, 0);
            FoldFlag = false;

        }

        private void SetWindowPosToMousePos()
        {
            Point pt=Mouse.GetPosition(this);
            double half_canvas_width = tw_canvas.Width * 0.5;
            if (pt.X + tw_canvas.Width > Width)
                pt.X = Width - half_canvas_width;
            tw_canvas.Margin = new Thickness(pt.X- half_canvas_width, 0, Width- half_canvas_width - pt.X,0);
        }

        public void OnDragOver(bool flag)
        {
            if(flag)
            {
                if (flag != DragFlag)
                {
                    DragFlag = flag;
                    FoldWindow();
                }
                SetWindowPosToMousePos();
            }
            else
            {
                if (flag != DragFlag)
                {
                    DragFlag = flag;
                    UnFoldWindow();
                }
            }
        }
       
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsPlaying)
            {
                base.DragMove();
                ((MainWindow)Application.Current.MainWindow).MergeTagWindow(this);
            }
        }
        private void ShowBorder()
        {
            if(!IsPlaying)
            {
                Width = ContentWidth;
                Height = ContentHeight;
                IsPlaying = true;
                ShowedWindow = true;
                tw_border.Visibility = Visibility.Visible;
               
                AnimationWithLambda(LeftProperty, scr.Width - Width, new Duration(TimeSpan.FromMilliseconds(ShowHideAnimationDuration)), (s, e) => { Left = scr.Width - Width; IsPlaying = false; });
            }
        }
        public void HideBorder()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                AnimationWithLambda(LeftProperty, scr.Width - MinWidth, new Duration(TimeSpan.FromMilliseconds(ShowHideAnimationDuration)), (s, e) => { Left = scr.Width - MinWidth; Width = MinWidth; Height = MinHeight; tw_border.Visibility = Visibility.Collapsed; ShowedWindow = false; IsPlaying = false; });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyColorTheme();
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndSourceHookHandler);
        }
        private IntPtr HwndSourceHookHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_MOVING = 0x0216;

            switch (msg)
            {
                case WM_MOVING:
                    {
                        MoveRectangle rectangle = (MoveRectangle)Marshal.PtrToStructure(lParam, typeof(MoveRectangle));
                        if (!FoldFlag)
                        {
                            if (rectangle.Right > scr.Width - 5)
                            {
                                NeedHide = true;
                                rectangle.Left = scr.Width - (int)Width;
                                rectangle.Right = scr.Width;
                            }
                            else
                            {
                                NeedHide = false;
                                if (rectangle.Left < 0)
                                {
                                    rectangle.Left = 0;
                                    rectangle.Right = (int)Width;
                                }
                            }
                            if (rectangle.Bottom > scr.Height)
                            {
                                rectangle.Top = scr.Height - (int)Height;
                                rectangle.Bottom = scr.Height;
                            }
                            else if (rectangle.Top < 0)
                            {
                                rectangle.Top = 0;
                                rectangle.Bottom = (int)Height;
                            }
                        }
                        Marshal.StructureToPtr(rectangle, lParam, true);
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        private void tw_ctrlbutton_Click(object sender, RoutedEventArgs e)
        {
            if (data.Count > 1)
            {
                if (IsCtrlBtnFolded)
                    UnFoldCtrlButton();
                else
                    FoldCtrlButton();
            }
        }
        private System.Windows.Shapes.Path CreatePath(string data)
        {
            System.Windows.Shapes.Path p = new System.Windows.Shapes.Path();
            p.Data = Geometry.Parse(data);
            p.Stroke = MainWindow.MakeBrush(FontColor);
            p.StrokeThickness = 1;
            return p;
        }

        private void ExchangeData(TagBase newtag)
        {
            foreach (var item in nowdata.cv.Children)
            {
                if (item is Button)
                    ((Button)item).Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            }
            foreach (var item in newtag.cv.Children)
            {
                if(item is Button)
                    ((Button)item).Background=MainWindow.MakeBrush(SelectColor);
            }
            UpdateWindow(newtag);
        }

        private void CtrlButtonRemoveData(TagBase tag)
        {
            tag.OnExit();
            data.Remove(tag);
            tw_grid.Children.Remove(tag.cv);
            if (data.Count == 1)
            {
                IsCtrlBtnFolded = true;
                FoldCtrlButton();
            }
            else
            {
                if (tag == nowdata)
                    ExchangeData(data.Find(x => { return x != tag; }));
                UpdateRowConfig();
            }
            UpdateCtrlBtnContent();
        }
        private void UpdateRowConfig()
        {
            for (int i = 0; i < data.Count; ++i)
                Grid.SetRow(data[i].cv, i+1);
        }
        private Canvas CreateButton(TagBase tag)
        {
            Canvas cv = new Canvas();
            cv.Height = CtrlButtonSize;
            cv.Width = CtrlButtonSize;
            cv.Background = new SolidColorBrush(Color.FromArgb(1,0,0,0));
            cv.Children.Add(CreatePath("M 1,10 L 1,1 L 10,1"));
            cv.Children.Add(CreatePath("M 40,1 L 49,1 L 49,10"));
            cv.Children.Add(CreatePath("M 49,40 L 49,49 L 40,49"));
            cv.Children.Add(CreatePath("M 1,40 L 1,49 L 10,49"));
            Button btn = new Button();
            btn.Style = FindResource("btnstyle") as Style;
            btn.FontFamily = new FontFamily("Agency FB");
            btn.FontSize = 18;
            btn.Width = CtrlButtonSize-4;
            btn.Height = CtrlButtonSize-4;
            btn.Margin = new Thickness(2, 2, 0, 0);
            btn.FontWeight = FontWeights.Bold;
            btn.Foreground = MainWindow.MakeBrush(FontColor);
            if(tag==nowdata)
                btn.Background=MainWindow.MakeBrush(SelectColor);
            btn.BorderBrush = null;
            btn.Content = tag.GetName();
            btn.Click += (s, e) => { ExchangeData(tag);  };
            btn.MouseRightButtonUp += (s, e) => { CtrlButtonRemoveData(tag); };
            cv.Children.Add(btn);
            return cv;
        }
        private void GridAddButton(TagBase tag)
        {
            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(CtrlButtonSize);
            tw_grid.RowDefinitions.Add(rd);
            Canvas cv = CreateButton(tag);
            tag.cv = cv;
            tw_grid.Children.Add(cv);
            Grid.SetRow(cv, tw_grid.RowDefinitions.Count-1);
        }
        public void AnimationWithLambda(DependencyProperty p,double new_size,Duration duration,EventHandler func=null)
        {    
            DoubleAnimation a = new DoubleAnimation(new_size,duration,FillBehavior.Stop);
            if(func!=null)a.Completed += func;
            BeginAnimation(p, a);
        }

        public void ChangeSize(double new_width=-1,double new_height=-1)
        {
            if (!IsLoaded) return;
            if (new_width != -1)
                Width = new_width;
            if (new_height != -1)
                Height = new_height;
        }
        private void ChangeBorderMargin(double new_width=-1,double new_height=-1)
        {
            if (new_width != -1 && new_height == -1)
                tw_border.Margin = new Thickness(1, 0, 1-new_width, tw_border.Margin.Bottom);
            else if (new_width == -1 && new_height != -1)
                tw_border.Margin = new Thickness(1, 0, tw_border.Margin.Right, CtrlButtonSize-new_height);
            else
                tw_border.Margin = new Thickness(1, 0, 1-new_width, CtrlButtonSize-new_height);
        }
        private void UnFoldCtrlButton()
        {
            IsCtrlBtnFolded = false;
            WinHeight = (data.Count + 1) * CtrlButtonSize;
            for(int i=0;i<data.Count;++i)
                   GridAddButton(data[i]);
        }
        private void FoldCtrlButton()
        {
            IsCtrlBtnFolded = true;
            for (int i = 0; i < data.Count; ++i)
            {
                tw_grid.Children.Remove(data[i].cv);
                data[i].cv = null;
            }
            tw_grid.RowDefinitions.RemoveRange(1, tw_grid.RowDefinitions.Count - 1);
            WinHeight = CtrlButtonSize;
        }
        public void MergeTag(TagWindow another)
        {
            int precount = data.Count;
            another.ClearContent();
            data.AddRange(another.data);
            for (int i = precount; i < data.Count; ++i)
                data[i].Window = this;
            if (!IsCtrlBtnFolded)
            {
                WinHeight = Height+(data.Count - precount) * CtrlButtonSize;
                for (int i = precount; i < data.Count; ++i)
                    GridAddButton(data[i]);
            }
            UpdateCtrlBtnContent();
            ((MainWindow)Application.Current.MainWindow).DeleteTagWindow(another);
        }

        private void Window_DragLeave(object sender, DragEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine(e.Data);
        }

        private void Window_Drop(object sender, DragEventArgs e)
        {
            Console.WriteLine(e.Data);
           
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
                ((MainWindow)Application.Current.MainWindow).TagWindowDragOver(this);
        }

        private void tw_ctrlbutton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (data.Count == 1)
            {
                nowdata.OnExit();
                ((MainWindow)Application.Current.MainWindow).DeleteTagWindow(this);
            }
        }

        public void OnSave(StreamWriter sw)
        {
            sw.WriteLine(data.Count.ToString());
            int index = 0;
            for (int i = 0; i < data.Count; ++i)
                if (nowdata == data[i])
                {
                    index = i;
                    break;
                }
            sw.WriteLine(Left.ToString() + " " + Top.ToString() + " " + index.ToString()+" "+NeedHide.ToString());
            foreach (var item in data)
            {
                sw.WriteLine(item.GetName() + " " + item.OnSave());
            }
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (NeedHide && backgroundWorker==null)
            {
                double top = Top, height = Height, width = Width;
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += Check_MouseLeave;
                backgroundWorker.RunWorkerCompleted += Finish_MouseLeave;
                backgroundWorker.RunWorkerAsync(new double[] { top, height,width});
            }
        }
        private void Check_MouseLeave(object sender,DoWorkEventArgs e)
        {
            double top = ((double[])(e.Argument))[0];
            double height=((double[])(e.Argument))[1];
            double width = ((double[])(e.Argument))[2];
            while (true)
            {
                if (!IsPlaying)
                {
                    System.Threading.Thread.Sleep(100);
                    POINT mp = new POINT();
                    GetCursorPos(out mp);
                    if ((mp.Y < top-15 || mp.Y > top+height+15 || mp.X < scr.Width-width-15))
                        return;
                }
            }
        }
        private void Finish_MouseLeave(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ShowedWindow && NeedHide)
            {
                if (!IsCtrlBtnFolded)
                    FoldCtrlButton();
                HideBorder();
                tw_focus_sub.Focus();
            }
            backgroundWorker = null;
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded)
            {
                if (!NeedHide)
                {
                    if (Left + Width > scr.Width)
                        Left = scr.Width - Width;
                    else if (Left < 0)
                        Left = 0;
                    if (Top + Height > scr.Height)
                        Top = scr.Height - Height;
                    else if (Top < 0)
                        Top = 0;
                }
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (NeedHide && !ShowedWindow)
                ShowBorder();
        }

        private void tw_border_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (nowdata.IsCreated)
                tw_content.IsHitTestVisible = !tw_content.IsHitTestVisible;
        }

        private void tw_content_LostFocus(object sender, RoutedEventArgs e)
        {
            if(nowdata.IsCreated)
                tw_content.IsHitTestVisible = false;
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            tw_focus_sub.Focus();
        }
    }
}
