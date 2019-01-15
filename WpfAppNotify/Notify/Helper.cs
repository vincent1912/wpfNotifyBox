using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace WpfAppNotify.Notify
{
    static class Helper
    {
        /// <summary>
        /// 获取元素在屏幕中的位置和大小
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Rect GetElementBounds(FrameworkElement element)
        {
            if (element is Window win)
            {
                Thickness resizeBorderThickness = new Thickness(0);
                Thickness nonClientFrameThickness = new Thickness(0);
                if (WindowChrome.GetWindowChrome(win) == null)
                {
                    if (win.WindowStyle != WindowStyle.None)
                    {
                        nonClientFrameThickness = SystemParameters.WindowNonClientFrameThickness;
                    }
                    if (win.ResizeMode != ResizeMode.NoResize)
                    {
                        resizeBorderThickness = SystemParameters.WindowResizeBorderThickness;
                    }
                }
                return new Rect(
                    win.Left + resizeBorderThickness.Left + nonClientFrameThickness.Left, 
                    win.Top, 
                    win.ActualWidth - (resizeBorderThickness.Left + resizeBorderThickness.Right + nonClientFrameThickness.Left + nonClientFrameThickness.Right), 
                    win.ActualHeight - (resizeBorderThickness.Bottom + nonClientFrameThickness.Bottom));
            }
            Point position = element.PointToScreen(new Point(0, 0));
            return new Rect(position.X / GetDPIRatio(), position.Y / GetDPIRatio(), element.ActualWidth, element.ActualHeight);
        }
         
        /// <summary>
        /// 获取主屏幕dpi缩放比例
        /// </summary>
        /// <returns></returns>
        public static double GetDPIRatio()
        {
            return (double)PrimaryScreen.DpiX / 96;
        }

        /// <summary>
        /// 获取主屏幕序号
        /// </summary>
        /// <returns></returns>
        public static int GetPrimaryScreenIndex()
        {
            return System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(System.Windows.Forms.Screen.PrimaryScreen);
        }

        internal static Rect GetScreenBounds(int screenIndex)
        {
            if (screenIndex < 0 || screenIndex > System.Windows.Forms.Screen.AllScreens.Length - 1)
            {
                return SystemParameters.WorkArea;
            }
            var scr = System.Windows.Forms.Screen.AllScreens[screenIndex];
            double ratio = GetDPIRatio();
            return new Rect(scr.Bounds.X / ratio, scr.Bounds.Y / ratio, scr.Bounds.Width / ratio, scr.Bounds.Height / ratio);
        }

        /// <summary>
        /// 获取鼠标所在的屏幕的序号
        /// </summary>
        /// <returns></returns>
        public static int GetScreenIndexOfMouse()
        {
            var screen = System.Windows.Forms.Screen.AllScreens.ToList().Find(s => s.Bounds.Left < System.Windows.Forms.Cursor.Position.X
                       && s.Bounds.Right >= (System.Windows.Forms.Cursor.Position.X));
            return screen == null ? -1 : (System.Windows.Forms.Screen.AllScreens.ToList().IndexOf(screen));
        }
    }



    internal class PrimaryScreen
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
