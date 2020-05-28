using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST2
{
    public class LayoutWindow
    {
        private int uses;
        private RECT window;
        private IntPtr hWnd;
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
    }
}
