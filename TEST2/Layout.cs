using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);

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
            Console.WriteLine("Adding " + inters.Count + " intersections");
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
            for (int i = 0; i < inter.GetLayoutWindowList().Count(); i++)
            {
                AddLayoutWindow(inter, i);
            }
            Console.WriteLine("-------- Intersections in the list --------");
            foreach (Intersection i in intersections)
            {
                Console.WriteLine(i.ToString());
            }
            return true;
        }
        private void AddLayoutWindow(Intersection inter, int index)
        {
            LayoutWindow lw = inter.GetLayoutWindowList()[index];
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
                        inter.GetLayoutWindowList()[index] = lw1;
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
        private List<LayoutWindow> GetlayoutWindows()
        {
            return layoutWindows;
        }
        public void CheckIntegrity()
        {
            RECT test = new RECT();
            int similarity;
            foreach (LayoutWindow lw in layoutWindows)
            {
                GetWindowRect(lw.GetHWnd(), ref test);
                similarity = Similar(test, lw.GetWindow());
                if (similarity > 0 && similarity < MAXDISTANCE)
                {
                    
                    Console.WriteLine("Window almost at the right spot: " + similarity + "pixels off\nAdjusting Data!");
                    MoveWindow(lw.GetHWnd(), lw.GetWindow().left, lw.GetWindow().top,
                        lw.GetWindow().right - lw.GetWindow().left, lw.GetWindow().bottom - lw.GetWindow().top, true);
                }
                else if (similarity > MAXDISTANCE)
                {
                    Console.WriteLine("Integrity check failed\n" + similarity + " pixels off");
                    Console.WriteLine("\t\tMem values: " + Form1.RECTToString(lw.GetWindow()));
                    Console.WriteLine("\t\tActual values: " + Form1.RECTToString(test));
                    Reset();
                    return; 
                }
            }
        }
        private int Similar(RECT r1, RECT r2)
        {
            int farthest = Math.Abs(r1.top - r2.top);
            int buffer = Math.Abs(r1.right - r2.right);
            if (buffer > farthest)
                farthest = buffer;
            buffer = Math.Abs(r1.bottom - r2.bottom);
            if (buffer > farthest)
                farthest = buffer;
            buffer = Math.Abs(r1.left - r2.left);
            if (buffer > farthest)
                farthest = buffer;
            return farthest;
        }
    }
}
