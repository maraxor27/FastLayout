using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TEST2
{
    public class Intersection
    {
        private static int idCounter = 0;
        private readonly int id;
        private RECT hitBox;
        private List<LayoutWindow> layoutWindowList;

        public Intersection(RECT r, List<LayoutWindow> lw_list)
        {
            id = idCounter++;
            hitBox = r;
            layoutWindowList = lw_list;
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
    }
}
