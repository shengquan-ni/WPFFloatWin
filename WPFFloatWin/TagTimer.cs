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
using System.Windows.Threading;
using System.Threading;
using System.ComponentModel;

namespace WPFFloatWin
{
    class TagTimer : TagBase
    {
        DateTime _dt;
        System.Timers.Timer _timer=null;
        Label _year;
        Label _year_text;
        Label _month;
        Label _month_text;
        Label _day;
        Label _day_text;
        Label _hour;
        Label _hour_text;
        Label _minute;
        Label _minute_text;

        public override Type[] CollideWith
        {
            get
            {
                return new Type[] {typeof(TagTimer) };
            }
        }

        public TagTimer(ref TagWindow host) : base(ref host)
        {
            
        }
        public override string GetName()
        {
            return "Timer";
        }
        public override bool CanCreateFromFile
        {
            get { return false; }
        }
        public override bool CanCreateNewFile
        {
            get { return true; }
        }
        public override bool RightClickActive
        {
            get { return true; }
        }
        private int GetInterval(DateTime target_time)
        {
            TimeSpan t = target_time - DateTime.Now;
            if (t <= TimeSpan.FromSeconds(1))
                return 0;
            else if (t <= TimeSpan.FromMinutes(1))
                return 1000;
            else if (t <= TimeSpan.FromMinutes(15))
                return 60000;
            else if (t <= TimeSpan.FromMinutes(30))
                return 900000;
            else if (t <= TimeSpan.FromHours(1))
                return 1800000;
            else if (t <= TimeSpan.FromHours(2))
                return 3600000;
            else
                return 7200000;
        }


        public override void OnCreateNew()
        {
            base.OnCreateNew();
            _dt = DateTime.Now;
            UpdateLabels();
        }
        private void UpdateLabels()
        {
            _year.Content = string.Format("{0:D4}", _dt.Year);
            _month.Content = string.Format("{0:D2}", _dt.Month);
            _day.Content = string.Format("{0:D2}", _dt.Day);
            _hour.Content = string.Format("{0:D2}", _dt.Hour);
            _minute.Content = string.Format("{0:D2}", _dt.Minute);
        }
        public override void OnShow()
        {
            base.OnShow();
            Add_Label_To_Content(_year, _year_text, _month, _month_text, _day, _day_text, _hour, _hour_text, _minute, _minute_text);
        }
        private void Add_Label_To_Content(params Label[] p)
        {
            foreach (var item in p)
            {
                window.tw_content.Children.Add(item);
            }
        }
        private void Label_SetPosition(params Label[] p)
        {
            double x = 0;
            foreach (var item in p)
            {
                PositionLabel(item, x, 0);
                x += item.Width;
            }
        }
        private void MouseWheelEvent()
        {
            _year.MouseWheel += (s, e) => {  _dt = _dt.AddYears(e.Delta > 0 ? 1 : -1); UpdateLabels(); };
            _month.MouseWheel += (s, e) => { _dt = _dt.AddMonths(e.Delta > 0 ? 1 : -1); UpdateLabels(); };
            _day.MouseWheel += (s, e) => { _dt = _dt.AddDays(e.Delta > 0 ? 1 : -1); UpdateLabels(); };
            _hour.MouseWheel += (s, e) => { _dt = _dt.AddHours(e.Delta > 0 ? 1 : -1); UpdateLabels(); };
            _minute.MouseWheel += (s, e) => { _dt = _dt.AddMinutes(e.Delta > 0 ? 1 : -1); UpdateLabels(); };
        }
        private void PositionLabel(Label l,double posx, double posy)
        {
            Canvas.SetLeft(l, posx);
            Canvas.SetTop(l, posy);
        }
        private void InitLabel(ref Label l,double width=-1,double height=-1,string content="")
        {
            l = new Label();
            if(width!=-1)
                l.Width = width;
            if(height!=-1)
                l.Height = height;
            l.FontFamily = new FontFamily("Agency FB");
            l.Foreground = Brushes.White;
            l.Background = null;
            l.BorderBrush = null;
            l.Padding = new Thickness();
            if (content != "")
            {
                l.FontSize = 18;
                l.Content = content;
                l.VerticalContentAlignment = VerticalAlignment.Bottom;
            }
            else
            {
                l.FontSize = 30;
                l.HorizontalContentAlignment = HorizontalAlignment.Center;
                l.VerticalContentAlignment = VerticalAlignment.Center;
            }
        }

        public override void OnInit()
        {
            base.OnInit();
            Width = 300;
            Height = 50;
            double height = window.tw_content.ActualHeight;
            InitLabel(ref _year, 60, height);
            InitLabel(ref _year_text, 20, height, "年");
            InitLabel(ref _month, 30, height);
            InitLabel(ref _month_text, 20, height, "月");
            InitLabel(ref _day, 30, height);
            InitLabel(ref _day_text, 20, height, "日");
            InitLabel(ref _hour, 30, height);
            InitLabel(ref _hour_text, 20, height, "时");
            InitLabel(ref _minute, 30, height);
            InitLabel(ref _minute_text, 20, height, "分");
            MouseWheelEvent();
            Label_SetPosition(_year, _year_text, _month, _month_text, _day, _day_text, _hour, _hour_text, _minute, _minute_text);
            UpdateLabels();
        }
        public override string OnSave()
        {
            return _dt.ToString();
        }
        public override void OnLoad(string data)
        {
            base.OnLoad(data);
            _dt=Convert.ToDateTime(data);
            BuildTimer();
        }
        public override void OnExit()
        {
            base.OnExit();
            window.WarningCancel();

        }
        private void TimerCallBack()
        {
            window.Dispatcher.BeginInvoke(new Action(() =>
            {
                window.WarningStart();
            }), null); 
        }
        private void BuildTimer()
        {
            if (!IsCreated) return;
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Close();
                _timer = null;
            }
            if (_dt.Second != 0)
                _dt = _dt.AddSeconds(-_dt.Second);
            int interval = GetInterval(_dt);
            if (interval == 0)
                TimerCallBack();
            else
            {
                _timer = new System.Timers.Timer(interval);
                _timer.Elapsed += (s, e) =>
                {
                    int i = GetInterval(_dt);
                    if (i == 0)
                    {
                        TimerCallBack();
                        _timer.Stop();
                    }
                    else
                        _timer.Interval = i;
                };
                _timer.Start();
            }
        }
        public override void OnLostFocus()
        {
            base.OnLostFocus();
            window.WarningCancel();
            BuildTimer();

        }
        public override void OnTransfer(TagWindow new_window)
        {
            window.WarningCancel();
            base.OnTransfer(new_window);
            BuildTimer();
        }


        public override void OnPowerModeChange()
        {
            BuildTimer();
        }
    }
}
