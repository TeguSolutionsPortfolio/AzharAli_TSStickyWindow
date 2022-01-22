namespace TSStickyWindow
{
    public class StickyWindowOptions
    {
        public StickyWindowOptions(int? snapOffset = null, int? snapCornerOffset = null,
            double? windowMinWidth = null, double? windowMinHeight = null,
            double? windowInitWidth = null, double? windowInitHeight = null,
            string labelTitleName = null, string buttonUnstickName = null)
        {
            if (snapOffset is not null)
                SnapOffset = snapOffset.Value;
            if (snapCornerOffset is not null)
                SnapCornerOffset = snapCornerOffset.Value;

            if (windowMinWidth is not null)
                WindowMinWidth = windowMinWidth.Value;
            if (windowMinHeight is not null)
                WindowMinHeight = windowMinHeight.Value;

            if (windowInitWidth is not null)
                WindowInitialWidth = windowInitWidth.Value;
            if (windowInitHeight is not null)
                WindowInitialHeight = windowInitHeight.Value;

            if (!string.IsNullOrWhiteSpace(labelTitleName))
                LabelTitleName = labelTitleName;
            if (!string.IsNullOrWhiteSpace(buttonUnstickName))
                ButtonUnstickName = buttonUnstickName;
        }

        /// <summary>
        /// Connection Offset - the maximum distance between Windows to stick them in pixels
        /// </summary>
        public int SnapOffset { get; } = 10;

        /// <summary>
        /// Offset from the corners to trigger the magnet effect and stick
        /// </summary>
        public int SnapCornerOffset { get; } = 20;

        /// <summary>
        /// Minimal width of the window for resize, has to be bigger or equal than 0!
        /// </summary>
        public double WindowMinWidth { get; } = 50;
        /// <summary>
        /// Minimal height of the window for resize, has to be bigger or equal than 0!
        /// </summary>
        public double WindowMinHeight { get; } = 50;

        public double WindowInitialWidth { get; } = 150;
        public double WindowInitialHeight { get; } = 200;


        /// <summary>
        /// Name of the Label that drags/moves the window (change if required)
        /// </summary>
        public string LabelTitleName { get; set; }

        /// <summary>
        /// Name of the Button which triggers the Unstick event
        /// </summary>
        public string ButtonUnstickName { get; set; }

    }
}