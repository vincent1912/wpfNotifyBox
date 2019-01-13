using System;
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
    /// 显示在屏幕右下角的消息通知
    /// </summary>
    public partial class NotifyBox : Window , INotifyPropertyChanged
    {
        public NotifyBox()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Owner = Application.Current.MainWindow;
        }

        double _topFrom;
        double _boxLife = 4000; // 提示框停留4秒

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

        double GetDPIRatio()
        { 
            return (double)PrimaryScreen.DpiX / 96;
        }
         
        double GetTopFrom(int screenIndex)
        {  
            double topFrom = GetScrBottom(screenIndex) - 4; 
            NotifyBox notifyBoxBottom = _boxes.FirstOrDefault(b => b._topFrom == topFrom);
            while (notifyBoxBottom != null)
            {
                topFrom = topFrom - notifyBoxBottom.ActualHeight;
                notifyBoxBottom = _boxes.FirstOrDefault(b => b._topFrom == topFrom);
            }

            if (topFrom <= 0)
                topFrom = GetScrBottom(screenIndex) - 4;

            return topFrom ;
        }

        double GetScrRight(int screenIndex)
        {
            if(screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length-1)
            { 
                return SystemParameters.WorkArea.Right;
            }
            var screen = System.Windows.Forms.Screen.AllScreens[screenIndex]; 
            return ((double)screen.WorkingArea.Right) / GetDPIRatio();
        }

        double GetScrBottom(int screenIndex)
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

        void StartIn(NotifyBox notifyBox)
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

        void StartOut(NotifyBox notifyBox)
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
                    notifyBox.Close();
                } 
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

        int GetScreenIndexOfMouse()
        {
            var screen = System.Windows.Forms.Screen.AllScreens.ToList().Find(s => s.Bounds.Left < System.Windows.Forms.Cursor.Position.X
                       && s.Bounds.Right >= (System.Windows.Forms.Cursor.Position.X));
            return screen == null ? -1 : (System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(screen));
        }

        public void Show(string msg)
        {
            Show(msg, -1);
        }

        public void Show(string title,string msg)
        {
            Show(title, msg, -1);
        }

        public void Show(DependencyObject content)
        {
            Show(content, -1);
        }

        public void Show(DependencyObject content, int screenIndex)
        {
            NotifyBox bx = new NotifyBox();
            bx.Content = content;
            bx._topFrom = GetTopFrom(screenIndex);
            _boxes.Add(bx);
            bx.Loaded += (s, e) =>
            {
                try
                {
                    NotifyBox self = s as NotifyBox;
                    self.UpdateLayout();
                    SystemSounds.Asterisk.Play();//播放提示声

                    self.Top = self._topFrom - self.ActualHeight;
                    self.Left = GetScrRight(screenIndex) - self.ActualWidth;
                    StartIn(self);

                    Task.Factory.StartNew((box) =>
                    {
                        NotifyBox notifyBox = box as NotifyBox;
                        System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            StartOut(notifyBox);
                        });
                    }, self);
                }
                catch (Exception ex)
                {
                    
                }
            };
            bx.Show();
        }

        public void Show(string msg,int screenIndex)
        {
            Show(null, msg, screenIndex);
        }

        public void Show(string title,string msg,int screenIndex)
        {
            NotifyBox bx = new NotifyBox();
            bx.Message = msg;
            bx.Title = title;
            bx._topFrom = GetTopFrom(screenIndex);
            _boxes.Add(bx);
            bx.Loaded += (s, e) =>
            {
                NotifyBox self = s as NotifyBox;
                self.UpdateLayout();
                SystemSounds.Asterisk.Play();//播放提示声

                self.Top = self._topFrom - self.ActualHeight;
                self.Left = GetScrRight(screenIndex) - self.ActualWidth;
                StartIn(self);

                Task.Factory.StartNew((box) =>
                {
                    NotifyBox notifyBox = box as NotifyBox;
                    System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(_boxLife));
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        StartOut(notifyBox);
                    });
                }, self);
            };
            bx.Show();
        }

        public void ShowOnCurrentScr(string msg)
        {
            Show( null, msg, GetScreenIndexOfMouse()); 
        }

        public void ShowOnCurrentScr(string title,string msg,int screenIndex)
        {
            Show( title, msg, GetScreenIndexOfMouse());
        }

        public void ShowOnCurrentScr(DependencyObject content)
        {
            Show(content, GetScreenIndexOfMouse());
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
