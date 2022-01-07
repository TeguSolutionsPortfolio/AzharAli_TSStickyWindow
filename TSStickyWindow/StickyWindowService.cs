using System;
using System.Collections.Generic;
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

        internal void TryConnectWithOtherWindows(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target == source)
                    continue;

                if (source.ConnectedWindows.Contains(target))
                    continue;

                // Right edge connection
                if (Math.Abs(target.Right - source.Left) < offset && 
                    target.Top <= source.Bottom && 
                    target.Bottom >= source.Top)
                {
                    source.Window.BorderBrush = new SolidColorBrush(Colors.Red);
                }
            }
        }
    }

    public class StickyWindow
    {
        private StickyWindowService service;


        public StickyWindow(StickyWindowService windowService, Window window)
        {
            service = windowService;
            Window = window;
            ConnectedWindows = new List<StickyWindow>();

            Window.LocationChanged += WindowOnLocationChanged;

            positionLeft = Window.FindName("LblPositionLeft") as Label;
            positionTop = Window.FindName("LblPositionTop") as Label;
            positionRight = Window.FindName("LblPositionRight") as Label;
            positionBottom = Window.FindName("LblPositionBottom") as Label;


            Window.Show();
        }

        private Label positionLeft;
        private Label positionTop;
        private Label positionRight;
        private Label positionBottom;

        private void WindowOnLocationChanged(object? sender, EventArgs e)
        {
            positionLeft.Content = Left.ToString("0");
            positionTop.Content = Top.ToString("0");
            positionRight.Content = Right.ToString("0");
            positionBottom.Content = Bottom.ToString("0");

            service.TryConnectWithOtherWindows(this);
        }

        public Window Window { get; }

        public List<StickyWindow> ConnectedWindows { get; }

        public double Left => Window.Left;
        public double Right => Window.Left + Window.Width;
        public double Top => Window.Top;
        public double Bottom => Window.Top + Window.Height;


    }
}
