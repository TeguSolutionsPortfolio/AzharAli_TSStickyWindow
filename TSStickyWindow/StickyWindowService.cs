using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        #region Id Service

        private int biggestGivenId;

        internal string GetNextId()
        {
            biggestGivenId ++;
            return biggestGivenId.ToString();
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

                // Source Top Edge
                if (Math.Abs(target.Bottom - source.Top) < offset &&
                         target.Left <= source.Right &&
                         target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Top) && target.CanStickWindow(source, StickPosition.Bottom))
                    {
                        source.StickWindow(target, StickPosition.Top, true);
                        target.StickWindow(source, StickPosition.Bottom);
                    }
                }

                // Source Right Edge
                else if (Math.Abs(target.Left - source.Right) < offset &&
                         target.Top <= source.Bottom &&
                         target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Right) && target.CanStickWindow(source, StickPosition.Left))
                    {
                        source.StickWindow(target, StickPosition.Right, true);
                        target.StickWindow(source, StickPosition.Left);
                    }
                }

                // Source Bottom Edge
                if (Math.Abs(target.Top - source.Bottom) < offset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Bottom) && target.CanStickWindow(source, StickPosition.Top))
                    {
                        source.StickWindow(target, StickPosition.Bottom, true);
                        target.StickWindow(source, StickPosition.Top);
                    }
                }

                // Source Left Edge
                else if (Math.Abs(target.Right - source.Left) < offset && 
                    target.Top <= source.Bottom && 
                    target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Left) && target.CanStickWindow(source, StickPosition.Right))
                    {
                        source.StickWindow(target, StickPosition.Left, true);
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

        internal void RemoveWindow(StickyWindow window)
        {
            windows.Remove(window);
        }
    }

    public class StickyWindow
    {
        private readonly StickyWindowService service;

        internal string Id { get; set; }

        #region Constructor

        public StickyWindow(StickyWindowService windowService, Window window)
        {
            service = windowService;
            this.Window = window;

            this.Window.LocationChanged += WindowOnLocationChanged;
            this.Window.Closing += WindowOnClosing;

            SetWindowControls();

            Id = service.GetNextId();
            lblTitle!.Content = Id;

            this.Window.Show();
        }

        #endregion

        internal Window Window { get; }

        #region Window Controls

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
                btnUnstick = Window.FindName("BtnUnstick") as Button ?? new Button();
                btnUnstick.Click += StartUnstickWindows;

                // Test Controls (removable in production)
                brdTop = Window.FindName("BrdTop") as Border ?? new Border();
                brdRight = Window.FindName("BrdRight") as Border ?? new Border();
                brdBottom = Window.FindName("BrdBottom") as Border ?? new Border();
                brdLeft = Window.FindName("BrdLeft") as Border ?? new Border();

                lblTitle = Window.FindName("LblTitle") as Label ?? new Label();

                lblConnectionTopId = Window.FindName("LblConnectionTopId") as Label ?? new Label();
                lblConnectionRightId = Window.FindName("LblConnectionRightId") as Label ?? new Label();
                lblConnectionBottomId = Window.FindName("LblConnectionBottomId") as Label ?? new Label();
                lblConnectionLeftId = Window.FindName("LblConnectionLeftId") as Label ?? new Label();

                positionLeft = Window.FindName("LblPositionLeft") as Label ?? new Label();
                positionTop = Window.FindName("LblPositionTop") as Label ?? new Label();
                positionRight = Window.FindName("LblPositionRight") as Label ?? new Label();
                positionBottom = Window.FindName("LblPositionBottom") as Label ?? new Label();

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

        private void LblTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            //Window.OnMouseLeftButtonDown(e);
            Window.DragMove();
        }

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

        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            service.TryUnstickWithOtherWindows(this);
            service.RemoveWindow(this);
        }

        #endregion

        #region Window Position

        public double Left => Window.Left;
        public double Right => Window.Left + Window.Width;
        public double Top => Window.Top;
        public double Bottom => Window.Top + Window.Height;

        #endregion

        #region Sticked Windows Management

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
        internal async void StickWindow(StickyWindow targetWindow, StickPosition position, bool arrange = false)
        {
            if (position == StickPosition.Top)
            {
                StickTop = targetWindow;

                if (arrange)
                {
                    WinAPI.MouseLeftUp(Window);
                    await Task.Delay(100);

                    Window.Left = targetWindow.Left;
                    Window.Top = targetWindow.Bottom;
                    Window.Width = targetWindow.Window.Width;
                }
            }

            else if (position == StickPosition.Right)
            {
                StickRight = targetWindow;

                if (arrange)
                {
                    WinAPI.MouseLeftUp(Window);
                    await Task.Delay(100);

                    Window.Left = targetWindow.Left - Window.Width;
                    Window.Top = targetWindow.Top;
                    Window.Height = targetWindow.Window.Height;
                }
            }

            else if (position == StickPosition.Bottom)
            {
                StickBottom = targetWindow;

                if (arrange)
                {
                    WinAPI.MouseLeftUp(Window);
                    await Task.Delay(100);

                    Window.Left = targetWindow.Left;
                    Window.Top = targetWindow.Top - Window.Height;
                    Window.Width = targetWindow.Window.Width;
                }
            }

            else if (position == StickPosition.Left)
            {
                StickLeft = targetWindow;

                if (arrange)
                {
                    WinAPI.MouseLeftUp(Window);
                    await Task.Delay(100);

                    Window.Left = targetWindow.Left + targetWindow.Window.Width;
                    Window.Top = targetWindow.Top;
                    Window.Height = targetWindow.Window.Height;
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
