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



        public static void FullScrNotify(string msg)
        {
            FullScrNotify("", msg);
        }

        public static void FullScrNotify(string msg, int screenIndex)
        {
            FullScrNotify(null, msg, screenIndex);
        }

        public static void FullScrNotify(string title, string msg)
        {
            FullScrNotify(title, msg, -1);
        }

        public static void FullScrNotify(string title, string msg, int screenIndex)
        {
            if (screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length - 1)
            {
                screenIndex = Helper.GetPrimaryScreenIndex();
            }
            Rect rect = Helper.GetScreenBounds(screenIndex);

            FullScrNotify(rect, title, msg);
        }

        public static void FullScrNotify(FrameworkElement relElement, string msg)
        {
            FullScrNotify(relElement, null, msg);
        }

        public static void FullScrNotify(FrameworkElement relElement, string title, string msg)
        {
            FullScrNotify(Helper.GetElementBounds(relElement), title, msg);
        }

        public static void FullScrNotify(Rect bounds, string title, string msg)
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
