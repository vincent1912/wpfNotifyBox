using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
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
    public partial class NotifyBox : Window
    {
        public NotifyBox()
        {
            InitializeComponent();
            this.Owner = Application.Current.MainWindow; 
        }

        double _topFrom;
        double _boxLife = 4000; // 提示框停留4秒

        static List<NotifyBox> _boxes = new List<NotifyBox>();

        double GetTopFrom()
        { 
            double topFrom = System.Windows.SystemParameters.WorkArea.Bottom - 5;
            bool isContinueFind = _boxes.Any(o => o._topFrom == topFrom);

            while (isContinueFind)
            {
                topFrom = topFrom - 80;//此处80是NotifyWindow的高
                isContinueFind = _boxes.Any(o => o._topFrom == topFrom);
            }

            if (topFrom <= 0)
                topFrom = System.Windows.SystemParameters.WorkArea.Bottom - 5;

            return topFrom ;
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
            aniTop.From = notifyBox.Top;
            aniTop.To = notifyBox.Top + 15;
            aniTop.EasingFunction = new QuarticEase() { EasingMode = EasingMode.EaseOut };
            Storyboard.SetTargetProperty(aniTop, new PropertyPath(Window.TopProperty));
            Storyboard.SetTarget(aniTop, notifyBox);

            DoubleAnimation aniOpacity = new DoubleAnimation();
            aniOpacity.Duration = new Duration(TimeSpan.FromMilliseconds(1000));
            aniOpacity.From = 1;
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

        public void Show(string msg)
        {
            NotifyBox bx = new NotifyBox();

            bx._topFrom = GetTopFrom();
            _boxes.Add(bx);
            //  self.DataContext = data;//设置通知里要显示的数据
            bx.Loaded += (s, e) => 
            {
                NotifyBox self = s as NotifyBox;
                self.UpdateLayout();
                SystemSounds.Asterisk.Play();//播放提示声
                 
                self.Top = self._topFrom - self.ActualHeight;
                self.Left = SystemParameters.WorkArea.Right - self.ActualWidth;
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
    }
}
