using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TEST2
{
    public class LayoutWindow
    {
        private int uses;
        private RECT window;
        private IntPtr hWnd;

        // c++ function 
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);
        //GetWindowRect(IntPtr hWnd, ref RECT Rect)
        public LayoutWindow(RECT window, IntPtr hWnd)
        {
            this.window = window;
            this.hWnd = hWnd;
            uses = 0;
        }
        public void IncrementUses()
        {
            uses++;
        }
        public void DecrementUses()
        {
            uses--;
        }
        public int GetUses()
        {
            return uses;
        }
        public void SetWindow(RECT r)
        {
            window = r;
        }
        public RECT GetWindow()
        {
            return window;
        }
        public IntPtr GetHWnd()
        {
            return hWnd;
        }
        public void UpdateWindow()
        {
            GetWindowRect(hWnd, ref window);
        }
        public override string ToString()
        {
            return string.Format("<LayoutWindow:<RECT:<({0}, {1}), ({2}, {3})>, hWnd: {4}>>", window.left, window.top, window.right, window.bottom, hWnd);
        }
    }
}
