namespace TSStickyWindow.Layout
{
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