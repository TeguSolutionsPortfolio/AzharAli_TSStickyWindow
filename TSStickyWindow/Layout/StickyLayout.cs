using System.Collections.Generic;

namespace TSStickyWindow.Layout
{
    public class StickyLayout
    {
        public StickyLayout()
        {
            Windows = new List<StickyLayoutWindow>();
        }

        public List<StickyLayoutWindow> Windows { get; set; }
    }
}