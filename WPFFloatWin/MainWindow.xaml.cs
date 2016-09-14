using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Windows.Interop;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace WPFFloatWin
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //全局键盘消息：
        public delegate int HookProc(int nCode, Int32 wParam, IntPtr lParam);
        HookProc KeyboardHookProcedure;
        static int hKeyboardHook = 0;
        bool isExistTagGen = false;
        public bool IsExistTagGen
        {
            set
            {
                isExistTagGen = value;
            }
            get
            {
                return isExistTagGen;
            }
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WM_KEYDOWN = 0x100;
        private const int WM_SYSKEYDOWN = 0x104;
        public const int WH_KEYBOARD_LL = 13;
        [StructLayout(LayoutKind.Sequential)]
        public class KeyboardHookStruct
        {
            public int vkCode; //表示一个在1到254间的虚似键盘码   
            public int scanCode; //表示硬件扫描码   
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
        [StructLayout(LayoutKind.Sequential)]
        //Struct for Marshalling the lParam
        public struct MoveRectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

        }
        private void HookInit()
        {
            if (hKeyboardHook == 0)
            {
                //实例化委托  
                KeyboardHookProcedure = new HookProc(KeyboardHookProc);
                Process curProcess = Process.GetCurrentProcess();
                ProcessModule curModule = curProcess.MainModule;
                hKeyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, KeyboardHookProcedure, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private int KeyboardHookProc(int nCode, Int32 wParam, IntPtr lParam)
        {
            if (!isExistTagGen && nCode >= 0 && wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN)
            {
                KeyboardHookStruct MyKeyboardHookStruct = (KeyboardHookStruct)Marshal.PtrToStructure(lParam, typeof(KeyboardHookStruct));
                System.Windows.Forms.Keys keyData = (System.Windows.Forms.Keys)MyKeyboardHookStruct.vkCode;

                if (keyData == System.Windows.Forms.Keys.Q && (int)System.Windows.Forms.Control.ModifierKeys == (int)System.Windows.Forms.Keys.Control)
                {
                    TagGenerator t = new TagGenerator();
                    t.Show();
                    t.Focus();
                    isExistTagGen = true;
                    return 1;
                }
            }
            return 0;
        }

        List<TagWindow> tagwinlist;
        int ColorThemeIndex = 0;
        DispatcherTimer MainTimer = null;
        DispatcherTimer ArcUpdateTimer = null;
        PerformanceCounter CPUCounter = null;
        PerformanceCounter MemCounter = null;
        PerformanceCounter MemLimCounter = null;
        List<NetworkInterface> NetInterfaces = null;
        long TotalSent = 0;
        long TotalRecv = 0;
        float CPUpercent = 0;
        float dCPUpercent = 0;
        static public SolidColorBrush MakeBrush(byte[] color)
        {
            return new SolidColorBrush(Color.FromRgb(color[0], color[1], color[2]));
        }

        static byte[][][] MyColorThemes =
        {
            //[0]:Arc填充 [1]:Arc背景 [2]:内圆填充 [3]:内圆背景 [4]:字体颜色 [5]:extra
           new byte[][]
           {
               new byte[]  {241, 141, 0},
               new byte[]  {242, 192, 99},
               new byte[]  {231, 76, 60},
               new byte[]  {242, 147, 92},
               new byte[]  {16, 32, 64},
               new byte[]  {0, 0, 0}
           },
           new byte[][]
           {
               new byte[] {98, 90, 5},
               new byte[] {217, 130, 118},
               new byte[] {160, 77, 125},
               new byte[] {181, 94, 154},
               new byte[] {220, 220, 134},
               new byte[] {0, 0, 0}
           },
           new byte[][]
           {
               new byte[] {125,118,0 },
               new byte[] {157,201,42},
               new byte[] {0,136,97},
               new byte[] {181,169,57},
               new byte[] {227,217,99},
               new byte[] { 0,0,0}
           }
        };

        public MainWindow()
        {
            InitializeComponent();
            System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
            Left = scr.Width - Width;
            Top = scr.Height - Height;
            tagwinlist = new List<TagWindow>();
            
        }
        private void CreateTagFromFile()
        {
            using (StreamReader sr = new StreamReader("data.dat"))
            {
                string strLine = string.Empty;
                Regex rex =new Regex(@"^\d+$");
                while (true)
                {
                    strLine = sr.ReadLine();
                    if (strLine == null)
                        break;
                    if (rex.IsMatch(strLine))
                    {
                        int num = Convert.ToInt32(strLine);
                        strLine = sr.ReadLine();
                        var pms = strLine.Split(' ');
                        TagWindow t = CreateTagWindow(Convert.ToInt32(pms[0]), Convert.ToInt32(pms[1]));
                        int focustagindex = Convert.ToInt32(pms[2]);
                        TagBase savedtag = null;
                        for (int i = 0; i < num; ++i)
                        {
                            strLine = sr.ReadLine();
                            var name_data = strLine.Split(new char[] { ' ' }, 2);
                            TagBase tag=CreateAndAddTag(name_data[0], name_data[1],t);
                            if (i == focustagindex)
                                savedtag = tag;
                        }
                        t.Nowdata = savedtag;
                        if (pms[3] == "True")
                        {
                            t.NeedHide = true;
                            t.tw_border.Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
        private void MemInit()
        {
            MemLimCounter = new PerformanceCounter("Memory", "Commit Limit");
            MemCounter = new PerformanceCounter("Memory", "Committed Bytes");
        }

        private void MemHandle()
        {
            int memlim = Convert.ToInt32(MemLimCounter.NextValue() / 1024);
            int mem = Convert.ToInt32(MemCounter.NextValue() / 1024);
            float mempercent = Convert.ToSingle(mem) / memlim;
            this.SemiCircle.Data = Geometry.Parse(CalcArcPoints(new Point(60, 60), 55,mempercent, false));
        }
        private void CPUInit()
        {
            CPUCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }

        private void CPUHandle()
        {
            dCPUpercent =(CPUCounter.NextValue() / 100 - CPUpercent)/20;
        }
        private void NetInit()
        {
            NetInterfaces = new List<NetworkInterface>();
            foreach (NetworkInterface t in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (t.OperationalStatus.ToString() == "Up")
                    if (t.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                        NetInterfaces.Add(t);
            }
        }
        private void NetHandle()
        {
            long sentBytes = 0;
            long recivedBytes = 0;

            foreach (NetworkInterface nic in NetInterfaces)
            {
                IPv4InterfaceStatistics interfaceStats = nic.GetIPv4Statistics();
                sentBytes += interfaceStats.BytesSent;
                recivedBytes += interfaceStats.BytesReceived;
            }

            float sentSpeed = sentBytes - TotalSent;
            float recivedSpeed = recivedBytes - TotalRecv;

            if (TotalSent == 0 && TotalRecv == 0)
            {
                sentSpeed = 0;
                recivedSpeed = 0;
            }
            sentSpeed /= 1024;
            if (sentSpeed > 1024)
            {
                sentSpeed = Convert.ToSingle(Math.Round(sentSpeed / 1024, 2));
                this.UpSpeed.Text = "MB/s";
            }
            else
            {
                sentSpeed =(int)sentSpeed;
                this.UpSpeed.Text = "KB/s";
            }

            recivedSpeed /= 1024;
            if (recivedSpeed > 1024)
            {
                recivedSpeed = Convert.ToSingle(Math.Round(recivedSpeed / 1024,2));
                this.DownSpeed.Text = "MB/s";
            }
            else
            {
                recivedSpeed = (int)recivedSpeed;
                this.DownSpeed.Text = "KB/s";
            }

            this.UpLoad.Text = string.Format("{0,5}", sentSpeed);
            this.DownLoad.Text = string.Format("{0,5}",recivedSpeed);

            TotalSent = sentBytes;
            TotalRecv = recivedBytes;
        }

        private string CalcArcPoints(Point center,double r,float percentage,bool isclosed)
        {
            string result;
            if (percentage == 0)
                return "";
            double dx = Math.Sqrt(r * r * (4 * percentage - 4 * percentage * percentage));
            if (percentage < 0.5)
            {
                double y = center.Y + 2 * r * (0.5-percentage);
                result = string.Format("M {0:f4},{1:f4} A {4},{4} 0 0 0 {2:f4},{1:f4}{3}", center.X - dx, y, center.X + dx, isclosed ? "Z" : "",r);
            }
            else
            {
                double y = center.Y - 2 * r * (percentage-0.5);
                result = string.Format("M {0:f4},{1:f4} A {4},{4} 0 1 0 {2:f4},{1:f4}{3}", center.X - dx, y, center.X + dx, isclosed ? "Z" : "",r);
            }
            return result;
        }

        private void MainTimerHandle(object sender, EventArgs e)
        {
            CPUHandle();
            MemHandle();
            NetHandle();
        }
        private void ArcUpdate(object sender, EventArgs e)
        {
            CPUpercent += dCPUpercent;
            this.ArcPath.Data = Geometry.Parse(CalcArcPoints(new Point(60, 60), 57.5, CPUpercent, false));
        }

        public bool isCollsionWithRect(double x1, double y1, double w1, double h1,
             double x2, double y2, double w2, double h2)
        {
            if (x1 >= x2 && x1 >= x2 + w2)
            {
                return false;
            }
            else if (x1 <= x2 && x1 + w1 <= x2)
            {
                return false;
            }
            else if (y1 >= y2 && y1 >= y2 + h2)
            {
                return false;
            }
            else if (y1 <= y2 && y1 + h1 <= y2)
            {
                return false;
            }
            return true;
        }
        public void MergeTagWindow(TagWindow dragwin)
        {
            double x1 = dragwin.Left, y1 = dragwin.Top, w1 = dragwin.Width, h1 = dragwin.Height;
            for (int i = 0; i < tagwinlist.Count; i++)
            {
                TagWindow temp = tagwinlist[i];
                if (temp != dragwin && isCollsionWithRect(x1, y1, w1, h1, temp.Left, temp.Top, temp.Width, temp.Height))
                {
                    temp.MergeTag(dragwin);
                    break;
                }
            }
        }
        public void DeleteTagWindow(TagWindow closedtag)
        {
            for (int i = 0; i < tagwinlist.Count; i++)
                if (tagwinlist[i] == closedtag)
                {
                    tagwinlist[i].Close();
                    tagwinlist.RemoveAt(i);
                    break;
                }
        }
        public void TagWindowDragOver(TagWindow dragwin)
        {
            double x1 = dragwin.Left, y1 = dragwin.Top, w1 = dragwin.Width, h1 = dragwin.Height;
            bool flag = false;
            for (int i = 0; i < tagwinlist.Count; i++)
            {
                TagWindow temp = tagwinlist[i];
                if (temp != dragwin && isCollsionWithRect(x1, y1, w1, h1, temp.Left, temp.Top, temp.Width, temp.Height))
                {
                    flag = true;
                    break;
                }
            }
            dragwin.OnDragOver(flag);
        }


        public TagWindow CreateTagWindow(int posx = -1, int posy = -1)
        { 
            TagWindow tagw = new TagWindow(posx,posy);
            byte[][] theme = MyColorThemes[ColorThemeIndex];
            tagw.SetColorTheme(theme[0], theme[4], theme[2]);
            tagw.Show();
            tagw.Focus();
            tagwinlist.Add(tagw);
            return tagw;
        }
        public TagBase CreateAndAddTag(string type, string data,TagWindow tagw)
        {
            Type tagtype = TagWindow.functions[type];
            object tag = Activator.CreateInstance(tagtype, new object[] { tagw });
            if (data != "") ((TagBase)tag).OnLoad(data);
            tagw.AddTag((TagBase)tag);
            return (TagBase)tag;
        }
        private void ApplyColorTheme()
        {
            byte[][] theme = MyColorThemes[ColorThemeIndex];
            ArcPath.Stroke = MakeBrush(theme[0]);
            ArcBg.Fill = MakeBrush(theme[1]);
            SemiCircle.Fill = MakeBrush(theme[2]);
            InnerCircle.Fill= MakeBrush(theme[3]);
            foreach (UIElement element in WindowGrid.Children)
            {
                if (element is TextBlock)
                    ((TextBlock)element).Foreground = MakeBrush(theme[4]);
            }
            foreach (var item in tagwinlist)
            {
                item.SetColorTheme(theme[0], theme[4], theme[2]);
                item.ApplyColorTheme();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CPUInit();
            MemInit();
            NetInit();
            HookInit();
            MainTimer = new DispatcherTimer();
            MainTimer.Interval = new TimeSpan(0, 0, 1);
            MainTimer.Tick += new EventHandler(MainTimerHandle);
            MainTimer.Start();
            ArcUpdateTimer = new DispatcherTimer();
            ArcUpdateTimer.Interval = TimeSpan.FromMilliseconds(50);
            ArcUpdateTimer.Tick += new EventHandler(ArcUpdate);
            ArcUpdateTimer.Start();
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndSourceHookHandler);
            ApplyColorTheme();
            CreateTagFromFile();
        }

        private void ChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            ColorThemeIndex++;
            if (ColorThemeIndex >= MyColorThemes.Length)
                ColorThemeIndex = 0;
            ApplyColorTheme();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void SaveTagInfo()
        {
            StreamWriter sw= new StreamWriter("data.dat", false);
            foreach (var item in tagwinlist)
            {
                item.OnSave(sw);
            }
            sw.Close();
        }
        private IntPtr HwndSourceHookHandler(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_MOVING = 0x0216;
            switch (msg)
            {
                case WM_MOVING:
                    {
                        MoveRectangle rectangle = (MoveRectangle)Marshal.PtrToStructure(lParam, typeof(MoveRectangle));
                        System.Drawing.Rectangle scr = System.Windows.Forms.Screen.AllScreens[0].WorkingArea;
                        if (rectangle.Right>scr.Width)
                        {
                            rectangle.Left = scr.Width-(int)Width;
                            rectangle.Right = scr.Width;
                        }
                        else if(rectangle.Left<0)
                        {
                            rectangle.Left = 0;
                            rectangle.Right = (int)Width;
                        }
                        if(rectangle.Bottom>scr.Height)
                        {
                            rectangle.Top = scr.Height - (int)Height;
                            rectangle.Bottom = scr.Height;
                        }
                        else if(rectangle.Top<0)
                        {
                            rectangle.Top = 0;
                            rectangle.Bottom = (int)Height;
                        }
                        Marshal.StructureToPtr(rectangle, lParam, true);
                        break;
                    }
            }
            return IntPtr.Zero;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            bool retKeyboard = true;

            if (hKeyboardHook != 0)
            {
                retKeyboard = UnhookWindowsHookEx(hKeyboardHook);
                hKeyboardHook = 0;
            }
            //如果卸下钩子失败   
            if (!(retKeyboard)) throw new Exception("卸下钩子失败！");

            SaveTagInfo();
        }
    }
}
