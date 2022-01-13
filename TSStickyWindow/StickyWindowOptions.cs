using System;

namespace TSStickyWindow
{
    public class StickyWindowOptions
    {
        public StickyWindowOptions(int? snapOffset = null, 
            double? windowMinWidth = null, double? windowMinHeight = null,
            double? windowInitWidth = null, double? windowInitHeight = null,
            string? labelTitleName = null, string buttonUnstickName = null)
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
        /// If the main window is included it needs special handling
        /// for example don't close or open twice
        /// </summary>
        public Type? MainWindowType { get; set; }

        public string LabelTitleName { get; set; } = "LblTitle";
        public string ButtonUnstickName { get; set; } = "BtnUnstick";

    }
}