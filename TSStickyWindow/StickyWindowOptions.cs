namespace TSStickyWindow
{
    public class StickyWindowOptions
    {
        public StickyWindowOptions(int? snapOffset = null, 
            double? windowMinWidth = null, double? windowMinHeight = null,
            double? windowInitWidth = null, double? windowInitHeight = null)
        {
            if (snapOffset is not null)
                SnapOffset = snapOffset.Value;

            if (windowMinWidth is not null)
                WindowMinWidth = windowMinWidth.Value;
            if (windowMinHeight is not null)
                WindowMinHeight = windowMinHeight.Value;

            if (windowInitWidth is not null)
                WindowInitialWidth = windowInitWidth.Value;
            if (windowInitHeight is not null)
                WindowInitialHeight = windowInitHeight.Value;
        }

        /// <summary>
        /// Connection Offset - the maximum distance between Windows to stick them in pixels
        /// </summary>
        public int SnapOffset { get; } = 10;

        public double WindowMinWidth { get; } = 50;
        public double WindowMinHeight { get; } = 50;

        public double WindowInitialWidth { get; } = 150;
        public double WindowInitialHeight { get; } = 200;

    }
}