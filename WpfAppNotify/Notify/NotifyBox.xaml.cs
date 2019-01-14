using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace WpfAppNotify.Notify
{
    /// <summary>
    /// 显示在屏幕或指定页面右下角的消息通知
    /// </summary>
    public partial class NotifyBox : Window , INotifyPropertyChanged
    {
        public NotifyBox()
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
            catch(Exception ex)
            { 
            }
        }
        /// <summary>
        /// 通知框底部位置
        /// </summary>
        double _bottom;

        /// <summary>
        /// 通知数据
        /// </summary>
        NotifyInfo _notifyInfo;

        /// <summary>
        /// 通知框显示停留时长（毫秒）
        /// </summary>
        static double _boxLife = 4000; // 提示框停留4秒

        /// <summary>
        /// 正在显示的通知框
        /// </summary>
        static List<NotifyBox> _boxes = new List<NotifyBox>();
 
        #region MyRegion

        public event PropertyChangedEventHandler PropertyChanged;

        private string _message;
         
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        #endregion

        /// <summary>
        /// 获取主屏幕dpi缩放比例
        /// </summary>
        /// <returns></returns>
        static double GetDPIRatio()
        { 
            return (double)PrimaryScreen.DpiX / 96;
        }

        /// <summary>
        /// 计算消息框底部位置
        /// </summary>
        /// <param name="screenIndex">屏幕序号</param>
        /// <returns></returns>
        static double CalcBoxBottom(int screenIndex)
        {
            lock (_boxes)
            {
                double topFrom = GetScrBottom(screenIndex) - 4;
                NotifyBox notifyBoxBottom = _boxes.FirstOrDefault(b => b._bottom == topFrom);
                while (notifyBoxBottom != null)
                {
                    // 注意：notifyBoxBottom超出屏幕时，visibility设置为hidden，其ActualHeight为0
                    topFrom = topFrom - (notifyBoxBottom.ActualHeight> 0? notifyBoxBottom.ActualHeight:50);
                    notifyBoxBottom = _boxes.FirstOrDefault(b => b._bottom == topFrom);
                }
                // 不限制是否超出屏幕 
                return topFrom;
            } 
        }

        /// <summary>
        /// 计算消息框底部位置（在参照物右下角开始往上）
        /// </summary>
        /// <param name="relElement">参照物</param>
        /// <returns></returns>
        static double CalcBoxBottom(FrameworkElement relElement)
        {
            lock (_boxes)
            {
                Rect rect = GetScreenBounds(relElement);
                double topFrom = rect.Bottom - 4;
                NotifyBox notifyBoxBottom = _boxes.FirstOrDefault(b => b._bottom == topFrom);
                while (notifyBoxBottom != null)
                {
                    // 注意：notifyBoxBottom超出屏幕时，visibility设置为hidden，其ActualHeight为0
                    topFrom = topFrom - (notifyBoxBottom.ActualHeight > 0 ? notifyBoxBottom.ActualHeight : 50);
                    notifyBoxBottom = _boxes.FirstOrDefault(b => b._bottom == topFrom);
                }
                // 不限制是否超出屏幕
                return topFrom;
            } 
        }

        /// <summary>
        /// 计算指定屏幕右侧位置
        /// </summary>
        /// <param name="screenIndex"></param>
        /// <returns></returns>
        static double GetScrRight(int screenIndex)
        {
            if(screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length-1)
            { 
                return SystemParameters.WorkArea.Right;
            }
            var screen = System.Windows.Forms.Screen.AllScreens[screenIndex]; 
            return ((double)screen.WorkingArea.Right) / GetDPIRatio();
        }

        /// <summary>
        /// 计算指定元素右侧位置（wpf单位）
        /// </summary>
        /// <param name="relElement"></param>
        /// <returns></returns>
        static double CalcElementRight(FrameworkElement relElement)
        {
            Rect rect = GetScreenBounds(relElement);
            return rect.Right;
        }

        /// <summary>
        /// 获取指定屏幕的底部位置（wpf单位）
        /// </summary>
        /// <param name="screenIndex"></param>
        /// <returns></returns>
        static double GetScrBottom(int screenIndex)
        {
            if (screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length - 1)
            {
                return SystemParameters.WorkArea.Bottom;
            }
            var screen = System.Windows.Forms.Screen.AllScreens[screenIndex];
            double dpiRatio = GetDPIRatio();
            var bm = ((double)screen.WorkingArea.Bottom) / dpiRatio;
            return bm;
        }

        /// <summary>
        /// 刷新消息框位置（重新计算‘位置在屏幕外的时间最早的一个消息框’的位置）
        /// </summary>
        static void ReLocateFirstBoxOutOfScreen()
        {
            Application.Current.Dispatcher.Invoke(() => 
            {
                NotifyBox box;
                lock (_boxes)
                {
                    box = _boxes.FirstOrDefault(b => b.Top < 0);  // 第一个显示位置超出屏幕的通知框
                }               
                if (box != null)
                {
                    if (box._notifyInfo.IsScreenNotify)
                    {
                        box._bottom = CalcBoxBottom(box._notifyInfo.ScreenIndex);
                    }
                    else
                    {
                        box._bottom = CalcBoxBottom(box._notifyInfo.RelElement);
                    }
                    box.Top = box._bottom - box.ActualHeight;
                    box.Visibility = Visibility.Visible;
                    FadeIn(box);
                    Task.Factory.StartNew((box1) =>
                    {
                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            FadeOut(box1 as NotifyBox);
                        });
                    }, box);
                }
            }); 
        }

        /// <summary>
        /// 淡入
        /// </summary>
        /// <param name="notifyBox"></param>
        static void FadeIn(NotifyBox notifyBox)
        { 
            DoubleAnimation aniTop = new DoubleAnimation();
            aniTop.Duration = new Duration(TimeSpan.FromMilliseconds(600));
            aniTop.From = notifyBox.Top + 15;
            aniTop.To = notifyBox.Top;
            aniTop.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(aniTop, new PropertyPath(Window.TopProperty));
            Storyboard.SetTarget(aniTop, notifyBox);

            DoubleAnimation aniOpacity = new DoubleAnimation();
            aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); 
            aniOpacity.From = 0;
            aniOpacity.To = 1; 
            aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(Window.OpacityProperty));
            Storyboard.SetTarget(aniOpacity, notifyBox);

            Storyboard sbIn = new Storyboard();
            sbIn.Children.Add(aniTop);
            sbIn.Children.Add(aniOpacity);
            sbIn.Begin();
        }

        /// <summary>
        /// 淡出
        /// </summary>
        /// <param name="notifyBox"></param>
        static void FadeOut(NotifyBox notifyBox)
        { 
            DoubleAnimation aniTop = new DoubleAnimation();
            aniTop.Duration = new Duration(TimeSpan.FromMilliseconds(600)); 
            aniTop.To = notifyBox.Top + 15;
            aniTop.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(aniTop, new PropertyPath(Window.TopProperty));
            Storyboard.SetTarget(aniTop, notifyBox);

            DoubleAnimation aniOpacity = new DoubleAnimation();
            aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(1000)); 
            aniOpacity.To = 0;
            aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            aniOpacity.Completed += (s, e) => 
            {  
                lock (_boxes)
                { 
                    _boxes.Remove(notifyBox); 
                }
                notifyBox.Close();
                ReLocateFirstBoxOutOfScreen(); // 刷新消息框位置
            };
            Storyboard.SetTargetProperty(aniOpacity, new PropertyPath(Window.OpacityProperty));
            Storyboard.SetTarget(aniOpacity, notifyBox);

            /*
            DoubleAnimation aniScale = new DoubleAnimation();
            aniScale.Duration = new Duration(TimeSpan.FromMilliseconds(600));
            aniScale.From = 1;
            aniScale.To = 1.2;
            aniScale.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            ((TransformGroup)notifyBox.RenderTransform).Children[0].BeginAnimation(ScaleTransform.ScaleXProperty, aniScale);
            */

            Storyboard sbOut = new Storyboard();
            sbOut.Children.Add(aniTop);
            sbOut.Children.Add(aniOpacity); 
            sbOut.Begin();

        } 

        /// <summary>
        /// 获取鼠标所在的屏幕的序号
        /// </summary>
        /// <returns></returns>
        static int GetScreenIndexOfMouse()
        {
            var screen = System.Windows.Forms.Screen.AllScreens.ToList().Find(s => s.Bounds.Left < System.Windows.Forms.Cursor.Position.X
                       && s.Bounds.Right >= (System.Windows.Forms.Cursor.Position.X));
            return screen == null ? -1 : (System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(screen));
        }

        /// <summary>
        /// 获取元素在屏幕中的位置和大小
        /// </summary>
        /// <param name="uIElement"></param>
        /// <returns></returns>
        static Rect GetScreenBounds(FrameworkElement uIElement)
        {
            if (uIElement is Window win)
            {
                return new Rect(win.Left, win.Top, win.ActualWidth , win.ActualHeight);
            }
            Point position = uIElement.PointToScreen(new Point(0, 0)); 
            return new Rect(position.X/GetDPIRatio() , position.Y/GetDPIRatio() , uIElement.ActualWidth , uIElement.ActualHeight );
        }

        /// <summary>
        /// 显示消息提示（主屏幕右下角
        /// </summary>
        /// <param name="msg">消息文字</param>
        public static void Show(string msg)
        {
            Show(msg, -1);
        }

        /// <summary>
        /// 显示消息提示（主屏幕右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param> 
        public static void Show(string title,string msg)
        {
            Show(title, msg, -1);
        }

        /// <summary>
        /// 显示消息提示（主屏幕右下角
        /// </summary>
        /// <param name="content">消息内容</param> 
        public static void Show(DependencyObject content)
        {
            Show(content, -1);
        }

        /// <summary>
        /// 显示消息提示（指定屏幕的右下角
        /// </summary>
        /// <param name="msg">消息文字</param>
        /// <param name="screenIndex">屏幕序号</param> 
        public static void Show(string msg,int screenIndex)
        {
            Show(null, msg, screenIndex);
        }
         
        /// <summary>
        /// 显示消息提示（鼠标指针所在屏幕的右下角
        /// </summary>
        /// <param name="msg">消息文字</param>
        public static void ShowOnCurrentScr(string msg)
        {
            Show( null, msg, GetScreenIndexOfMouse()); 
        }

        /// <summary>
        /// 显示消息提示（鼠标指针所在屏幕的右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        /// <param name="screenIndex">屏幕序号</param>
        public static void ShowOnCurrentScr(string title,string msg,int screenIndex)
        {
            Show( title, msg, GetScreenIndexOfMouse());
        }

        /// <summary>
        /// 显示消息提示（鼠标指针所在屏幕的右下角
        /// </summary>
        /// <param name="content">消息内容</param>
        public static void ShowOnCurrentScr(DependencyObject content)
        {
            Show(content, GetScreenIndexOfMouse());
        }

        /// <summary>
        /// 显示消息提示（指定元素的右下角
        /// </summary>
        /// <param name="relElement">参考元素</param>
        /// <param name="msg">消息文字</param>
        public static void Show(FrameworkElement relElement, string msg )
        {
            Show(relElement, null, msg);
        }

        /// <summary>
        /// 显示消息提示（指定屏幕的右下角
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="screenIndex">屏幕序号</param>  
        public static void Show(DependencyObject content, int screenIndex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = GetScrRight(screenIndex) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Content = content;
                bx._bottom = CalcBoxBottom(screenIndex);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = true, IsText = false, ScreenIndex = screenIndex };
                bx._notifyInfo = notifyInfo;
                lock (_boxes)
                {
                    _boxes.Add(bx);
                } 
                bx.Loaded += (s, e) =>
                {
                    try
                    { 
                        NotifyBox self = s as NotifyBox;
                        self.UpdateLayout();
                        SystemSounds.Asterisk.Play();//播放提示声

                        self.Top = self._bottom - self.ActualHeight;
                        self.Left = GetScrRight(screenIndex) - self.ActualWidth;
                        if (self.Top < 0)
                        {
                            self.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            FadeIn(self);
                            Task.Factory.StartNew((box1) =>
                            {
                                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    FadeOut(box1 as NotifyBox);
                                });
                            }, self);
                        }
                    }
                    catch (Exception ex)
                    { 
                    }
                };
                bx.Show();
            });
        }

        /// <summary>
        /// 显示消息提示（指定屏幕的右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        /// <param name="screenIndex">屏幕序号</param> 
        public static void Show(string title, string msg, int screenIndex)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = GetScrRight(screenIndex) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Message = msg;
                bx.Title = title == null ? "" : title;
                bx._bottom = CalcBoxBottom(screenIndex);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = true, IsText = true, ScreenIndex = screenIndex };
                bx._notifyInfo = notifyInfo;  // 注意
                lock (_boxes)
                {
                    _boxes.Add(bx);
                }
                bx.Loaded += (s, e) =>
                {
                    NotifyBox self = s as NotifyBox; 
                    self.UpdateLayout();
                    SystemSounds.Asterisk.Play();//播放提示声

                    self.Top = self._bottom - self.ActualHeight;
                    self.Left = GetScrRight(screenIndex) - self.ActualWidth;
                    if (self.Top < 0)
                    {
                        self.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        FadeIn(self);
                        Task.Factory.StartNew((box1) =>
                        {
                            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                FadeOut(box1 as NotifyBox);
                            });
                        }, self);
                    }
                };
                bx.Show();
            });
        }
        
        /// <summary>
        /// 显示消息提示（指定元素的右下角
        /// </summary>
        /// <param name="relElement">参考元素</param>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        public static void Show(FrameworkElement relElement, string title, string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = CalcElementRight(relElement) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Message = msg;
                bx.Title = title==null?"":title;
                bx._bottom = CalcBoxBottom(relElement);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = false, IsText = true,  RelElement = relElement };
                bx._notifyInfo = notifyInfo;  // 注意
                lock (_boxes)
                {
                    _boxes.Add(bx);
                }
                bx.Loaded += (s, e) =>
                {
                    NotifyBox self = s as NotifyBox; 
                    self.UpdateLayout();
                    SystemSounds.Asterisk.Play();//播放提示声

                    self.Top = self._bottom - self.ActualHeight;
                    self.Left = CalcElementRight(relElement) - self.ActualWidth;
                    if (self.Top < 0)
                    {
                        self.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        FadeIn(self);
                        Task.Factory.StartNew((box1) =>
                        {
                            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                FadeOut(box1 as NotifyBox);
                            });
                        }, self);
                    } 
                };
                bx.Show();
            });
        }

        /// <summary>
        /// 显示消息提示（指定元素的右下角
        /// </summary>
        /// <param name="relElement">参考元素</param>
        /// <param name="content">消息内容</param> 
        public static void Show(FrameworkElement relElement, DependencyObject content)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = CalcElementRight(relElement) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Content = content; 
                bx._bottom = CalcBoxBottom(relElement);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = false, IsText = false, RelElement = relElement };
                bx._notifyInfo = notifyInfo;  // 注意
                lock (_boxes)
                {
                    _boxes.Add(bx);
                }
                bx.Loaded += (s, e) =>
                {
                    NotifyBox self = s as NotifyBox; 
                    self.UpdateLayout();
                    SystemSounds.Asterisk.Play();//播放提示声

                    self.Top = self._bottom - self.ActualHeight;
                    self.Left = CalcElementRight(relElement) - self.ActualWidth;
                    if (self.Top < 0)
                    {
                        self.Visibility = Visibility.Hidden;
                    }
                    else
                    {
                        FadeIn(self);
                        Task.Factory.StartNew((box1) =>
                        {
                            System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                FadeOut(box1 as NotifyBox);
                            });
                        }, self);
                    }
                };
                bx.Show();
            });
        }
    }


    public partial class NotifyBox
    {
        public class PrimaryScreen
        {
            #region Win32 API
            [DllImport("user32.dll")]
            static extern IntPtr GetDC(IntPtr ptr);
            [DllImport("gdi32.dll")]
            static extern int GetDeviceCaps(
                                IntPtr hdc, // handle to DC
                                int nIndex // index of capability
                                );
            [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
            static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
            #endregion
            #region DeviceCaps常量
            const int HORZRES = 8;
            const int VERTRES = 10;
            const int LOGPIXELSX = 88;
            const int LOGPIXELSY = 90;
            const int DESKTOPVERTRES = 117;
            const int DESKTOPHORZRES = 118;
            #endregion
            #region 属性
            /// <summary>
            /// 获取屏幕分辨率当前物理大小
            /// </summary>
            public static Size WorkingArea
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    Size size = new Size();
                    size.Width = GetDeviceCaps(hdc, HORZRES);   // HORZRES：屏幕的宽度（像素）
                    size.Height = GetDeviceCaps(hdc, VERTRES);
                    ReleaseDC(IntPtr.Zero, hdc);
                    return size;
                }
            }
            /// <summary>
            /// 当前系统DPI_X 大小 一般为96
            /// </summary>
            public static int DpiX
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    int DpiX = GetDeviceCaps(hdc, LOGPIXELSX); // LOGPIXELSX：沿屏幕宽度每逻辑英寸的像素数，在多显示器系统中，该值对所显示器相同
                    ReleaseDC(IntPtr.Zero, hdc);
                    return DpiX;
                }
            }
            /// <summary>
            /// 当前系统DPI_Y 大小 一般为96
            /// </summary>
            public static int DpiY
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    int DpiX = GetDeviceCaps(hdc, LOGPIXELSY);
                    ReleaseDC(IntPtr.Zero, hdc);
                    return DpiX;
                }
            }
            /// <summary>
            /// 获取真实设置的桌面分辨率大小
            /// </summary>
            public static Size DESKTOP
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    Size size = new Size();
                    size.Width = GetDeviceCaps(hdc, DESKTOPHORZRES);  // Windows NT：可视桌面的以像素为单位的宽度。如果设备支持一个可视桌面或双重显示则此值可能大于VERTRES
                    size.Height = GetDeviceCaps(hdc, DESKTOPVERTRES);
                    ReleaseDC(IntPtr.Zero, hdc);
                    return size;
                }
            }

            /// <summary>
            /// 获取宽度缩放百分比
            /// </summary>
            public static float ScaleX
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    int t = GetDeviceCaps(hdc, DESKTOPHORZRES);
                    int d = GetDeviceCaps(hdc, HORZRES);
                    float ScaleX = (float)GetDeviceCaps(hdc, DESKTOPHORZRES) / (float)GetDeviceCaps(hdc, HORZRES);
                    ReleaseDC(IntPtr.Zero, hdc);
                    return ScaleX;
                }
            }
            /// <summary>
            /// 获取高度缩放百分比
            /// </summary>
            public static float ScaleY
            {
                get
                {
                    IntPtr hdc = GetDC(IntPtr.Zero);
                    float ScaleY = (float)(float)GetDeviceCaps(hdc, DESKTOPVERTRES) / (float)GetDeviceCaps(hdc, VERTRES);
                    ReleaseDC(IntPtr.Zero, hdc);
                    return ScaleY;
                }
            }
            #endregion
        }
    }
}
