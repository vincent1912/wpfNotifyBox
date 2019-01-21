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
using System.Windows.Shell;

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
            //  Notify.NotifyBox.NotifyCustomCurScr(new UserControl1());

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

           // Notify.FullScrBox.Loading("alsjdflj",bd);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        { 
            if (e.LeftButton == MouseButtonState.Pressed)
            this.DragMove(); 
        }

        private void BtnMsgRBElement_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupNotify.Notify("message", "message title", this.bd);
        }

        private void BtnMsgRBWindow_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupNotify.Notify("message", "message title", this);

        }

        private void BtnMsgRBScr_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupNotify.Notify("message", "message title", -1);

        }

        private void BtnMsgScrElement_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Notify("message title", "message", bd);

            double dd = bd.ActualWidth;
        }

        private void BtnMsgScrWindow_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Notify("message title", "message", this);

        }

        private void BtnMsgScrScreen_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Notify("message title", "message", -1);

        }

        private void BtnLoadingElement_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Loading("loading", this.bd);

            Task.Run(() => 
            {
                System.Threading.Thread.Sleep(5000);
                Notification.Wpf.PopupMaskNotify.CloseLoading();
            });
        }

        private void BtnLoadingWindow_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Loading("loading", this);
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);
                Notification.Wpf.PopupMaskNotify.CloseLoading();
            });

        }

        private void BtnLoadingScreen_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.PopupMaskNotify.Loading("loading",1);
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(5000);
                Notification.Wpf.PopupMaskNotify.CloseLoading();
            });

        }

        private void BtnMaskElement_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.MaskNotify.Notify("遮罩消息通知", bd);
        }

        Notification.Wpf.MessageBoxDisplayMode GetMessageBoxDisplayModeSelected()
        {
            if (rbtnNormal.IsChecked.HasValue && rbtnNormal.IsChecked.Value)
            {
                return Notification.Wpf.MessageBoxDisplayMode.Normal;
            }
            else if (rbtnScr1.IsChecked.HasValue && rbtnScr1.IsChecked.Value)
            {
                return Notification.Wpf.MessageBoxDisplayMode.MaskCenter;
            }
            else
            {
                return Notification.Wpf.MessageBoxDisplayMode.MaskHor;
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.MessageBox.OK("提示","OK",GetMessageBoxDisplayModeSelected());
        }
        private void BtnOKCancel_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.MessageBox.OKCancel("提示", "OKCancel", GetMessageBoxDisplayModeSelected());

        }

        private void BtnYesNo_Click(object sender, RoutedEventArgs e)
        {
            if( Notification.Wpf.MessageBox.YesNo("询问", "YesNo", GetMessageBoxDisplayModeSelected()) == MessageBoxResult.Yes)
            {

            }

        }

        private void BtnYesNoCancel_Click(object sender, RoutedEventArgs e)
        {
            if(Notification.Wpf.MessageBox.YesNoCancel("询问", "YesNoCancel", GetMessageBoxDisplayModeSelected()) == MessageBoxResult.Yes)
            {

            }

        }

        private void BtnMaskElementLoading_Click(object sender, RoutedEventArgs e)
        {
            btnMaskElementLoading.Tag = Notification.Wpf.MaskNotify.Loading("遮罩等待消息", bd);

        }

        private void BtnMaskElementCloseLoading_Click(object sender, RoutedEventArgs e)
        {
            Notification.Wpf.MaskNotify.CloseLoading(btnMaskElementLoading.Tag.ToString());

        }
    }
}
