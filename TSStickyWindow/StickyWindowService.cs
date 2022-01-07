using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TSStickyWindow
{
    /// <summary>
    /// Plan: make it as a singleton service and pass the windows here
    /// The inner logic will manage the window positions and others
    /// </summary>
    public class StickyWindowService
    {
        /// <summary>
        /// Connection Offset - the maximum distance between Windows to stick them in pixels
        /// </summary>
        private const int offset = 10;

        /// <summary>
        /// Lazy solution, better to use dependency injection in production
        /// </summary>
        public static StickyWindowService Instance { get; } = new();

        private readonly List<StickyWindow> windows;

        #region Init

        public StickyWindowService()
        {
            windows = new List<StickyWindow>();
        }

        #endregion

        public void AddNewWindow(Window window)
        {
            windows.Add(new StickyWindow(this, window));
        }

        internal void TryStickWithOtherWindows(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target == source)
                    continue;

                // Top edge
                if (Math.Abs(target.Top - source.Bottom) < offset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Top) && target.CanStickWindow(source, StickPosition.Bottom))
                    {
                        source.StickWindow(target, StickPosition.Top);
                        target.StickWindow(source, StickPosition.Bottom);
                    }
                }

                // Right edge
                else if (Math.Abs(target.Right - source.Left) < offset && 
                    target.Top <= source.Bottom && 
                    target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Right) && target.CanStickWindow(source, StickPosition.Left))
                    {
                        source.StickWindow(target, StickPosition.Right);
                        target.StickWindow(source, StickPosition.Left);
                    }
                }

                // Bottom edge
                else if (Math.Abs(target.Bottom - source.Top) < offset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Bottom) && target.CanStickWindow(source, StickPosition.Top))
                    {
                        source.StickWindow(target, StickPosition.Bottom);
                        target.StickWindow(source, StickPosition.Top);
                    }
                }

                // Left edge
                else if (Math.Abs(target.Left - source.Right) < offset &&
                    target.Top <= source.Bottom &&
                    target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Left) && target.CanStickWindow(source, StickPosition.Right))
                    {
                        source.StickWindow(target, StickPosition.Left);
                        target.StickWindow(source, StickPosition.Right);
                    }
                }
            }
        }

        internal void TryUnstickWithOtherWindows(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target == source)
                    continue;

                source.UnstickWindow(target);
                target.UnstickWindow(source);
            }
        }
    }

    public class StickyWindow
    {
        private readonly StickyWindowService service;

        #region Constructor

        public StickyWindow(StickyWindowService windowService, Window window)
        {
            service = windowService;
            Window = window;

            Window.LocationChanged += WindowOnLocationChanged;

            SetWindowControls();

            Window.Show();
        }

        #endregion

        internal Window Window { get; }

        #region Window Controls

        // Production controls
        private Button btnUnstick;

        // Test Controls (removable in production)
        private Border mainBorder;
        private Label positionLeft;
        private Label positionTop;
        private Label positionRight;
        private Label positionBottom;

        private void SetWindowControls()
        {
            try
            {
                // Production controls
                btnUnstick = Window.FindName("BtnUnstick") as Button ?? new Button();
                btnUnstick.Click += StartUnstickWindows;

                // Test Controls (removable in production)
                mainBorder = Window.FindName("MainBorder") as Border ?? new Border();
                positionLeft = Window.FindName("LblPositionLeft") as Label ?? new Label();
                positionTop = Window.FindName("LblPositionTop") as Label ?? new Label();
                positionRight = Window.FindName("LblPositionRight") as Label ?? new Label();
                positionBottom = Window.FindName("LblPositionBottom") as Label ?? new Label();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #endregion

        #region Window Events

        private void WindowOnLocationChanged(object? sender, EventArgs e)
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

            service.TryStickWithOtherWindows(this);
        }

        #endregion

        #region Window Position

        public double Left => Window.Left;
        public double Right => Window.Left + Window.Width;
        public double Top => Window.Top;
        public double Bottom => Window.Top + Window.Height;

        #endregion

        #region Sticked Windows Management

        //internal List<(StickyWindow window, StickPosition position)> StickedWindows { get; }
        //internal List<KeyValuePair<StickyWindow, StickPosition>> StickedWindows { get; }
        //internal Dictionary<StickPosition, StickyWindow?> StickedWindows { get; }

        internal StickyWindow? StickTop { get; set; }
        internal StickyWindow? StickRight { get; set; }
        internal StickyWindow? StickBottom { get; set; }
        internal StickyWindow? StickLeft { get; set; }

        internal bool CanStickWindow(StickyWindow window, StickPosition position)
        {
            if (StickTop == window || StickRight == window || StickBottom == window || StickLeft == window)
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
        internal void StickWindow(StickyWindow window, StickPosition position)
        {
            if (position == StickPosition.Top)
                StickTop = window;

            else if (position == StickPosition.Right)
                StickRight = window;

            else if (position == StickPosition.Bottom)
                StickBottom = window;

            else if (position == StickPosition.Left)
                StickLeft = window;

            HighlightStickState();
        }

        internal void UnstickWindow(StickyWindow window)
        {
            if (StickTop == window)
                StickTop = null;
            else if (StickRight == window)
                StickRight = null;
            else if (StickBottom == window)
                StickBottom = null;
            else if (StickLeft == window)
                StickLeft = null;

            HighlightStickState();
        }

        private void HighlightStickState()
        {
            if (StickTop is not null || StickRight is not null || StickBottom is not null || StickLeft is not null)
            {
                mainBorder.BorderBrush = new SolidColorBrush(Colors.Red);
                btnUnstick.Visibility = Visibility.Visible;
            }
            else
            {
                mainBorder.BorderBrush = new SolidColorBrush(Colors.Lime);
                btnUnstick.Visibility = Visibility.Collapsed;
            }
        }

        private void StartUnstickWindows(object sender, RoutedEventArgs e)
        {
            service.TryUnstickWithOtherWindows(this);
        }

        #endregion
    }

    public enum StickPosition
    {
        Top = 0,
        Right = 1,
        Bottom = 2,
        Left = 3
    }
}
