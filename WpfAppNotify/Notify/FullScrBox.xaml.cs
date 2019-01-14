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
        bool _isWaitingBox = false;

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

        public static void Loading(object msgContent)
        {
            Loading(msgContent, -1);
        } 
        public static void Loading(object msgContent, int screenIndex)
        { 
            Loading(Helper.GetScreenBounds(screenIndex), msgContent); 
        }
        public static void Loading(object msgContent, FrameworkElement placeTarget)
        {
            Loading(Helper.GetElementBounds(placeTarget), msgContent);
        }
        public static void Loading(Rect bounds,object msgContent)
        {
            Application.Current.Dispatcher.Invoke(() =>
            { 
                FullScrBox fullScrBox;
                lock (_boxes)
                {
                    fullScrBox = new FullScrBox();
                    fullScrBox._isWaitingBox = true;
                    _boxes.Add(fullScrBox);
                }
                fullScrBox.rowIcon.Height = new GridLength(10, GridUnitType.Star);
                fullScrBox.rectWaitingIcon.Visibility = Visibility.Visible;
                fullScrBox.NotifyContent = msgContent;
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

                    DoubleAnimation aniRotate = new DoubleAnimation();
                    aniRotate.Duration = new Duration(TimeSpan.FromMilliseconds(2400));
                    aniRotate.From = 0;
                    aniRotate.To = 360;
                    aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                    (s as FullScrBox).rectRotateTransform.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
                };
                fullScrBox.Show();
            });
        }

        public static void LoadingCustom(object content)
        {
            LoadingCustom(content, -1);
        }
        public static void LoadingCustom(object content, int screenIndex)
        {
            LoadingCustom(Helper.GetScreenBounds(screenIndex), content);
        }
        public static void LoadingCustom(object content, FrameworkElement placeTarget)
        {
            LoadingCustom(Helper.GetElementBounds(placeTarget), content);
        }
        public static void LoadingCustom(Rect bounds, object content)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FullScrBox fullScrBox;
                lock (_boxes)
                {
                    fullScrBox = new FullScrBox();
                    fullScrBox._isWaitingBox = true;
                    _boxes.Add(fullScrBox);
                }
                fullScrBox.rowIcon.Height = new GridLength(10, GridUnitType.Star);
                fullScrBox.rectWaitingIcon.Visibility = Visibility.Visible;
                fullScrBox.Content = content;
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

                    DoubleAnimation aniRotate = new DoubleAnimation();
                    aniRotate.Duration = new Duration(TimeSpan.FromMilliseconds(2400));
                    aniRotate.From = 0;
                    aniRotate.To = 360;
                    aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                    (s as FullScrBox).rectRotateTransform.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
                };
                fullScrBox.Show();
            });
        }

        public static void LoadingUpdate(object msgContent)
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                lock (_boxes)
                {
                    var box = _boxes.Find(b => b._isWaitingBox);
                    if (box != null)
                    {
                        box.NotifyContent = msgContent;
                    }
                }
            });
        }

        public static void CloseLoading()
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                lock (_boxes)
                {
                    var boxes = _boxes.FindAll(b => b._isWaitingBox);
                    foreach (var item in boxes)
                    {
                        item.Close();
                        _boxes.Remove(item);
                    }
                }
            });
        }
    }
}
