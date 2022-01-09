namespace TSStickyWindow
{
    public class StickyWindowOptions
    {
        public StickyWindowOptions(int? snapOffset = null, double? windowMinHeight = null, double? windowMinWidth = null)
        {
            if (snapOffset is not null)
                SnapOffset = snapOffset.Value;

            if (windowMinHeight is not null)
                WindowMinHeight = windowMinHeight.Value;

            if (windowMinWidth is not null)
                WindowMinWidth = windowMinWidth.Value;
        }

        /// <summary>
        /// Connection Offset - the maximum distance between Windows to stick them in pixels
        /// </summary>
        public int SnapOffset { get; } = 10;


        public double WindowMinHeight { get; } = 50;


        public double WindowMinWidth { get; } = 50;
    }
}