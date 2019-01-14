using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfAppNotify
{
    /// <summary>
    /// 
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent(); 
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
              Notify.NotifyBox.NotifyCustomCurScr(new UserControl1());

            //Notify.NotifyBox.Notify( $"{DateTime.Now.ToString("HH:mm:ss.fff")} Message Notifition", "Issue titles are like email subject lines. " +
            //    "They tell your collaborators what the issue is about at a glance. For example, the title of this issue is Getting Started with GitHub.",
            //    bd);
            //Notify.NotifyBox.Notify( $"{DateTime.Now.ToString("HH:mm:ss.fff")} Message Notifition",
            //    "Issue titles are like email subject lines. They tell your collaborators what the issue is about at a glance. For example, the title of this issue is Getting Started with GitHub."
            //    ,this);
            // Notify.NotifyBox.Notify($"{DateTime.Now.ToString("HH:mm:ss.fff")} Message Notifition", "Issue titles are like email subject lines. " +
            //     "They tell your collaborators what the issue is about at a glance. For example, the title of this issue is Getting Started with GitHub.");
            // Notify.NotifyBox.NotifyOnCurrentScr($"{DateTime.Now.ToString("HH:mm:ss.fff")} Message Notifition", "Issue titles are like email subject lines. " +
            // "They tell your collaborators what the issue is about at a glance. For example, the title of this issue is Getting Started with GitHub.");

            //  Notify.FullScrBox.Notify("lsjf", "正在登录...",1);

            Notify.FullScrBox.Loading("alsjdflj",bd);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
