using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TSStickyWindow.Definitions;
using TSStickyWindow.Development;

namespace TSStickyWindow
{
    internal class StickyWindow
    {
        private readonly StickyWindowService service;
        private readonly StickyWindowOptions options;
        private readonly StickyWindowTestControls testControls;

        /// <summary>
        /// Unique Id of the StickyWindow object, currently it's generated by the StickyWindowService,
        /// but it can be passed from outside as well
        /// </summary>
        internal string Id { get; set; }

        #region Constructor

        public StickyWindow(StickyWindowService stickyService,
            StickyWindowOptions stickyOptions,
            Window window,
            string id,
            bool? testMode = null)
        {
            Id = id;
            service = stickyService;
            options = stickyOptions;

            if (testMode == true)
                testControls = new StickyWindowTestControls(this, window);

            this.window = window;

            this.window.LocationChanged += WindowOnLocationChanged;
            this.window.SizeChanged += WindowOnSizeChanged;
            this.window.Closing += WindowOnClosing;

            SetWindowControls();

            lblTitle.Content = Id;

            Stick = new Dictionary<StickPosition, StickyWindow>
            {
                [StickPosition.Top] = null,
                [StickPosition.Right] = null,
                [StickPosition.Bottom] = null,
                [StickPosition.Left] = null
            };

            this.window.Show();
        }

        #endregion


        #region Window & Control

        private Window window { get; }

        internal string GetWindowType()
        {
            return window.GetType().Name;
        }
        internal void SetWindowWidth(double width)
        {
            if (width < options.WindowMinWidth/* && width >= 0*/)
                window.Width = options.WindowMinWidth;
            else
                window.Width = width;
        }

        internal List<string> SetWindowWidthDiff(List<string> handledIds, double dWidth)
        {
            if (handledIds.Contains(Id))
                return handledIds;
            handledIds.Add(Id);

            SetWindowWidth(window.Width + dWidth);

            if (Stick[StickPosition.Left] is not null)
                handledIds = Stick[StickPosition.Left].SetWindowWidthDiff(handledIds, dWidth);
            if (Stick[StickPosition.Right] is not null)
                handledIds = Stick[StickPosition.Right].SetWindowWidthDiff(handledIds, dWidth);

            return handledIds;
        }
        internal void SetWindowHeight(double height)
        {
            if (height < options.WindowMinHeight /*&& height >= 0*/)
                window.Height = options.WindowMinHeight;
            else
                window.Height = height;
        }
        internal List<string> SetWindowHeightDiff(List<string> handledIds,double dHeight)
        {
            if (handledIds.Contains(Id))
                return handledIds;
            handledIds.Add(Id);

            SetWindowHeight(window.Height + dHeight);

            if (Stick[StickPosition.Top] is not null)
                handledIds = Stick[StickPosition.Top].SetWindowHeightDiff(handledIds, dHeight);
            if (Stick[StickPosition.Bottom] is not null)
                handledIds = Stick[StickPosition.Bottom].SetWindowHeightDiff(handledIds, dHeight);

            return handledIds;
        }

        internal void SetWindowPosition(double left, double top)
        {
            window.Left = left;
            window.Top = top;
        }
        internal List<string> SetWindowPositionDiff(List<string> handledIds, double deltaX, double deltaY)
        {
            if (handledIds.Contains(Id))
                return handledIds;
            handledIds.Add(Id);

            SetWindowPosition(window.Left + deltaX, window.Top + deltaY);

            foreach (var (_, stickyWindow) in Stick)
            {
                if (stickyWindow is not null)
                    handledIds = stickyWindow.SetWindowPositionDiff(handledIds, deltaX, deltaY);
            }

            return handledIds;
        }

        internal void ShowWindow()
        {
            window.Show();
        }
        internal void CloseWindow()
        {
            window.Close();
        }

        #endregion

        #region Window Position Controls (for testing)

        // Production controls
        private Label lblTitle;
        private Button btnUnstick;

