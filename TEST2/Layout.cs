using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEST2
{
    public class Layout
    {
        public static int MAXDISTANCE = 10;
        private RECT currentWin;
        private int intersectionIdCount = 0;
        private List<Intersection> intersections;
        private List<LayoutWindow> layoutWindows;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);
        //GetWindowRect(IntPtr hWnd, ref RECT Rect)

        public Layout(RECT r)
        {
            currentWin = r;
            intersections = new List<Intersection>();
            layoutWindows = new List<LayoutWindow>();
        }
        public void Reset()
        {
            intersectionIdCount = 0;
            intersections = new List<Intersection>();
            layoutWindows = new List<LayoutWindow>();
        }
        public void AddIntersections(List<Intersection> inters)
        {
            foreach (Intersection inter in inters)
            {
                AddIntersection(inter);
            }
        }
        public bool AddIntersection(Intersection inter)
        {
            if (!ContainsRECT(inter.GetHitBox()))
                return false;
            inter.SetId(intersectionIdCount++);
            intersections.Add(inter);
            foreach (LayoutWindow lw in inter.GetLayoutWindowList())
                AddLayoutWindow(lw);
            return true;
        }
        private void AddLayoutWindow(LayoutWindow lw)
        {
            if (!ContainshWnd(lw.GetHWnd()))
            {
                layoutWindows.Add(lw);
                return;
            }
            else
            {
                foreach (LayoutWindow lw1 in layoutWindows)
                {
                    if (lw1.GetHWnd() == lw.GetHWnd())
                    {
                        lw1.IncrementUses();
                        return;
                    }
                }
            }
        }
        public bool RemoveInterserction(Intersection inter)
        {
            bool removedInter = false;
            for (int i = 0; i < intersections.Count && !removedInter; i++)
            {
                if (inter.GetId() == intersections[i].GetId())
                {
                    intersections.RemoveAt(i);
                    break;
                }
            }
            return true;
        }
        private bool ContainsRECT(RECT r)
        {
            if (r.top >= currentWin.top && r.top <= currentWin.bottom && r.bottom >= currentWin.top && r.bottom <= currentWin.bottom)
                if (r.right >= currentWin.left && r.right <= currentWin.right && r.left >= currentWin.left && r.left <= currentWin.right)
                    return true;
            return false;
        }
        public bool ContainshWnd(IntPtr hWnd)
        {
            bool found = false;
            foreach (LayoutWindow lw in layoutWindows)
                if (lw.GetHWnd() == hWnd)
                {
                    found = true;
                    break;
                }
            return found;
        }
        public void RemoveLayoutWindow(LayoutWindow lw)
        {
            for (int i = 0; i < layoutWindows.Count; i++)
            {
                LayoutWindow lw2 = layoutWindows[i];
                if (lw.GetHWnd() == lw2.GetHWnd())
                {
                    lw2.DecrementUses();
                    if (lw2.GetUses() >= 0)
                    {
                        layoutWindows.RemoveAt(i);
                    }
                    return;
                }
            }
            return;
        }
        public List<Intersection> GetIntersections()
        {
            return intersections;
        }
        public int GetIntersectionsSize()
        {
            return intersections.Count;
        }
        public List<LayoutWindow> GetlayoutWindows()
        {
            return layoutWindows;
        }
        public void CheckIntegrity()
        {
            RECT test = new RECT();
            foreach (LayoutWindow lw in layoutWindows)
            {
                GetWindowRect(lw.GetHWnd(), ref test);
                if (test != lw.GetWindow()) 
                {
                    Console.WriteLine("Integrity check failed");
                    Reset();
                    return; 
                }
            }
        }
    }
}
