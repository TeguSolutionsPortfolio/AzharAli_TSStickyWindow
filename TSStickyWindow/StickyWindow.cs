using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TSStickyWindow
{
    internal class StickyWindow
    {
        private readonly StickyWindowService service;
        private readonly StickyWindowOptions options;

        internal string Id { get; set; }

        #region Constructor

        public StickyWindow(StickyWindowService stickyService,
            StickyWindowOptions stickyOptions,
            Window window)
        {
            service = stickyService;
            options = stickyOptions;
            this.window = window;

            this.window.LocationChanged += WindowOnLocationChanged;
            this.window.SizeChanged += WindowOnSizeChanged;
            this.window.Closing += WindowOnClosing;

            SetWindowControls();

            Id = service.GetNextId();
            lblTitle!.Content = Id;

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
            if (width < options.WindowMinWidth && width >= 0)
                window.Width = options.WindowMinWidth;
            else
                window.Width = width;
        }
        internal void SetWindowWidthDiff(double dWidth)
        {
            SetWindowWidth(window.Width + dWidth);
        }
        internal void SetWindowHeight(double height)
        {
            if (height < options.WindowMinHeight && height >= 0)
                window.Height = options.WindowMinHeight;
            else
                window.Height = height;
        }
        internal void SetWindowHeightDiff(double dHeight)
        {
            SetWindowHeight(window.Height + dHeight);
        }

        internal void SetWindowPosition(double left, double top)
        {
            window.Left = left;
            window.Top = top;
        }
        internal void SetWindowPositionDiff(double dLeft, double dTop)
        {
            SetWindowPosition(window.Left + dLeft, window.Top + dTop);
        }

        internal void ShowWindow()
        {
            if (window.GetType() != options.MainWindowType)
                window.Show();
        }
        internal void CloseWindow()
        {
            if (window.GetType() != options.MainWindowType)
                window.Close();
        }

        #endregion

        #region Window Position Controls (for testing)

        // Production controls
        private Button btnUnstick;

        // Test Controls (removable in production)
        private Border brdTop;
        private Border brdRight;
        private Border brdBottom;
        private Border brdLeft;

        private Label lblTitle;

        private Label lblConnectionTopId;
        private Label lblConnectionRightId;
        private Label lblConnectionBottomId;
        private Label lblConnectionLeftId;

        private Label positionLeft;
        private Label positionTop;
        private Label positionRight;
        private Label positionBottom;

        private void SetWindowControls()
        {
            try
            {
                // Production controls
                btnUnstick = window.FindName("BtnUnstick") as Button ?? new Button();
                btnUnstick.Click += StartUnstickWindows;

                // Test Controls (removable in production)
                brdTop = window.FindName("BrdTop") as Border ?? new Border();
                brdRight = window.FindName("BrdRight") as Border ?? new Border();
                brdBottom = window.FindName("BrdBottom") as Border ?? new Border();
                brdLeft = window.FindName("BrdLeft") as Border ?? new Border();

                lblTitle = window.FindName("LblTitle") as Label ?? new Label();

                lblConnectionTopId = window.FindName("LblConnectionTopId") as Label ?? new Label();
                lblConnectionRightId = window.FindName("LblConnectionRightId") as Label ?? new Label();
                lblConnectionBottomId = window.FindName("LblConnectionBottomId") as Label ?? new Label();
                lblConnectionLeftId = window.FindName("LblConnectionLeftId") as Label ?? new Label();

                positionLeft = window.FindName("LblPositionLeft") as Label ?? new Label();
                positionTop = window.FindName("LblPositionTop") as Label ?? new Label();
                positionRight = window.FindName("LblPositionRight") as Label ?? new Label();
                positionBottom = window.FindName("LblPositionBottom") as Label ?? new Label();

                // Custom event handlers
                lblTitle.PreviewMouseLeftButtonDown += LblTitleMouseLeftButtonDown;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void UpdatePositionLabels()
        {
            try
            {
                positionLeft.Content = Left.ToString("0");
                positionTop.Content = Top.ToString("0");
                positionRight.Content = Right.ToString("0");
                positionBottom.Content = Bottom.ToString("0");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        #endregion

        #region Window Events

        private bool isSticking;

        private void LblTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            SetLastWindowPosition();

            //Window.OnMouseLeftButtonDown(e);
            window.DragMove();

            isSticking = true;
            service.TryStickWithOtherWindows(this);
            isSticking = false;
        }

        private void WindowOnLocationChanged(object? sender, EventArgs e)
        {
            UpdatePositionLabels();

            if (!isSticking && window.IsActive)
            {
                var deltaX = window.Left - lastLeft;
                var deltaY = window.Top - lastTop;

                //Debug.WriteLine("Window: " + Id + "  - dX: " + deltaX + " | dY: " + deltaY);

                service.RepositionStickedWindows(this, deltaX, deltaY);
                SetLastWindowPosition();
            }
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!window.IsActive)
                return;

            if (e.HeightChanged)
            {
                if (e.NewSize.Height <= options.WindowMinHeight)
                {
                    window.Height = options.WindowMinHeight;
                    return;
                }

                service.ResizeStickedWindowsHeight(this, e.NewSize.Height - e.PreviousSize.Height);
            }
            else if (e.WidthChanged)
            {
                if (e.NewSize.Width <= options.WindowMinWidth)
                {
                    window.Width = options.WindowMinWidth;
                    return;
                }

                service.ResizeStickedWindowsWidth(this, e.NewSize.Width - e.PreviousSize.Width);
            }

            UpdatePositionLabels();
        }

        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            service.TryUnstickWithOtherWindows(this);
            service.RemoveWindow(this);

            // Cleanup the events
            window.LocationChanged -= WindowOnLocationChanged;
            window.SizeChanged -= WindowOnSizeChanged;
            window.Closing -= WindowOnClosing;
        }

        #endregion

        #region Window Position

        private double lastLeft;
        private double lastTop;

        private void SetLastWindowPosition()
        {
            lastLeft = window.Left;
            lastTop = window.Top;
        }

        public double Left => window.Left;
        public double Right => window.Left + window.Width;
        public double Top => window.Top;
        public double Bottom => window.Top + window.Height;

        public double Width => window.Width;
        public double Height => window.Height;

        #endregion

        #region Sticked Windows Management

        internal StickyWindow? StickTop { get; set; }
        internal StickyWindow? StickRight { get; set; }
        internal StickyWindow? StickBottom { get; set; }
        internal StickyWindow? StickLeft { get; set; }

        internal bool CanStickWindow(StickyWindow source, StickPosition position)
        {
            if (StickTop == source || StickRight == source || StickBottom == source || StickLeft == source)
                return false;


            if (position == StickPosition.Top)
            {
                if (StickTop is not null)
                    return false;
            }

            if (position == StickPosition.Right)
            {
                if (StickRight is not null)
                    return false;
            }

            if (position == StickPosition.Bottom)
            {
                if (StickBottom is not null)
                    return false;
            }

            if (position == StickPosition.Left)
            {
                if (StickLeft is not null)
                    return false;
            }

            return true;
        }
        // !! Use after the CanStickWindow validation !!
        internal void StickWindow(StickyWindow targetWindow, StickPosition position, bool arrange = false)
        {
            if (position == StickPosition.Top)
            {
                StickTop = targetWindow;

                if (arrange)
                {
                    window.Left = targetWindow.Left;
                    window.Top = targetWindow.Bottom;
                }
            }

            else if (position == StickPosition.Right)
            {
                StickRight = targetWindow;

                if (arrange)
                {
                    window.Left = targetWindow.Left - window.Width;
                    window.Top = targetWindow.Top;
                }
            }

            else if (position == StickPosition.Bottom)
            {
                StickBottom = targetWindow;

                if (arrange)
                {
                    window.Left = targetWindow.Left;
                    window.Top = targetWindow.Top - window.Height;
                }
            }

            else if (position == StickPosition.Left)
            {
                StickLeft = targetWindow;

                if (arrange)
                {
                    window.Left = targetWindow.Left + targetWindow.window.Width;
                    window.Top = targetWindow.Top;
                }
            }

            HighlightStickState();
        }

        internal void UnstickWindow(StickyWindow targetWindow)
        {
            if (StickTop == targetWindow)
                StickTop = null;
            else if (StickRight == targetWindow)
                StickRight = null;
            else if (StickBottom == targetWindow)
                StickBottom = null;
            else if (StickLeft == targetWindow)
                StickLeft = null;

            HighlightStickState();
        }

        private void HighlightStickState()
        {
            if (StickTop is null)
            {
                brdTop.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionTopId.Content = "";
            }
            else
            {
                brdTop.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionTopId.Content = StickTop.Id;
            }

            if (StickRight is null)
            {
                brdRight.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionRightId.Content = "";
            }
            else
            {
                brdRight.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionRightId.Content = StickRight.Id;
            }

            if (StickBottom is null)
            {
                brdBottom.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionBottomId.Content = "";
            }
            else
            {
                brdBottom.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionBottomId.Content = StickBottom.Id;
            }

            if (StickLeft is null)
            {
                brdLeft.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionLeftId.Content = "";
            }
            else
            {
                brdLeft.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionLeftId.Content = StickLeft.Id;
            }

            if (StickTop is not null || StickRight is not null || StickBottom is not null || StickLeft is not null)
                btnUnstick.Visibility = Visibility.Visible;
            else
                btnUnstick.Visibility = Visibility.Collapsed;
        }

        private void StartUnstickWindows(object sender, RoutedEventArgs e)
        {
            service.TryUnstickWithOtherWindows(this);
        }

        internal List<StickyWindow> GetAllStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            if (StickTop is not null && !existingWindows.Contains(StickTop))
                newWindows.Add(StickTop);

            if (StickRight is not null && !existingWindows.Contains(StickRight))
                newWindows.Add(StickRight);

            if (StickBottom is not null && !existingWindows.Contains(StickBottom))
                newWindows.Add(StickBottom);

            if (StickLeft is not null && !existingWindows.Contains(StickLeft))
                newWindows.Add(StickLeft);

            return newWindows;
        }

        internal List<StickyWindow> GetHorizontalStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            if (StickRight is not null && !existingWindows.Contains(StickRight))
                newWindows.Add(StickRight);

            if (StickLeft is not null && !existingWindows.Contains(StickLeft))
                newWindows.Add(StickLeft);

            return newWindows;
        }

        internal List<StickyWindow> GetVerticalStickedWindows(List<StickyWindow> existingWindows)
        {
            var newWindows = new List<StickyWindow>();

            if (StickTop is not null && !existingWindows.Contains(StickTop))
                newWindows.Add(StickTop);

            if (StickBottom is not null && !existingWindows.Contains(StickBottom))
                newWindows.Add(StickBottom);

            return newWindows;
        }

        #endregion
    }
}