using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFFloatWin
{
    class TagTimer : TagBase
    {

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
        public override void OnCreateNew()
        {
            base.OnCreateNew();
        }
        public override void OnShow()
        {
            base.OnShow();
        }
        public override void OnInit()
        {
            base.OnInit();
        }
        public override string OnSave()
        {
            return base.OnSave();
        }
        public override void OnLoad(string data)
        {
            base.OnLoad(data);
        }
    }
}
