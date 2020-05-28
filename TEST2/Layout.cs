using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TEST2
{
    public class Layout
    {
        public const int MAXDISTANCE = 4;
        private RECT currentWin;
        private int intersectionIdCount = 0;
        private List<Intersection> intersections;
        private List<LayoutWindow> layoutWindows;
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
        public bool addIntersection(Intersection inter)
        {
            if (!ContainsRECT(inter.GetHitBox()))
                return false;
            intersections.Add(inter);
            foreach (LayoutWindow lw in inter.GetLayoutWindowList())
                AddLayoutWindow(lw);
            return true;
        }
        private void AddLayoutWindow(LayoutWindow lw)
        {
            foreach (LayoutWindow lw1 in layoutWindows)
            {
                if (lw1.GetHWnd() == lw.GetHWnd())
                {
                    lw1.IncrementUses();
                    return;
                }
            }
            layoutWindows.Add(lw);
        }
        public bool RemoveInterserction(Intersection inter)
        {
            bool removedInter = false;
            for (int i = 0; i < intersections.Count && !removedInter; i++)
            {

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
        public List<Intersection> getIntersections()
        {
            return intersections;
        }
        public List<LayoutWindow> getlayoutWindows()
        {
            return layoutWindows;
        }
    }
}
