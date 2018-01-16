using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WPFFloatWin
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {

        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            Log.ErrorLog("程序非正常退出！");
            Log.ErrorLog("原因：" + e.ReasonSessionEnding.ToString());
            ((MainWindow)Current.MainWindow).SaveTagInfo();
            Log.CloseErrorLog();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.ErrorLog("程序异常！");
            Log.ErrorLog("原因：" + e.Exception.ToString());
            Log.ErrorLog("对象：" + sender.ToString());
            ((MainWindow)Current.MainWindow).SaveTagInfo();
            Log.CloseErrorLog();
        }
    }
}
