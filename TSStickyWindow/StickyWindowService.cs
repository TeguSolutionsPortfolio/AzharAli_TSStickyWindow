using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using TSStickyWindow.Layout;
using TSStickyWindow.Messages;

namespace TSStickyWindow
{
    /// <summary>
    /// Plan: make it as a singleton service and pass the windows here
    /// The inner logic will manage the window positions and others
    /// </summary>
    public class StickyWindowService
    {
        private readonly StickyWindowOptions options;
        private readonly List<StickyWindow> windows;

        #region Init

        public StickyWindowService(StickyWindowOptions? options = null)
        {
            this.options = options ?? new StickyWindowOptions();

            windows = new List<StickyWindow>();
        }

        #endregion

        /// <summary>
        /// Lazy solution, better to use dependency injection in production
        /// </summary>
        public static StickyWindowService Instance { get; set; }

        #region Id Service

        private int biggestGivenId;

        internal string GetNextId()
        {
            biggestGivenId++;
            return biggestGivenId.ToString();
        }

        #endregion

        #region Public Functions

        public void AddNewWindow(Window window)
        {
            window.Width = options.WindowInitialWidth;
            window.Height = options.WindowInitialHeight;

            windows.Add(new StickyWindow(this, options, window));
        }

        public string GetLayout()
        {
            var layout = new StickyLayout();

            foreach (var window in windows)
            {
                var layoutWindow = new StickyLayoutWindow(window.Id)
                {
                    WindowTypeName = window.GetWindowType(),
                    PositionLeft = window.Left,
                    PositionTop = window.Top,
                    Width = window.Width,
                    Height = window.Height
                };

                if (window.StickTop is not null)
                    layoutWindow.ConnectionTopId = window.StickTop.Id;
                if (window.StickRight is not null)
                    layoutWindow.ConnectionRightId = window.StickRight.Id;
                if (window.StickBottom is not null)
                    layoutWindow.ConnectionBottomId = window.StickBottom.Id;
                if (window.StickLeft is not null)
                    layoutWindow.ConnectionLeftId = window.StickLeft.Id;

                layout.Windows.Add(layoutWindow);
            }

            var json = JsonSerializer.Serialize(layout);
            return json;
        }

        public void LoadLayout(StickyLayout layout)
        {
            CloseAllWindows();

            // Step 01 - Initialize and open the windows
            foreach (var layoutWindow in layout.Windows)
            {
                layoutWindow.Window.Left = layoutWindow.PositionLeft;
                layoutWindow.Window.Top = layoutWindow.PositionTop;
                layoutWindow.Window.Width = layoutWindow.Width;
                layoutWindow.Window.Height = layoutWindow.Height;
                
                var stickyWindow = new StickyWindow(this, options, layoutWindow.Window)
                {
                    Id = layoutWindow.Id
                };

                windows.Add(stickyWindow);
                stickyWindow.ShowWindow();
            }

            // Step 02 - Set the relations
            foreach (var stickyWindow in windows)
            {
                var layoutWindow = layout.Windows.First(lw => lw.Id == stickyWindow.Id);

                if (!string.IsNullOrWhiteSpace(layoutWindow.ConnectionTopId))
                    stickyWindow.StickWindow(windows.First(w => w.Id == layoutWindow.ConnectionTopId), StickPosition.Top);

                if (!string.IsNullOrWhiteSpace(layoutWindow.ConnectionRightId))
                    stickyWindow.StickWindow(windows.First(w => w.Id == layoutWindow.ConnectionRightId), StickPosition.Right);

                if (!string.IsNullOrWhiteSpace(layoutWindow.ConnectionBottomId))
                    stickyWindow.StickWindow(windows.First(w => w.Id == layoutWindow.ConnectionBottomId), StickPosition.Bottom);

                if (!string.IsNullOrWhiteSpace(layoutWindow.ConnectionLeftId))
                    stickyWindow.StickWindow(windows.First(w => w.Id == layoutWindow.ConnectionLeftId), StickPosition.Left);
            }
        }

        private void CloseAllWindows()
        {
            // Note: create a separate list, because the window closing event triggers the RemoveWindow
            // which modifies the collection and leads to errors..
            var closingWindows = windows.ToList();
            windows.Clear();

            foreach (var stickyWindow in closingWindows)
            {
                stickyWindow.CloseWindow();
            }
        }

        #endregion

        #region Public Events

        public Action<WindowStickedMessage>? WindowSticked { get; set; }
        public Action<WindowUnstickedMessage>? WindowUnsticked { get; set; }

        #endregion

        #region Internal Functions

        internal void TryStickWithOtherWindows(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target == source)
                    continue;

