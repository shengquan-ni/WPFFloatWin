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
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPFFloatWin
{
    class TagText:TagBase
    {
        string _text_data;
        TextBox _text_input;
        public TagText(ref TagWindow host):base(ref host)
        {

        }
        public override string GetName()
        {
            return "Text";
        }

        public override bool CanCreateFromFile()
        {
            return true;
        }
        public override bool CanCreateNewFile()
        {
            return true;
        }
        public override void OnCreateNew()
        {
            _text_data = "";
            IsCreated = true;
        }
        public override void OnInit()
        {
            base.OnInit();
            _text_input = new TextBox();
            _text_input.Width = window.tw_content.Width;
            _text_input.Height = window.tw_content.Height;
            _text_input.Background = null;
            _text_input.BorderBrush = null;
            _text_input.Foreground = Brushes.White;
            _text_input.Text = _text_data;
            _text_input.FontFamily= new FontFamily("Agency FB");
            _text_input.VerticalContentAlignment = VerticalAlignment.Center;
            _text_input.HorizontalContentAlignment = HorizontalAlignment.Center;
            _text_input.FontSize = 24;
        }
        public override void OnShow()
        {
            base.OnShow();
            window.tw_content.Children.Add(_text_input);
        }
        public override void OnExit()
        {

        }
        public override string OnSave()
        {
            return _text_data;
        }
        public override void OnLoad(string data)
        {
            _text_data = data;
            IsCreated = true;
        }
    }
}
