using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;

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
            biggestGivenId++;
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

        internal void RepositionStickedWindows(StickyWindow source, double deltaX, double deltaY)
        {
            var hasNewWindow = true;
            var allWindows = new List<StickyWindow> { source };
            var newWindows = new List<StickyWindow>();

            while (hasNewWindow)
            {
                foreach (var stickyWindow in allWindows)
                {
                    var stickedWindows = stickyWindow.GetStickedWindows(allWindows, newWindows);
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
                stickyWindow.Window.Left += deltaX;
                stickyWindow.Window.Top += deltaY;
            }
        }
    }
}
