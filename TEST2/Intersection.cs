using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;

namespace TEST2
{
    public class Intersection
    {
        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int SWP_DRAWFRAME = 0x0020;
        private const int SWP_NOCOPYBITS = 0x0100;
        private const int SWP_NOREDRAW = 0x0008;
        private const int SWP_DEFERERASE = 0x2000;
        private const uint flag = SWP_NOZORDER | SWP_SHOWWINDOW | SWP_DRAWFRAME | SWP_NOCOPYBITS;
        private static IntPtr hWndInsertAfter = IntPtr.Zero;
        private static int idCounter = 0;
        private int id;
        private bool idSet;
        private RECT hitBox;
        private List<LayoutWindow> layoutWindowList;
        private bool horizontal;

        // c++ function 
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);
        //GetWindowRect(IntPtr hWnd, ref RECT Rect)

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

        /*
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        */

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public Intersection(RECT r, List<LayoutWindow> lw_list, bool isMovingHorizontally)
        {
            id = idCounter++;
            hitBox = r;
            layoutWindowList = new List<LayoutWindow>();
            foreach (LayoutWindow lw in lw_list)
                layoutWindowList.Add(lw);
            horizontal = isMovingHorizontally;
        }
        public RECT GetHitBox()
        {
            return hitBox;
        }
        public void UpdateHitBox(RECT r)
        {
            hitBox = r;
        }
        public List<LayoutWindow> GetLayoutWindowList()
        {
            return layoutWindowList;
        }
        public int GetId()
        {
            return id;
        }
        public bool IsInHitBox(int x, int y)
        {
            return (y >= hitBox.top && y <= hitBox.bottom) && (x >= hitBox.left && x <= hitBox.right);
        }
        public void SetId(int id)
        {
            if (!idSet)
            {
                this.id = id;
                idSet = true;
            }
        }
        public bool IsMovingHorizontally()
        {
            return horizontal;
        }
        public bool IsMovingVertically()
        {
            return !horizontal;
        }
        public void MoveHitBox(int x, int y)
        {
            // Console.WriteLine("Moving intersection to (" + x + "," + y + ")");
            foreach (LayoutWindow lw in layoutWindowList)
            {
                UpdateWindow(lw, x, y);
                lw.UpdateWindow();
            }
            if (IsMovingVertically())
            {
                hitBox.top = y - (Layout.MAXDISTANCE + 1);
                hitBox.bottom = y + Layout.MAXDISTANCE;
            }
            else if (IsMovingHorizontally())
            {
                hitBox.right = x + Layout.MAXDISTANCE;
                hitBox.left = x - (Layout.MAXDISTANCE + 1);
            }

        }
        private void UpdateWindow(LayoutWindow lw, int x, int y)
        {
            IntPtr hWnd = lw.GetHWnd();
            // Console.WriteLine("\tMoving window to (" + x + "," + y + ")");
            RECT currentWindow = new RECT();
            GetWindowRect(hWnd, ref currentWindow);
            
            int thickness = Form1.GetBorderThickness(hWnd);
            float scaling = Form1.getScalingFactor(hWnd);
            if (IsMovingHorizontally())
            {
                // Console.WriteLine("\tHorizontal change");
                currentWindow.right -= (int)(thickness * scaling);
                currentWindow.left += (int)(thickness * scaling);
                // Console.WriteLine("\t\thitBox: " + Form1.RECTToString(hitBox));
                // Console.WriteLine("\t\tWindow: " + Form1.RECTToString(currentWindow));
                if (currentWindow.right >= hitBox.left && currentWindow.right <= hitBox.right)
                {
                    MoveWindow(hWnd, currentWindow.left - (int)(thickness * scaling), 
                        currentWindow.top, 
                        (x - currentWindow.left) + 2 * (int)(thickness * scaling), 
                        (currentWindow.bottom - currentWindow.top), true);
                    lw.SetWindow(new RECT { 
                        top = currentWindow.top, 
                        left = currentWindow.left - (int)(thickness * scaling),
                        bottom = currentWindow.bottom,
                        right = x + (int)(thickness * scaling)
                    });
                    /*
                    SetWindowPos(hWnd, hWndInsertAfter, currentWindow.left - (int)(thickness * scaling),
                        currentWindow.top,
                        (x - currentWindow.left) + 2 * (int)(thickness * scaling),
                        (currentWindow.bottom - currentWindow.top), flag);
                    */
                }
                else if (currentWindow.left >= hitBox.left && currentWindow.left <= hitBox.right)
                {
                    MoveWindow(hWnd, x - (int)(thickness * scaling),
                        currentWindow.top,
                        (currentWindow.right - x) + 2 * (int)(thickness * scaling),
                        (currentWindow.bottom - currentWindow.top), true);
                    lw.SetWindow(new RECT
                    {
                        top = currentWindow.top,
                        left = x - (int)(thickness * scaling),
                        bottom = currentWindow.bottom,
                        right = currentWindow.right + (int)(thickness * scaling)
                    });
                }
            }
            else if (IsMovingVertically())
            {
                // Console.WriteLine("\tVertical change");
                currentWindow.bottom -= (int)(thickness * scaling);
                // Console.WriteLine("\t\thitBox: " + Form1.RECTToString(hitBox));
                // Console.WriteLine("\t\tWindow: " + Form1.RECTToString(currentWindow));
                
                if (currentWindow.bottom >= hitBox.top && currentWindow.bottom <= hitBox.bottom)
                {
                    bool r = MoveWindow(hWnd, currentWindow.left,
                        currentWindow.top,
                        currentWindow.right - currentWindow.left,
                        (y - currentWindow.top) + (int)(thickness * scaling), true);
                    // Console.WriteLine("\t\tWas move1 successful: " + r);
                    lw.SetWindow(new RECT
                    {
                        top = currentWindow.top,
                        left = currentWindow.left,
                        bottom = y + (int)(thickness * scaling),
                        right = currentWindow.right
                    });
                }
                else if (currentWindow.top >= hitBox.top && currentWindow.top <= hitBox.bottom)
                {
                    bool r = MoveWindow(hWnd, currentWindow.left,
                        y, currentWindow.right - currentWindow.left,
                        (currentWindow.bottom - y) + (int)(thickness * scaling), true);
                    //Console.WriteLine("\t\tWas move2 successful: " + r);
                    lw.SetWindow(new RECT
                    {
                        top = y,
                        left = currentWindow.left,
                        bottom = currentWindow.bottom + (int)(thickness * scaling),
                        right = currentWindow.right
                    });
                } 
                else
                {
                    Console.WriteLine("\t\tCan't find edges");
                }
            }
            // Console.WriteLine("\t\tNew Window: " + Form1.RECTToString(lw.GetWindow()));
        }
        public void UpdateWindows()
        {
            foreach (LayoutWindow lw in layoutWindowList) 
            {
                RECT r = new RECT();
                GetWindowRect(lw.GetHWnd(), ref r);
                lw.SetWindow(r);
            }
        }
        public bool Equals(Intersection other)
        {
            if (layoutWindowList.Count != other.layoutWindowList.Count)
                return false;
            if (hitBox != other.hitBox)
                return false;
            return true;
        }
        public static int GetMAXDISTANCE()
        {
            return Layout.MAXDISTANCE;
        }
        public static List<Intersection> GetIntersections(RECT display, RECT[] windows, IntPtr[] hWnds)
        {
            List<int> blh = new List<int>();
            List<int> blv = new List<int>();
            List<Intersection> test;
            List<Intersection> result = new List<Intersection>();
            RECT taskbar = new RECT();
            GetWindowRect(FindWindow("Shell_TrayWnd", null), ref taskbar);
            int taskBarHeight = Math.Abs(taskbar.bottom - taskbar.top);
            for (int i = 0; i < windows.Length; i++)
            {
                test = new List<Intersection>();
                Intersection intersec;
                if (windows[i].top > display.top)
                {
                    intersec = GetHorizontalAdjacentWindows(windows[i].top, windows, hWnds, blh);
                    if (!AlreadyExists(result, intersec))
                        test.Add(intersec);
                }
                if (windows[i].bottom < display.bottom - taskBarHeight)
                {
                    intersec = GetHorizontalAdjacentWindows(windows[i].bottom - (int)(Form1.getScalingFactor(hWnds[i]) * Form1.GetBorderThickness(hWnds[i])), windows, hWnds, blh);
                    if (!AlreadyExists(result, intersec))
                        test.Add(intersec);
                }
                if (windows[i].right < display.right)
                {
                    intersec = GetVerticalAdjacentWindows(windows[i].right - (int)(Form1.getScalingFactor(hWnds[i]) * Form1.GetBorderThickness(hWnds[i])), windows, hWnds, blv);
                    if (!AlreadyExists(result, intersec))
                        test.Add(intersec);
                }
                if (windows[i].left > display.left)
                {
                    intersec = GetVerticalAdjacentWindows(windows[i].left + (int)(Form1.getScalingFactor(hWnds[i]) * Form1.GetBorderThickness(hWnds[i])), windows, hWnds, blv);
                    if (!AlreadyExists(result, intersec))
                        test.Add(intersec);
                }

                foreach (Intersection inter in test)
                    if (inter != null)
                    {
                        Console.WriteLine(inter.ToString());
                        result.Add(inter);
                    }
            }
            Console.WriteLine("" + result.Count + " intersections were found");
            return result;
        }
        private static Intersection GetHorizontalAdjacentWindows(int pos, RECT[] windows, IntPtr[] hWnds, List<int> bl)
        {
            if (bl.Contains(pos))
                return null;
            List<LayoutWindow> lws = new List<LayoutWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                IntPtr chWnd = hWnds[i];
                if (CloseEnough(windows[i].top, pos, 1))
                {
                    lws.Add(new LayoutWindow(windows[i], hWnds[i]));
                }
                else if (CloseEnough(windows[i].bottom - (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)), pos, 1))
                {
                    lws.Add(new LayoutWindow(windows[i], hWnds[i]));
                }
            }
            if (lws.Count > 1)
            {
                bl.Add(pos);
                IntPtr hWnd = lws[0].GetHWnd();
                RECT hitBox = new RECT
                {
                    top = pos - GetMAXDISTANCE(),
                    bottom = pos + GetMAXDISTANCE(),
                    right = lws[0].GetWindow().right - (int)(Form1.getScalingFactor(hWnd) * Form1.GetBorderThickness(hWnd)),
                    left = lws[0].GetWindow().left + (int)(Form1.getScalingFactor(hWnd) * Form1.GetBorderThickness(hWnd))
                };

                for (int i = 1; i < lws.Count; i++)
                {
                    IntPtr chWnd = lws[i].GetHWnd();
                    if (lws[i].GetWindow().right - (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)) > hitBox.right)
                        hitBox.right = lws[i].GetWindow().right - (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd));
                }
                for (int i = 1; i < lws.Count; i++)
                {
                    IntPtr chWnd = lws[i].GetHWnd();
                    if (lws[i].GetWindow().left + (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)) < hitBox.left)
                        hitBox.left = lws[i].GetWindow().left + (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd));
                }
                return new Intersection(hitBox, lws, false);
            }
            return null;
        }
        private static Intersection GetVerticalAdjacentWindows(int pos, RECT[] windows, IntPtr[] hWnds, List<int> bl)
        {
            Console.WriteLine("stating to look for an intersection at pos: " + pos + " horizontal");
            if (bl.Contains(pos))
                return null;
            List<LayoutWindow> lws = new List<LayoutWindow>();
            for (int i = 0; i < windows.Length; i++)
            {
                IntPtr chWnd = hWnds[i];
                Console.WriteLine("window.right = {0}\n window.left = {1}\nborder = {2}\nscaling = {3}", windows[i].right, windows[i].left, Form1.getScalingFactor(chWnd), Form1.GetBorderThickness(chWnd));
                if (CloseEnough(windows[i].right - (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)), pos, 1))
                {
                    Console.WriteLine("add " + hWnds[i]);
                    lws.Add(new LayoutWindow(windows[i], hWnds[i]));
                } 
                else if (CloseEnough(windows[i].left + (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)), pos, 1))
                {
                    Console.WriteLine("add " + hWnds[i]);
                    lws.Add(new LayoutWindow(windows[i], hWnds[i]));
                }
            }
            if (lws.Count > 1)
            {
                Console.WriteLine("Intersection will be created!!!");
                bl.Add(pos);
                IntPtr hWnd = lws[0].GetHWnd();
                RECT hitBox = new RECT
                {
                    top = lws[0].GetWindow().top,
                    bottom = lws[0].GetWindow().bottom - (int)(Form1.getScalingFactor(hWnd) * Form1.GetBorderThickness(hWnd)),
                    right = pos + GetMAXDISTANCE(),
                    left = pos - GetMAXDISTANCE()
                };
                for (int i = 1; i < lws.Count; i++)
                {
                    if (lws[i].GetWindow().top < hitBox.top)
                    {
                        hitBox.top = lws[i].GetWindow().top;
                    }
                }
                for (int i = 1; i < lws.Count; i++)
                {
                    IntPtr chWnd = lws[i].GetHWnd();
                    if (lws[i].GetWindow().bottom - (int)(Form1.getScalingFactor(chWnd) * Form1.GetBorderThickness(chWnd)) > hitBox.bottom)
                    {
                        hitBox.bottom = lws[i].GetWindow().bottom;
                    }
                }
                return new Intersection(hitBox, lws, true);
            }
            return null;
        }
        public static bool CloseEnough(int pos1, int pos2, int prescision=0)
        {
            return (pos1 + prescision >= pos2 && pos1 - prescision <= pos2);
        }
        private static bool AlreadyExists(List<Intersection> inters, Intersection inter)
        {
            if (inter == null)
                return false;
            bool isHorizontal = IsHitBoxHorizontal(inter.GetHitBox());
            foreach (Intersection i in inters)
            {
                if (IsHitBoxHorizontal(i.GetHitBox()) == isHorizontal)
                    if (Overlapped(i, inter, isHorizontal))
                        return true;
            }
            return false;
        }
        public static bool IsHitBoxHorizontal(RECT r)
        {
            return (r.right - r.left) > (r.bottom - r.top);
        }
        private static bool Overlapped(Intersection i1, Intersection i2, bool isHorizontal)
        {
            if (isHorizontal)
            {
                if (CloseEnough(i1.GetHitBox().top, i2.GetHitBox().top, GetMAXDISTANCE() * 5))
                    return true;
            }
            else 
            {
                if (CloseEnough(i1.GetHitBox().left, i2.GetHitBox().left, GetMAXDISTANCE() * 5))
                    return true;
            }
            return false;
        }
        public override string ToString()
        {
            string result = string.Format("<{4} Intersection:<hitBox: <({0},{1}), ({2},{3})>", hitBox.left, hitBox.top, hitBox.right, hitBox.bottom, OrientationToString());
            foreach (LayoutWindow lw in layoutWindowList)
            {
                result = result + ",\n\t" + lw.ToString();
            }
            return result;
        }
        private string OrientationToString()
        {
            if (IsMovingHorizontally())
                return "vertical";
            else
                return "horizontal";
        }
        public bool ContainsHandle(IntPtr handle) 
        {
            foreach (LayoutWindow lw in layoutWindowList)
            {
                if (handle == lw.GetHWnd())
                    return true;
            }
            return false;
        }
    }
}
