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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Notification.Wpf
{
    /// <summary>
    /// MessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            InitializeComponent();
            this.DataContext = this;
            this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            this.MouseDown += (s, e) => 
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            };
        }

        public MessageBoxResult MessageBoxResult { get; set; }

        public object Message { get; set; }

        public MessageBoxButton MessageBoxButton { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
         
        void UpdateBoxStyle( MessageBoxDisplayMode mode)
        {
            switch (mode)
            {
                case MessageBoxDisplayMode.Normal:
                    this.Style = this.Resources["msgBoxStyleNormal"] as Style;
                    break;
                case MessageBoxDisplayMode.MaskCenter:
                    this.Style = this.Resources["msgBoxStyleFullScr"] as Style;
                    break;
                case MessageBoxDisplayMode.MaskHor:
                    this.Style = this.Resources["msgBoxStyleFullScrCross"] as Style;
                    break;
                default:
                    break;
            }
        }

        public static void OK(string title, string msg,MessageBoxDisplayMode mode = MessageBoxDisplayMode.MaskHor, Window owner = null)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                MessageBox messageBox = new MessageBox();
                messageBox.Opacity = 0;
                messageBox.Message = msg;
                messageBox.Title = title;
                messageBox.MessageBoxButton = MessageBoxButton.OK;
                messageBox.UpdateBoxStyle(mode);
                try
                {
                    messageBox.Owner = owner == null? Application.Current.MainWindow : owner;
                }
                catch { }
                messageBox.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as MessageBox).BeginAnimation(MessageBox.OpacityProperty, aniOpacity);
                };
                messageBox.ShowDialog();
            }); 
        }

        public static void OKCancel(string title, string msg, MessageBoxDisplayMode mode = MessageBoxDisplayMode.MaskHor, Window owner = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox messageBox = new MessageBox();
                messageBox.Opacity = 0;
                messageBox.Message = msg;
                messageBox.Title = title;
                messageBox.MessageBoxButton = MessageBoxButton.OKCancel;
                messageBox.UpdateBoxStyle(mode);
                try
                {
                    messageBox.Owner = owner == null ? Application.Current.MainWindow : owner;
                }
                catch { }
                messageBox.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as MessageBox).BeginAnimation(MessageBox.OpacityProperty, aniOpacity);
                };
                messageBox.ShowDialog();
            });
        }

        public static System.Windows.MessageBoxResult YesNo(string title, string msg, MessageBoxDisplayMode mode = MessageBoxDisplayMode.MaskHor, Window owner = null)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox messageBox = new MessageBox();
                messageBox.Opacity = 0;
                messageBox.Message = msg;
                messageBox.Title = title;
                messageBox.MessageBoxButton = MessageBoxButton.YesNo;
                messageBox.UpdateBoxStyle(mode);
                try
                {
                    messageBox.Owner = owner == null ? Application.Current.MainWindow : owner;
                }
                catch { }
                messageBox.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as MessageBox).BeginAnimation(MessageBox.OpacityProperty, aniOpacity);
                };
                messageBox.ShowDialog();
                return messageBox.MessageBoxResult;
            });
        }

        public static System.Windows.MessageBoxResult YesNoCancel(string title, string msg, MessageBoxDisplayMode mode = MessageBoxDisplayMode.MaskHor, Window owner = null)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox messageBox = new MessageBox();
                messageBox.Opacity = 0;
                messageBox.Message = msg;
                messageBox.Title = title;
                messageBox.MessageBoxButton = MessageBoxButton.YesNoCancel;
                messageBox.UpdateBoxStyle(mode);
                try
                {
                    messageBox.Owner = owner == null ? Application.Current.MainWindow : owner;
                }
                catch { }
                messageBox.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as MessageBox).BeginAnimation(MessageBox.OpacityProperty, aniOpacity);
                };
                messageBox.ShowDialog();
                return messageBox.MessageBoxResult;
            });
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.MessageBoxResult = MessageBoxResult.OK;
            this.Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.MessageBoxResult = MessageBoxResult.Yes;
            this.Close();
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.MessageBoxResult = MessageBoxResult.No;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.MessageBoxResult = MessageBoxResult.Cancel;
            this.Close();
        }
    }
}
