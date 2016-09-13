using System;
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

namespace WPFFloatWin
{
    /// <summary>
    /// Tag.xaml 的交互逻辑
    /// </summary>
    public partial class TagWindow : Window
    {
        public static Dictionary<string, Type> functions = new Dictionary<string, Type> { { "Text", typeof(TagText) } };
        private static int BorderWidth = 300;
        private static int BorderHeight = 50;
        private static int ContentCanvasWidth = 280;
        private static int ContentCanvasHeight = 40;
        public bool NeedHide = false;
        bool IsPlaying = false;
        bool FoldFlag = false;
        List<TagBase> data;
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
        private void UpdateAll(TagBase newtag)
        {
            UpdateWindow(newtag);
            UpdateCtrlBtnContent();
        }
        public void ClearContent()
        {
            tw_content.Children.Clear();
            tw_border.Width = BorderWidth;
            tw_border.Height = BorderHeight;
            tw_content.Width = ContentCanvasWidth;
            tw_content.Height = ContentCanvasHeight;
        }
        public void UpdateWindow(TagBase newtag)
        {
            if (nowdata != null)
                ClearContent();
            nowdata = newtag;
            if (nowdata.IsCreated)
                nowdata.OnShow();
            else
                nowdata.CreateButtonShow();
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
            base.DragMove();
            ((MainWindow)Application.Current.MainWindow).MergeTagWindow(this);
        }
        private void ShowBorder()
        {
            if(!IsPlaying)
            {
                IsPlaying = true;
                tw_border.Visibility = Visibility.Visible;
                System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                DoubleAnimation a = new DoubleAnimation(Left, scr.Width - Width, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.Stop);
                a.Completed += (s, e) => { Left = scr.Width - Width; IsPlaying = false; };
                BeginAnimation(LeftProperty, a);
            }
        }
        public void HideBorder()
        {
            if (!IsPlaying)
            {
                IsPlaying = true;
                System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                DoubleAnimation a = new DoubleAnimation(Left, scr.Width - 50, new Duration(TimeSpan.FromMilliseconds(300)), FillBehavior.Stop);
                a.Completed += (s, e) => { Left = scr.Width - 50; tw_border.Visibility = Visibility.Collapsed; IsPlaying = false; };
                BeginAnimation(LeftProperty, a);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
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
                            System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
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
            p.Stroke = Brushes.White;
            p.StrokeThickness = 1;
            return p;
        }

        private void ExchangeData(TagBase newtag)
        {
            nowdata.cv.Background = null;
            newtag.cv.Background = Brushes.Red;
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
            cv.Height = 50;
            cv.Width = 50;
            cv.Children.Add(CreatePath("M 1,10 L 1,1 L 10,1"));
            cv.Children.Add(CreatePath("M 40,1 L 49,1 L 49,10"));
            cv.Children.Add(CreatePath("M 49,40 L 49,49 L 40,49"));
            cv.Children.Add(CreatePath("M 1,40 L 1,49 L 10,49"));
            if (tag == nowdata)
                cv.Background = Brushes.Red;
            Button btn = new Button();
            btn.Style = FindResource("btnstyle") as Style;
            btn.FontFamily = new FontFamily("Agency FB");
            btn.FontSize = 18;
            btn.Width = 50;
            btn.Height = 50;
            btn.FontWeight = FontWeights.Bold;
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
            rd.Height = new GridLength(50);
            tw_grid.RowDefinitions.Add(rd);
            Canvas cv = CreateButton(tag);
            tag.cv = cv;
            tw_grid.Children.Add(cv);
            Grid.SetRow(cv, tw_grid.RowDefinitions.Count-1);
        }
        private void UnFoldCtrlButton()
        {
            IsCtrlBtnFolded = false;
            Height += (data.Count) * 50;
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
            Height = 50;
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
                Height += (data.Count - precount) * 50;
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
            if(IsLoaded)
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
            if(NeedHide)
            {
                if (!IsCtrlBtnFolded)
                    FoldCtrlButton();
                HideBorder();
            }
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if(NeedHide)
                ShowBorder();
        }
    }
}