        private void SetWindowControls()
        {
            try
            {
                // Production controls
                btnUnstick = window.FindName(options.ButtonUnstickName) as Button ?? new Button();
                btnUnstick.Click += StartUnstickWindows;

                lblTitle = window.FindName(options.LabelTitleName) as Label ?? new Label();

                // Custom event handlers
                lblTitle.PreviewMouseLeftButtonDown += LblTitleMouseLeftButtonDown;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        #region Window Events

        private bool isSticking;

        private void LblTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            SetLastWindowPosition(true);

            //Window.OnMouseLeftButtonDown(e);
            window.DragMove();

            isSticking = true;
            service.TryStickWithOtherWindows(this);
            isSticking = false;
        }

        private void WindowOnLocationChanged(object sender, EventArgs e)
        {
            // Filter out the accidental changes caused by Size change!!!
            if (Math.Abs(lastWidthAfterLocationChange - window.Width) > 0.01 ||
                Math.Abs(lastHeightAfterLocationChange - window.Height) > 0.01)
                return;

            testControls?.UpdatePositionLabels();

            if (window.IsActive)
                service.TryMagnetWithUnstickedWindows(this);

            if (!isSticking && window.IsActive)
            {
                var deltaX = window.Left - lastLeft;
                var deltaY = window.Top - lastTop;

                var handledIds = new List<string> { Id };
                foreach (var (_, stickyWindow) in Stick.Where(s => s.Value is not null))
                {
                    handledIds = stickyWindow.SetWindowPositionDiff(handledIds, deltaX, deltaY);
                }
            }

            SetLastWindowPosition(true);
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!window.IsActive)
            {
                SetLastWindowPosition(false);
                return;
            }

            if (e.HeightChanged)
                HandleWindowHeightChanged(e.PreviousSize, e.NewSize);
            else if (e.WidthChanged)
                HandleWindowWidthChanged(e.PreviousSize, e.NewSize);

            SetLastWindowPosition(false);
            testControls?.UpdatePositionLabels();
        }

        private void HandleWindowHeightChanged(Size prevSize, Size newSize)
        {
            if (newSize.Height <= options.WindowMinHeight)
            {
                window.Height = options.WindowMinHeight;
                return;
            }

            var handledSizeIds = new List<string> { Id };
            var handledPositionIds = new List<string> { Id };

            // Resized from the Top
            if (Math.Abs(window.Top - lastTop) > 0.001)
            {
                // Resize the horizontal windows
                handledSizeIds = Stick[StickPosition.Left]?.SetWindowHeightDiff(handledSizeIds, Top - lastTop);
                handledSizeIds = Stick[StickPosition.Right]?.SetWindowHeightDiff(handledSizeIds, Top - lastTop);

                // Move the top windows
                Stick[StickPosition.Top]?.SetWindowPositionDiff(handledPositionIds, 0, Top - lastTop);
            }
            // Resized from the Bottom
            else
            {
                // Resize the horizontal windows
                handledSizeIds = Stick[StickPosition.Left]?.SetWindowHeightDiff(handledSizeIds, Bottom - lastBottom);
                handledSizeIds = Stick[StickPosition.Right]?.SetWindowHeightDiff(handledSizeIds, Bottom - lastBottom);

                // Move the bottom windows
                Stick[StickPosition.Bottom]?.SetWindowPositionDiff(handledPositionIds, 0, Bottom - lastBottom);
            }
        }
        private void HandleWindowWidthChanged(Size prevSize, Size newSize)
        {
            if (newSize.Width <= options.WindowMinWidth)
            {
                window.Width = options.WindowMinWidth;
                return;
            }

            var handledSizeIds = new List<string> { Id };
            var handledPositionIds = new List<string> { Id };

            // Resized from the Left
            if (Math.Abs(window.Left - lastLeft) > 0.001)
            {
                // Resize the vertical windows
                handledSizeIds = Stick[StickPosition.Top]?.SetWindowWidthDiff(handledSizeIds, Left - lastLeft);
                handledSizeIds = Stick[StickPosition.Bottom]?.SetWindowWidthDiff(handledSizeIds, Left - lastLeft);

                // Move the left windows
                Stick[StickPosition.Left]?.SetWindowPositionDiff(handledPositionIds, Left - lastLeft, 0);
            }
            // Resized from the Right
            else
            {
                // Resize the vertical windows
                handledSizeIds = Stick[StickPosition.Top]?.SetWindowWidthDiff(handledSizeIds, Right - lastRight);
                handledSizeIds = Stick[StickPosition.Bottom]?.SetWindowWidthDiff(handledSizeIds, Right - lastRight);

                // Move the right windows
                Stick[StickPosition.Right]?.SetWindowPositionDiff(handledPositionIds, Right - lastRight, 0);
            }
        }


        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            UnsticFromAllWindows();
            service.RemoveWindow(this);

            // Cleanup the events
            window.LocationChanged -= WindowOnLocationChanged;
            window.SizeChanged -= WindowOnSizeChanged;
            window.Closing -= WindowOnClosing;
        }

        #endregion

        #region Window Position

        private double lastWidthAfterLocationChange;
        private double lastHeightAfterLocationChange;

        private double lastLeft;
        private double lastTop;
        private double lastRight;
        private double lastBottom;

