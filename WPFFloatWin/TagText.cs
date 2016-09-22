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
using System.Text.RegularExpressions;

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
            _text_input.Height = window.tw_content.ActualHeight;
            _text_input.Width = window.tw_content.ActualWidth;
            _text_input.Background = null;
            _text_input.BorderBrush = null;
            TextBlock.SetLineHeight(_text_input, _text_input.Height);
            _text_input.Foreground = Brushes.White;
            _text_input.Text = @_text_data;
            _text_input.TextChanged += (s, e) => { TextInputEvent(); };
            _text_data = "";
            TextInputEvent();
            _text_input.FontFamily= new FontFamily("Agency FB");
            _text_input.VerticalContentAlignment = VerticalAlignment.Top;
            _text_input.HorizontalContentAlignment = HorizontalAlignment.Left;
            _text_input.FontSize = 30;
            _text_input.TextWrapping = TextWrapping.Wrap;
            _text_input.AcceptsReturn = true;
            
        }
        private void TextInputEvent()
        {
            int linecount = _text_data.Count((x) => { return x == '\n'; }) + 1;
            int inputlinecount = _text_input.Text.Count((x) => { return x == '\n'; })+ 1;
            if (linecount != inputlinecount)
            {
                double lineheight = TextBlock.GetLineHeight(_text_input);
                Height = TagWindow.CtrlButtonSize + (inputlinecount - 1) * (int)lineheight;
                _text_input.Height = (inputlinecount) * (int)lineheight;
            }
            _text_data = _text_input.Text;

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
            string result = Regex.Escape(_text_data);
            return result;
        }
        public override void OnLoad(string data)
        {
            _text_data = data;
            IsCreated = true;
        }
        
    }
}
