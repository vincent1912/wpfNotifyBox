using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Notification.Wpf
{
    class NotifyInfo
    { 
        public bool IsText { get; set; }
        public int ScreenIndex { get; set; }
        public FrameworkElement PlaceTarget { get; set; }
        public bool IsScreenNotify { get; set; } 
    }
}
