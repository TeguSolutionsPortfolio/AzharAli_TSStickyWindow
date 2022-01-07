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

                if (source.StickedWindows.Contains(target))
                    continue;

                // Right edge connection
                if (Math.Abs(target.Right - source.Left) < offset && 
                    target.Top <= source.Bottom && 
                    target.Bottom >= source.Top)
                {
                    source.StickWindow(target);
                    target.StickWindow(source);
                }
            }
        }

        internal void TryUnstickWithOtherWindows(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target == source)
                    continue;

                if (source.StickedWindows.Contains(target))
                {
                    source.UnstickWindow(target);
                    target.UnstickWindow(source);
                }
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
            StickedWindows = new List<StickyWindow>();

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

        internal List<StickyWindow> StickedWindows { get; }

        internal void StickWindow(StickyWindow window)
        {
            if (!StickedWindows.Contains(window))
                StickedWindows.Add(window);

            HighlightStickState();
        }

        internal void UnstickWindow(StickyWindow window)
        {
            StickedWindows.Remove(window);
            HighlightStickState();
        }

        private void HighlightStickState()
        {
            if (StickedWindows.Count > 0)
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
}
