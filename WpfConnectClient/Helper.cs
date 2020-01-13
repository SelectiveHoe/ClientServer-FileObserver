using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfConnectClient
{
    public static class Helper
    {
        public static void Set(this UIElement control, Action action)
        {
            try
            {
                if (Application.Current.Dispatcher.CheckAccess()) // вызов в том же потоке
                {
                    action();// вызов в том же потоке
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, action);
                }
            }
            catch { }           
        }
    }
}
