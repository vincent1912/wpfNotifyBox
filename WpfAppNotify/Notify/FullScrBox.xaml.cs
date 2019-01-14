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

namespace WpfAppNotify.Notify
{
    /// <summary>
    /// 全屏消息通知
    /// </summary>
    public partial class FullScrBox : Window
    {
        public FullScrBox()
        {
            InitializeComponent();
            this.DataContext = this;
            try
            {
                if (Application.Current.MainWindow != null)
                {
                    this.Owner = Application.Current.MainWindow;
                }
            }
            catch (Exception ex)
            {
            } 
        }

        static List<FullScrBox> _boxes = new List<FullScrBox>();

        int _lifeMillionSeconds = 1500;

        public object NotifyContent
        {
            get { return (object)GetValue(NotifyContentProperty); }
            set { SetValue(NotifyContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NotifyContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotifyContentProperty =
            DependencyProperty.Register("NotifyContent", typeof(object), typeof(FullScrBox), new PropertyMetadata(null));
 
        /// <summary>
        /// 在主屏幕全屏显示消息通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        public static void Notify(string title, string msg)
        {
            Notify(title, msg, -1);
        }
        /// <summary>
        /// 在指定屏幕全屏显示消息通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="screenIndex"></param>
        public static void Notify(string title, string msg, int screenIndex)
        {
            if (screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length - 1)
            {
                screenIndex = Helper.GetPrimaryScreenIndex();
            }
            Rect rect = Helper.GetScreenBounds(screenIndex);

            Notify(title, msg,rect);
        }
        /// <summary>
        /// 在指定目标上方‘全屏’显示消息通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="placeTarget"></param>
        public static void Notify(string title, string msg, FrameworkElement placeTarget)
        {
            Notify( title, msg , Helper.GetElementBounds(placeTarget));
        }
        /// <summary>
        /// 在指定屏幕区域显示消息通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="bounds"></param>
        public static void Notify(string title, string msg, Rect bounds)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FullScrBox fullScrBox;
                lock (_boxes)
                {
                    fullScrBox = new FullScrBox();
                    _boxes.Add(fullScrBox);
                }
                fullScrBox.Title = title == null ? "" : title;
                fullScrBox.NotifyContent = msg;
                fullScrBox.Width = bounds.Width;
                fullScrBox.Height = bounds.Height;
                fullScrBox.Left = bounds.X;
                fullScrBox.Top = bounds.Y;
                fullScrBox.Opacity = 0;
                fullScrBox.Loaded += (s, e) => 
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as FullScrBox).BeginAnimation(FrameworkElement.OpacityProperty, aniOpacity);
                };
                fullScrBox.Show();
                Task.Factory.StartNew(async (box) =>
                {
                    FullScrBox fbox = box as FullScrBox;
                    await Task.Delay( fbox._lifeMillionSeconds);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        lock (_boxes)
                        {
                            _boxes.Remove(fbox);
                        }
                        fbox.Close();
                    });
                }, fullScrBox);

            });
        }

    }
}
