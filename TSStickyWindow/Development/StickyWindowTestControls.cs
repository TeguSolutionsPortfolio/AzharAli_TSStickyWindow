using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TSStickyWindow.Development
{
    internal class StickyWindowTestControls
    {
        private readonly StickyWindow stickyWindow;

        private readonly Border brdTop;
        private readonly Border brdRight;
        private readonly Border brdBottom;
        private readonly Border brdLeft;

        private readonly Label lblConnectionTopId;
        private readonly Label lblConnectionRightId;
        private readonly Label lblConnectionBottomId;
        private readonly Label lblConnectionLeftId;

        private readonly Label positionLeft;
        private readonly Label positionTop;
        private readonly Label positionRight;
        private readonly Label positionBottom;

        internal StickyWindowTestControls(StickyWindow stickyWindow, Window window)
        {
            this.stickyWindow = stickyWindow;

            // Test Controls (removable in production)
            brdTop = window.FindName("BrdTop") as Border ?? new Border();
            brdRight = window.FindName("BrdRight") as Border ?? new Border();
            brdBottom = window.FindName("BrdBottom") as Border ?? new Border();
            brdLeft = window.FindName("BrdLeft") as Border ?? new Border();

            lblConnectionTopId = window.FindName("LblConnectionTopId") as Label ?? new Label();
            lblConnectionRightId = window.FindName("LblConnectionRightId") as Label ?? new Label();
            lblConnectionBottomId = window.FindName("LblConnectionBottomId") as Label ?? new Label();
            lblConnectionLeftId = window.FindName("LblConnectionLeftId") as Label ?? new Label();

            positionLeft = window.FindName("LblPositionLeft") as Label ?? new Label();
            positionTop = window.FindName("LblPositionTop") as Label ?? new Label();
            positionRight = window.FindName("LblPositionRight") as Label ?? new Label();
            positionBottom = window.FindName("LblPositionBottom") as Label ?? new Label();
        }

        public void UpdatePositionLabels()
        {
            try
            {
                positionLeft.Content = stickyWindow.Left.ToString("0");
                positionTop.Content = stickyWindow.Top.ToString("0");
                positionRight.Content = stickyWindow.Right.ToString("0");
                positionBottom.Content = stickyWindow.Bottom.ToString("0");
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
            }
        }

        public void HighlightStickState()
        {
            if (stickyWindow.Stick[StickPosition.Top] is null)
            {
                brdTop.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionTopId.Content = "";
            }
            else
            {
                brdTop.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionTopId.Content = stickyWindow.Stick[StickPosition.Top].Id;
            }

            if (stickyWindow.Stick[StickPosition.Right] is null)
            {
                brdRight.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionRightId.Content = "";
            }
            else
            {
                brdRight.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionRightId.Content = stickyWindow.Stick[StickPosition.Right].Id;
            }

            if (stickyWindow.Stick[StickPosition.Bottom] is null)
            {
                brdBottom.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionBottomId.Content = "";
            }
            else
            {
                brdBottom.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionBottomId.Content = stickyWindow.Stick[StickPosition.Bottom].Id;
            }

            if (stickyWindow.Stick[StickPosition.Left] is null)
            {
                brdLeft.BorderBrush = new SolidColorBrush(Colors.DarkGray);
                lblConnectionLeftId.Content = "";
            }
            else
            {
                brdLeft.BorderBrush = new SolidColorBrush(Colors.Lime);
                lblConnectionLeftId.Content = stickyWindow.Stick[StickPosition.Left].Id;
            }
        }
    }
}