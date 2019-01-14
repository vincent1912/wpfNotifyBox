using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppNotify.Notify
{
    public class NotifyArgs
    {
        public string Message { get; set; }
        public object Content { get; set; }
        public string Title { get; set; } 
    }
}
