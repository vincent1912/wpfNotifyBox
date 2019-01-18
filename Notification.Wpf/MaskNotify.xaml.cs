using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Notification.Wpf
{
    /// <summary>
    /// 遮罩式消息通知
    /// </summary>
    public partial class MaskNotify : UserControl,INotifyPropertyChanged
    {
        public MaskNotify()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        FrameworkElement _placeTarget;
        Grid _gridPanel;
        DependencyObject _parentOrigin;
        const int LifeMillionSeconds = 4000;

        public event PropertyChangedEventHandler PropertyChanged;

        private object _notifyContent;

        public object NotifyContent
        {
            get { return _notifyContent; }
            set { _notifyContent = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NotifyContent))); }
        }


        /// <summary>
        /// 将element2添加到element1的父容器中，element1则从其父容器中移除
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        static void Switch(FrameworkElement element1, FrameworkElement element2)
        { 
            if (element1 is Window win)
            {
                element1 = win.Content as FrameworkElement;
                if (element1 == null)
                {
                    throw new NotSupportedException();
                }
            }
            var parent = element1.Parent;

            element2.HorizontalAlignment = element1.HorizontalAlignment;
            element2.VerticalAlignment = element1.VerticalAlignment;
            element2.Margin = element1.Margin;
            Grid.SetRow(element2, Grid.GetRow(element1));
            Grid.SetColumn(element2, Grid.GetColumn(element1));
            Grid.SetRowSpan(element2, Grid.GetRowSpan(element1));
            Grid.SetColumnSpan(element2, Grid.GetColumnSpan(element1));
            DockPanel.SetDock(element2, DockPanel.GetDock(element1));
            Canvas.SetLeft(element2, Canvas.GetLeft(element1));
            Canvas.SetTop(element2, Canvas.GetTop(element1));
            Panel.SetZIndex(element2, Panel.GetZIndex(element1));
            if (element1.Width != double.NaN)
            {
                element2.Width = element1.Width;
            }
            if (element1.Height != double.NaN)
            {
                element2.Height = element1.Height;
            }
             
            if (parent is Window win1)
            {
                win1.Content = element2;
            }
            else if (parent is Grid grid)
            {
                grid.Children.Remove(element1);
                grid.Children.Add(element2);
            }
            else if (parent is ContentControl contentControl)
            {
                contentControl.Content = element2;
            }
            else if (parent is StackPanel stackPanel)
            {
                int index = stackPanel.Children.IndexOf(element1);
                stackPanel.Children.Remove(element1);
                stackPanel.Children.Insert(index, element2);
            }
            else if (parent is DockPanel dockPanel)
            {
                int index = dockPanel.Children.IndexOf(element1);
                dockPanel.Children.Remove(element1);
                dockPanel.Children.Insert(index, element2);
            }
            else if (parent is WrapPanel wrapPanel)
            {
                int index = wrapPanel.Children.IndexOf(element1);
                wrapPanel.Children.Remove(element1);
                wrapPanel.Children.Insert(index, element2);
            }
            else if (parent is Border border)
            {
                border.Child = element2;
            }
        }
         
        public static MaskNotify Notify(object msg,FrameworkElement element)
        {
            return Application.Current.Dispatcher.Invoke(() => 
            {
                // 创建容器
                Grid grid = new Grid(); 
                if (element is Window win)
                {
                    element = win.Content as FrameworkElement;
                    if (element == null)
                    {
                        throw new NotSupportedException();
                    }
                }
                var parent = element.Parent;
                Switch(element, grid);

                element.Margin = new Thickness();  // 注意，要设置margin为0，因为grid已替换了element
                grid.Children.Add(element);

                MaskNotify maskMessage = new MaskNotify();
                maskMessage.Opacity = 0;
                maskMessage.NotifyContent = msg;
                maskMessage._placeTarget = element;
                maskMessage._gridPanel = grid;
                maskMessage._parentOrigin = parent;
                grid.Children.Add(maskMessage);
                maskMessage.Loaded += (s, e) =>
                {
                    DoubleAnimation aniOpacity = new DoubleAnimation();
                    aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(600));
                    aniOpacity.To = 1;
                    aniOpacity.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
                    (s as MaskNotify).BeginAnimation(FrameworkElement.OpacityProperty, aniOpacity); 
                };
                Task.Factory.StartNew(async(box) => 
                {
                    await Task.Delay(MaskNotify.LifeMillionSeconds);
                    MaskNotify mm = box as MaskNotify;
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        mm._gridPanel.Children.Clear();
                        Switch(mm._gridPanel, mm._placeTarget);
                    }); 
                }, maskMessage);

                return maskMessage;
            });
        }


        public static void Loading(object msg, FrameworkElement element)
        {
            var maskMessage = Notify(msg, element);
            Application.Current.Dispatcher.Invoke(() =>
            {
                maskMessage.rectWaitingIcon.Visibility = Visibility.Visible;
                maskMessage.rowIcon.Height = new GridLength(10, GridUnitType.Star);
                maskMessage.Loaded += (s, e) =>
                {
                    DoubleAnimation aniRotate = new DoubleAnimation();
                    aniRotate.Duration = new Duration(TimeSpan.FromMilliseconds(1800));
                    aniRotate.By = 360;
                    aniRotate.RepeatBehavior = RepeatBehavior.Forever;
                    (s as MaskNotify).rectRotateTransform.BeginAnimation(RotateTransform.AngleProperty, aniRotate);
                };
            });

        }
    }
}
