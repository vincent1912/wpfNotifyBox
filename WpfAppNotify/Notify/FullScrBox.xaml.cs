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
            this.Focusable = false;
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

        void HostWindow_StateChanged(object sender, EventArgs e)
        {
            if (sender is Window win)
            {
                if (win.WindowState == WindowState.Minimized)
                {
                    this.WindowState = WindowState.Minimized;
                }
                else
                {
                    RefreshPosition();
                }
            }
        }
        void HostWindow_LocationChanged(object sender, EventArgs e)
        {
            RefreshPosition();
        }
        void HostWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshPosition();
        }

        static List<FullScrBox> _boxes = new List<FullScrBox>();

        int _lifeMillionSeconds = 1500;
        bool _isWaitingBox = false;
        NotifyInfo _notifyInfo = null;
        Window _hostWindow = null;

        public object NotifyContent
        {
            get { return (object)GetValue(NotifyContentProperty); }
            set { SetValue(NotifyContentProperty, value); }
        }
         
        public static readonly DependencyProperty NotifyContentProperty =
            DependencyProperty.Register("NotifyContent", typeof(object), typeof(FullScrBox), new PropertyMetadata(null));

        void RefreshPosition()
        {
            if (_notifyInfo.PlaceTarget != null)
            {
                Rect rect = Helper.GetElementBounds(_notifyInfo.PlaceTarget);
                this.Top = rect.Y;
                this.Left = rect.X;
                this.Width = rect.Width;
                this.Height = rect.Height;
            }
        }
 
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
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bounds = Helper.GetScreenBounds(screenIndex);
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
                fullScrBox._notifyInfo = new NotifyInfo { IsScreenNotify = true, IsText = true, PlaceTarget = null, ScreenIndex = screenIndex };
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
                    await Task.Delay(fbox._lifeMillionSeconds);
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
        /// <summary>
        /// 在指定目标上方‘全屏’显示消息通知
        /// </summary>
        /// <param name="title"></param>
        /// <param name="msg"></param>
        /// <param name="placeTarget"></param>
        public static void Notify(string title, string msg, FrameworkElement placeTarget)
        { 
            Application.Current.Dispatcher.Invoke(() =>
            {
                var bounds = Helper.GetElementBounds(placeTarget);
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
                fullScrBox._notifyInfo = new NotifyInfo { ScreenIndex = 0, PlaceTarget = placeTarget, IsScreenNotify = false, IsText = true };
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
                    await Task.Delay(fbox._lifeMillionSeconds);
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
 
        static void Loading(object msgContent,object content,FrameworkElement placeTarget,int screenIndex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Rect bounds = placeTarget != null ? Helper.GetElementBounds(placeTarget) : Helper.GetScreenBounds(screenIndex);
                FullScrBox fullScrBox;
                lock (_boxes)
                {
                    fullScrBox = new FullScrBox();
                    fullScrBox._isWaitingBox = true;
                    _boxes.Add(fullScrBox);
                } 
                if (content != null)
                {
                    fullScrBox.Content = content;
                }
                else
                {
                    fullScrBox.rowIcon.Height = new GridLength(10, GridUnitType.Star);
                    fullScrBox.rectWaitingIcon.Visibility = Visibility.Visible;
                    fullScrBox.NotifyContent = msgContent;
                }
                fullScrBox.Width = bounds.Width;
                fullScrBox.Height = bounds.Height;
                fullScrBox.Left = bounds.X;
                fullScrBox.Top = bounds.Y;
                fullScrBox.Opacity = 0;

                fullScrBox._notifyInfo = new NotifyInfo { IsScreenNotify = placeTarget==null, IsText = msgContent!=null, PlaceTarget = placeTarget, ScreenIndex = screenIndex };
                if (placeTarget!= null)
                {
                    Window window = Window.GetWindow(placeTarget);
                    fullScrBox._hostWindow = window;
                    if (window != null)
                    {
                        window.StateChanged += fullScrBox.HostWindow_StateChanged;
                        window.LocationChanged += fullScrBox.HostWindow_LocationChanged;
                        window.SizeChanged += fullScrBox.HostWindow_SizeChanged;
                    }
                }
                fullScrBox.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as FullScrBox).BeginAnimation(FrameworkElement.OpacityProperty, aniOpacity);

                    DoubleAnimation aniRotate = new DoubleAnimation();
                    aniRotate.Duration = new Duration(TimeSpan.FromMilliseconds(1800));
                    aniRotate.From = 0;
                    aniRotate.To = 360;
                    aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                    (s as FullScrBox).rectRotateTransform.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
                };
                fullScrBox.Closed += (s, e) => 
                {
                    var fbox = s as FullScrBox;
                    if (fbox._hostWindow != null)
                    {
                        fbox._hostWindow.StateChanged -= fbox.HostWindow_StateChanged;
                        fbox._hostWindow.LocationChanged -= fbox.HostWindow_LocationChanged;
                        fbox._hostWindow.SizeChanged -= fbox.HostWindow_SizeChanged;
                    }
                };
                fullScrBox.Show();
            });
        } 

        public static void Loading(object msgContent)
        {
            Loading(msgContent, -1);
        } 
        public static void Loading(object msgContent, int screenIndex)
        {  
            Loading(msgContent, null, null, screenIndex);
        }
        public static void Loading(object msgContent, FrameworkElement placeTarget)
        {
            Loading(msgContent, null, placeTarget, 0);
        }
 
        public static void LoadingCustom(object content)
        {
            LoadingCustom(content, -1);
        }
        public static void LoadingCustom(object content, int screenIndex)
        {
            Loading( null, content, null,screenIndex);
        }
        public static void LoadingCustom(object content, FrameworkElement placeTarget)
        {
            Loading(null, content,placeTarget,0);
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
