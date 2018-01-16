using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace WPFFloatWin
{
    public static class Log
    {
        private static StreamWriter sw = new StreamWriter("Error.log", true);
        private static int count = 0;
        private const int BuffMax = 3;
        public static void ErrorLog(string data)
        {
            if (sw != null)
            {
                sw.WriteLine(DateTime.Now.ToString() + " " + data);
                count++;
                if(count==BuffMax)
                {
                    sw.Flush();
                    count = 0;
                }
            }
        }
        public static void CloseErrorLog()
        {
            if (sw != null)
            {
                sw.Close();
                sw = null;
            }
        }
    }
}
