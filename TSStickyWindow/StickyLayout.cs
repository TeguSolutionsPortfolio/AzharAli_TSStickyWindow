using System;
using System.Collections.Generic;

namespace TSStickyWindow
{
    public class StickyLayout
    {
        public StickyLayout()
        {
            Windows = new List<StickyLayoutWindow>();
        }

        public List<StickyLayoutWindow> Windows { get; set; }
    }

    public class StickyLayoutWindow
    {
        public StickyLayoutWindow(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public string WindowTypeName { get; set; }

        public double PositionLeft { get; set; }
        public double PositionTop { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string ConnectionTopId { get; set; }
        public string ConnectionRightId { get; set; }
        public string ConnectionBottomId { get; set; }
        public string ConnectionLeftId { get; set; }
    }
}