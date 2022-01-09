using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TSStickyWindow
{
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

        private bool isSticking;
        private bool isDraggedWindow;

        private void LblTitleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
                return;

            SetLastWindowPosition();

            isDraggedWindow = true;
            //Window.OnMouseLeftButtonDown(e);
            Window.DragMove();

            isDraggedWindow = false;

            isSticking = true;
            service.TryStickWithOtherWindows(this);
            isSticking = false;
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

            if (!isSticking && isDraggedWindow)
            {

                var deltaX = Window.Left - lastLeft;
                var deltaY = Window.Top - lastTop;

                //Debug.WriteLine("Window: " + Id + "  - dX: " + deltaX + " | dY: " + deltaY);

                service.RepositionStickedWindows(this, deltaX, deltaY);
                SetLastWindowPosition();
            }
        }

        private void WindowOnClosing(object sender, CancelEventArgs e)
        {
            service.TryUnstickWithOtherWindows(this);
            service.RemoveWindow(this);
        }

        #endregion

        #region Window Position

        private double lastLeft;
        private double lastTop;

        private void SetLastWindowPosition()
        {
            lastLeft = Window.Left;
            lastTop = Window.Top;
        }

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
        internal void StickWindow(StickyWindow targetWindow, StickPosition position, bool arrange = false)
        {
            if (position == StickPosition.Top)
            {
                StickTop = targetWindow;

                if (arrange)
                {
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
}