        /// <summary>
        /// Call after Location change & Size change
        /// </summary>
        private void SetLastWindowPosition(bool fromLocation)
        {
            if (fromLocation)
            {
                lastWidthAfterLocationChange = window.Width;
                lastHeightAfterLocationChange = window.Height;
            }

            lastLeft = window.Left;
            lastTop = window.Top;
            lastRight = window.Left + window.Width;
            lastBottom = window.Top + window.Height;
        }

        public double Left => window.Left;
        public double Right => window.Left + window.Width;
        public double Top => window.Top;
        public double Bottom => window.Top + window.Height;

        public double Width => window.Width;
        public double Height => window.Height;

        #endregion

        #region Sticked Windows Management

        internal Dictionary<StickPosition, StickyWindow> Stick { get; set; }

        internal bool CanStickWindow(StickyWindow source, StickPosition position)
        {
            if (Stick.ContainsValue(source))
                return false;

            if (Stick[position] is not null)
                return false;

            return true;
        }
        // !! Use after the CanStickWindow validation !!
        internal void StickWindow(StickyWindow targetWindow, StickPosition position, bool arrange = false)
        {
            Stick[position] = targetWindow;

            if (arrange)
            {
                if (position == StickPosition.Top)
                    window.Top = targetWindow.Bottom;
                
                else if (position == StickPosition.Right)
                    window.Left = targetWindow.Left - window.Width;
                
                else if (position == StickPosition.Bottom)
                    window.Top = targetWindow.Top - window.Height;
                
                else if (position == StickPosition.Left)
                    window.Left = targetWindow.Right;
            }

            HighlightStickState();
        }

        /// <summary>
        /// Called from the Source window, to unstick it from the target, based on the relative Position
        /// </summary>
        /// <param name="position"></param>
        internal void UnstickWindow(StickPosition position)
        {
            Stick[position] = null;

            HighlightStickState();
        }
        internal void UnsticFromAllWindows()
        {
            Stick[StickPosition.Top]?.UnstickWindow(StickPosition.Bottom);
            Stick[StickPosition.Right]?.UnstickWindow(StickPosition.Left);
            Stick[StickPosition.Bottom]?.UnstickWindow(StickPosition.Top);
            Stick[StickPosition.Left]?.UnstickWindow(StickPosition.Right);

            service.InvokeWindowsUnsticked(Id, 
                Stick[StickPosition.Top]?.Id ?? "", 
                Stick[StickPosition.Right]?.Id ?? "",
                Stick[StickPosition.Bottom]?.Id ?? "", 
                Stick[StickPosition.Left]?.Id ?? ""
            );

            Stick[StickPosition.Top] = null;
            Stick[StickPosition.Right] = null;
            Stick[StickPosition.Bottom] = null;
            Stick[StickPosition.Left] = null;

            HighlightStickState();
        }

        private void HighlightStickState()
        {
            testControls?.HighlightStickState();

            btnUnstick.Visibility = Stick.Values.Any(s => s != null) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void StartUnstickWindows(object sender, RoutedEventArgs e)
        {
            UnsticFromAllWindows();
        }

        internal List<StickyWindow> GetAllStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            foreach (var (_, swindow) in Stick)
            {
                if (swindow is not null && !existingWindows.Contains(swindow))
                    newWindows.Add(swindow);
            }

            return newWindows;
        }

        internal List<StickyWindow> GetHorizontalStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            if (Stick[StickPosition.Right] is not null && !existingWindows.Contains(Stick[StickPosition.Right]))
                newWindows.Add(Stick[StickPosition.Right]);

            if (Stick[StickPosition.Left] is not null && !existingWindows.Contains(Stick[StickPosition.Left]))
                newWindows.Add(Stick[StickPosition.Left]);

            return newWindows;
        }

        internal List<StickyWindow> GetVerticalStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            if (Stick[StickPosition.Top] is not null && !existingWindows.Contains(Stick[StickPosition.Top]))
                newWindows.Add(Stick[StickPosition.Top]);

            if (Stick[StickPosition.Bottom] is not null && !existingWindows.Contains(Stick[StickPosition.Bottom]))
                newWindows.Add(Stick[StickPosition.Bottom]);

            return newWindows;
        }

        #endregion

        #region Magnet

        internal void SetMagnetPosition(double? top, double? right, double? bottom, double? left)
        {
            if (top is not null)
                window.Top = top.Value;
            else if (right is not null)
                window.Left = right.Value - Width;
            else if (bottom is not null)
                window.Top = bottom.Value - Height;
            else if (left is not null)
                window.Left = left.Value;
        }

        #endregion
    }
}