                // Source Top Edge
                if (Math.Abs(target.Bottom - source.Top) < options.SnapOffset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Top) && target.CanStickWindow(source, StickPosition.Bottom))
                    {
                        source.StickWindow(target, StickPosition.Top, true);
                        target.StickWindow(source, StickPosition.Bottom);

                        WindowSticked?.Invoke(new WindowStickedMessage());
                    }
                }

                // Source Right Edge
                else if (Math.Abs(target.Left - source.Right) < options.SnapOffset &&
                         target.Top <= source.Bottom &&
                         target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Right) && target.CanStickWindow(source, StickPosition.Left))
                    {
                        source.StickWindow(target, StickPosition.Right, true);
                        target.StickWindow(source, StickPosition.Left);

                        WindowSticked?.Invoke(new WindowStickedMessage());
                    }
                }

                // Source Bottom Edge
                if (Math.Abs(target.Top - source.Bottom) < options.SnapOffset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    if (source.CanStickWindow(target, StickPosition.Bottom) && target.CanStickWindow(source, StickPosition.Top))
                    {
                        source.StickWindow(target, StickPosition.Bottom, true);
                        target.StickWindow(source, StickPosition.Top);

                        WindowSticked?.Invoke(new WindowStickedMessage());
                    }
                }

                // Source Left Edge
                else if (Math.Abs(target.Right - source.Left) < options.SnapOffset &&
                         target.Top <= source.Bottom &&
                         target.Bottom >= source.Top)
                {
                    if (source.CanStickWindow(target, StickPosition.Left) && target.CanStickWindow(source, StickPosition.Right))
                    {
                        source.StickWindow(target, StickPosition.Left, true);
                        target.StickWindow(source, StickPosition.Right);

                        WindowSticked?.Invoke(new WindowStickedMessage());
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

            WindowUnsticked?.Invoke(new WindowUnstickedMessage());
        }

        internal void RemoveWindow(StickyWindow window)
        {
            windows.Remove(window);
        }

        internal void RepositionStickedWindows(StickyWindow source, double deltaX, double deltaY)
        {
            var hasNewWindow = true;
            var allWindows = new List<StickyWindow> { source };
            var newWindows = new List<StickyWindow>();

            while (hasNewWindow)
            {
                foreach (var stickyWindow in allWindows)
                {
                    var stickedWindows = stickyWindow.GetAllStickedWindows(allWindows/*, newWindows*/);
                    foreach (var stickedWindow in stickedWindows)
                    {
                        if (!allWindows.Contains(stickedWindow))
                            newWindows.Add(stickedWindow);
                    }
                }

                // Assuming there are only new windows available here at this point
                allWindows.AddRange(newWindows);

                if (newWindows.Count == 0)
                    hasNewWindow = false;

                newWindows.Clear();
            }

            // Remove the source (already moved) window
            allWindows.Remove(source);

            // And reposition the rest
            foreach (var stickyWindow in allWindows)
            {
                stickyWindow.SetWindowPositionDiff(deltaX, deltaY);
            }
        }

        internal void ResizeStickedWindowsWidth(StickyWindow source, double deltaWidth)
        {
            var hasNewWindow = true;
            var allWindows = new List<StickyWindow> { source };
            var newWindows = new List<StickyWindow>();

            while (hasNewWindow)
            {
                foreach (var stickyWindow in allWindows)
                {
                    var stickedWindows = stickyWindow.GetVerticalStickedWindows(allWindows/*, newWindows*/);
                    foreach (var stickedWindow in stickedWindows)
                    {
                        if (!allWindows.Contains(stickedWindow))
                            newWindows.Add(stickedWindow);
                    }
                }

                // Assuming there are only new windows available here at this point
                allWindows.AddRange(newWindows);

                if (newWindows.Count == 0)
                    hasNewWindow = false;

                newWindows.Clear();
            }

            // Remove the source (already moved) window
            allWindows.Remove(source);

            // And reposition the rest
            foreach (var stickyWindow in allWindows)
            {
                stickyWindow.SetWindowWidthDiff(deltaWidth);
            }
        }

        internal void ResizeStickedWindowsHeight(StickyWindow source, double deltaHeight)
        {
            var hasNewWindow = true;
            var allWindows = new List<StickyWindow> { source };
            var newWindows = new List<StickyWindow>();

            while (hasNewWindow)
            {
                foreach (var stickyWindow in allWindows)
                {
                    var stickedWindows = stickyWindow.GetHorizontalStickedWindows(allWindows/*, newWindows*/);
                    foreach (var stickedWindow in stickedWindows)
                    {
                        if (!allWindows.Contains(stickedWindow))
                            newWindows.Add(stickedWindow);
                    }
                }

                // Assuming there are only new windows available here at this point
                allWindows.AddRange(newWindows);

                if (newWindows.Count == 0)
                    hasNewWindow = false;

                newWindows.Clear();
            }

            // Remove the source (already moved) window
            allWindows.Remove(source);

            // And reposition the rest
            foreach (var stickyWindow in allWindows)
            {
                stickyWindow.SetWindowHeightDiff(deltaHeight);
            }
        }

        #endregion
    }
}
