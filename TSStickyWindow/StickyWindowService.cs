using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using TSStickyWindow.Definitions;
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
        private readonly bool testMode;
        private readonly StickyWindowOptions options;
        private readonly List<StickyWindow> windows;

        #region Constructor & Initialization

        public StickyWindowService(StickyWindowOptions options = null, bool? testmode = null)
        {
            if (testmode == true)
                testMode = true;

            this.options = options ?? new StickyWindowOptions();

            windows = new List<StickyWindow>();
        }

        #endregion

        /// <summary>
        /// Lazy solution, better to use dependency injection in production
        /// </summary>
        public static StickyWindowService Instance { get; set; }

        #region Public Functions

        public void AddNewWindow(Window window, string id)
        {
            window.Width = options.WindowInitialWidth;
            window.Height = options.WindowInitialHeight;

            windows.Add(new StickyWindow(this, options, window, id, testMode));
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

                if (window.Stick[StickPosition.Top] is not null)
                    layoutWindow.ConnectionTopId = window.Stick[StickPosition.Top]!.Id;
                if (window.Stick[StickPosition.Right] is not null)
                    layoutWindow.ConnectionRightId = window.Stick[StickPosition.Right]!.Id;
                if (window.Stick[StickPosition.Bottom] is not null)
                    layoutWindow.ConnectionBottomId = window.Stick[StickPosition.Bottom]!.Id;
                if (window.Stick[StickPosition.Left] is not null)
                    layoutWindow.ConnectionLeftId = window.Stick[StickPosition.Left]!.Id;

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

                var stickyWindow = new StickyWindow(this, options, layoutWindow.Window, layoutWindow.Id, testMode);

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

        #endregion

        #region Public Events

        public Action<WindowStickedMessage> WindowSticked { get; set; }
        public Action<WindowUnstickedMessage> WindowUnsticked { get; set; }

        #endregion

        #region Internal Functions

        internal (MagnetOrientiation orientiation, string targetId) TryMagnetWithUnstickedWindows(StickyWindow source)
        {
            var (targetPosition, target) = GetTargetPosition(source);

            switch (targetPosition)
            {
                case StickPosition.Top:
                    source.SetMagnetPosition(target.Bottom, null, null, null);
                    return (MagnetOrientiation.Vertical, target.Id);
                case StickPosition.Right:
                    source.SetMagnetPosition(null, target.Left, null, null);
                    return (MagnetOrientiation.Horizontal, target.Id);
                case StickPosition.Bottom:
                    source.SetMagnetPosition(null, null, target.Top, null);
                    return (MagnetOrientiation.Vertical, target.Id);
                case StickPosition.Left:
                    source.SetMagnetPosition(null, null, null, target.Right);
                    return (MagnetOrientiation.Horizontal, target.Id);
                default:
                    return (MagnetOrientiation.None, null);
            }
        }

        internal void TryStickWithOtherWindows(StickyWindow source)
        {
            var (targetPosition, target) = GetTargetPosition(source);

            if (targetPosition == StickPosition.Top)
            {
                if (!source.CanStickWindow(target, StickPosition.Top) ||
                    !target.CanStickWindow(source, StickPosition.Bottom)) 
                    return;

                source.StickWindow(target, StickPosition.Top, true);
                target.StickWindow(source, StickPosition.Bottom);
                WindowSticked?.Invoke(new WindowStickedMessage());
            }

            else if (targetPosition == StickPosition.Right)
            {
                if (!source.CanStickWindow(target, StickPosition.Right) ||
                    !target.CanStickWindow(source, StickPosition.Left)) 
                    return;

                source.StickWindow(target, StickPosition.Right, true);
                target.StickWindow(source, StickPosition.Left);
                WindowSticked?.Invoke(new WindowStickedMessage());
            }

            else if (targetPosition == StickPosition.Bottom)
            {
                if (!source.CanStickWindow(target, StickPosition.Bottom) ||
                    !target.CanStickWindow(source, StickPosition.Top)) 
                    return;

                source.StickWindow(target, StickPosition.Bottom, true);
                target.StickWindow(source, StickPosition.Top);
                WindowSticked?.Invoke(new WindowStickedMessage());
            }

            else if (targetPosition == StickPosition.Left)
            {
                if (!source.CanStickWindow(target, StickPosition.Left) ||
                    !target.CanStickWindow(source, StickPosition.Right)) 
                    return;

                source.StickWindow(target, StickPosition.Left, true);
                target.StickWindow(source, StickPosition.Right);
                WindowSticked?.Invoke(new WindowStickedMessage());
            }
        }

        internal void InvokeWindowsUnsticked(string sourceId,
            string targetTopId = "", string targetRightId = "",
            string targetBottomId = "", string targetLeftId = "")
        {
            WindowUnsticked?.Invoke(new WindowUnstickedMessage(sourceId, targetTopId, targetRightId, targetBottomId, targetLeftId));
        }

        internal void RemoveWindow(StickyWindow window)
        {
            windows.Remove(window);
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

        #region Helper functions

        private (StickPosition? targetPosition, StickyWindow target) GetTargetPosition(StickyWindow source)
        {
            foreach (var target in windows)
            {
                if (target.Id == source.Id ||
                    target.Id == source.Stick[StickPosition.Top]?.Id ||
                    target.Id == source.Stick[StickPosition.Right]?.Id ||
                    target.Id == source.Stick[StickPosition.Bottom]?.Id ||
                    target.Id == source.Stick[StickPosition.Left]?.Id)
                    continue;

                // Source Top Edge
                if (Math.Abs(target.Bottom - source.Top) < options.SnapOffset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    return (StickPosition.Top, target);
                }

                // Source Right Edge
                if (Math.Abs(target.Left - source.Right) < options.SnapOffset &&
                    target.Top <= source.Bottom &&
                    target.Bottom >= source.Top)
                {
                    return (StickPosition.Right, target);
                }

                // Source Bottom Edge
                if (Math.Abs(target.Top - source.Bottom) < options.SnapOffset &&
                    target.Left <= source.Right &&
                    target.Right >= source.Left)
                {
                    return (StickPosition.Bottom, target);
                }

                // Source Left Edge
                if (Math.Abs(target.Right - source.Left) < options.SnapOffset &&
                         target.Top <= source.Bottom &&
                         target.Bottom >= source.Top)
                {
                    return (StickPosition.Left, target);
                }
            }

            return (null, null);
        }

        #endregion
    }
}
