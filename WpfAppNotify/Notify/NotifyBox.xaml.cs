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
    public partial class NotifyBox : Window 
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

        public string Message { get; set; }

        public object MessageObj { get; set; }

        void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            lock (_boxes)
            {
                _boxes.Remove(sender as NotifyBox);
                this.Close();
            }
        }

        #endregion

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
                Rect rect = Helper.GetElementBounds(relElement);
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
        /// <param name="screenIndex">屏幕序号（-1表示主屏幕）</param>
        /// <returns></returns>
        static double GetScrRight(int screenIndex)
        {
            if(screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length-1)
            { 
                return SystemParameters.WorkArea.Right;
            }
            var screen = System.Windows.Forms.Screen.AllScreens[screenIndex]; 
            return ((double)screen.WorkingArea.Right) / Helper.GetDPIRatio();
        }

        /// <summary>
        /// 计算指定元素右侧位置（wpf单位）
        /// </summary>
        /// <param name="relElement"></param>
        /// <returns></returns>
        static double CalcElementRight(FrameworkElement relElement)
        {
            Rect rect = Helper.GetElementBounds(relElement);
            return rect.Right;
        }

        /// <summary>
        /// 获取指定屏幕的底部位置（wpf单位）
        /// </summary>
        /// <param name="screenIndex">屏幕序号（-1表示主屏幕）</param>
        /// <returns></returns>
        static double GetScrBottom(int screenIndex)
        {
            if (screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length - 1)
            {
                return SystemParameters.WorkArea.Bottom;
            }
            var screen = System.Windows.Forms.Screen.AllScreens[screenIndex];
            double dpiRatio = Helper.GetDPIRatio();
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
         
        #region 文字消息
         
        /// <summary>
        /// 显示消息提示（主屏幕右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param> 
        public static void Notify(string title,string msg)
        {
            Notify(title, msg, -1);
        }
        /// <summary>
        /// 显示消息提示（鼠标指针所在屏幕的右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        /// <param name="screenIndex">屏幕序号</param>
        public static void NotifyOnCurrentScr(string title, string msg)
        {
            Notify(title, msg, Helper.GetScreenIndexOfMouse());
        }
        /// <summary>
        /// 显示消息提示（指定屏幕的右下角
        /// </summary>
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        /// <param name="screenIndex">屏幕序号</param> 
        public static void Notify(string title, string msg, int screenIndex)
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
        /// <param name="title">消息标题</param>
        /// <param name="msg">消息文字</param>
        /// <param name="placeTarget">消息停靠目标（消息在该元素内部右下角显示）</param>
        public static void Notify( string title, string msg , FrameworkElement placeTarget)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = CalcElementRight(placeTarget) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Message = msg;
                bx.Title = title == null ? "" : title;
                bx._bottom = CalcBoxBottom(placeTarget);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = false, IsText = true, RelElement = placeTarget };
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
                    self.Left = CalcElementRight(placeTarget) - self.ActualWidth;
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

        #endregion

        #region 自定义消息

        /// <summary>
        /// 显示消息提示（主屏幕的右下角
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="title">消息标题</param>
        public static void Notify(DependencyObject content, string title)
        {
            Notify(content, title, -1);
        }

        /// <summary>
        /// 显示消息提示（鼠标指针所在屏幕的右下角
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="title">消息标题</param>
        public static void NotifyOnCurrentScr(DependencyObject content, string title)
        {
            Notify(content, title, Helper.GetScreenIndexOfMouse());
        }

        /// <summary>
        /// 显示消息提示（指定屏幕的右下角
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="title">消息标题</param>  
        /// <param name="screenIndex">屏幕序号</param>  
        public static void Notify(DependencyObject content, string title, int screenIndex)
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
                bx.MessageObj = content;
                bx.Title = title == null ? "" : title;
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
        /// 显示消息提示
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <param name="title">消息标题</param>
        /// <param name="placeTarget">消息停靠目标（消息在该元素内部右下角显示）</param>
        public static void Notify(DependencyObject content, string title , FrameworkElement placeTarget)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = CalcElementRight(placeTarget) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.MessageObj = content;
                bx.Title = title;
                bx._bottom = CalcBoxBottom(placeTarget);
                var notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = false, IsText = false, RelElement = placeTarget };
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
                    self.Left = CalcElementRight(placeTarget) - self.ActualWidth;
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
                            lock (_boxes)
                            {
                                if (_boxes.Contains(box1))
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        FadeOut(box1 as NotifyBox);
                                    });
                                }
                            }
                        }, self);
                    }
                };
                bx.Show();
            });
        }

        #endregion

        #region 完全自定义

        public static void NotifyCustom(object content)
        {
            NotifyCustom(content, -1);
        }
        public static void NotifyCustomCurScr(object content)
        {
            NotifyCustom(content, Helper.GetScreenIndexOfMouse());
        }
        public static void NotifyCustom(object content, int screenIndex)
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
                bx.Title = "";
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
                        self.Left = GetScrRight(self._notifyInfo.ScreenIndex) - self.ActualWidth;
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
        public static void NotifyCustom(object content, FrameworkElement placeTarget)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NotifyBox bx = new NotifyBox();
                DependencyPropertyDescriptor.FromProperty(ActualWidthProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Left = CalcElementRight(placeTarget) - (sender as NotifyBox).ActualWidth;
                });
                DependencyPropertyDescriptor.FromProperty(ActualHeightProperty, typeof(FrameworkElement)).AddValueChanged(bx, (sender, e) =>
                {
                    (sender as NotifyBox).Top = (sender as NotifyBox)._bottom - (sender as NotifyBox).ActualHeight;
                });
                bx.Content = content;
                bx.Title = "";
                bx._bottom = CalcBoxBottom(placeTarget);
                bx._notifyInfo = new NotifyInfo { Box = bx, IsScreenNotify = false, IsText = false, RelElement = placeTarget };
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
                    self.Left = CalcElementRight(self._notifyInfo.RelElement) - self.ActualWidth;
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
                            lock (_boxes)
                            {
                                if (_boxes.Contains(box1))
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        FadeOut(box1 as NotifyBox);
                                    });
                                }
                            }
                        }, self);
                    }
                };
                bx.Show();
            });
        }

        #endregion
         
    } 
}
