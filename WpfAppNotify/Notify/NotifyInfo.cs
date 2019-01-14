using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfAppNotify.Notify
{
    class NotifyInfo
    { 
        public bool IsText { get; set; }
        public int ScreenIndex { get; set; }
        public FrameworkElement RelElement { get; set; }
        public bool IsScreenNotify { get; set; }
        public NotifyBox Box { get; set; }
    }
}
