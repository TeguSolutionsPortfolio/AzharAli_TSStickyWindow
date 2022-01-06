using System.Collections.Generic;
using System.Windows;

namespace TSStickyWindow
{
    /// <summary>
    /// Plan: make it as a singleton service and pass the windows here
    /// The inner logic will manage the window positions and others
    /// </summary>
    public class StickyWindowService
    {
        /// <summary>
        /// Lazy solution, better to use dependency injection in production
        /// </summary>
        public static StickyWindowService Instance { get; } = new();

        private List<StickyWindow> windows;

        #region Init

        public StickyWindowService()
        {

        }

        #endregion

        public void AddNewWindow(Window window)
        {
            windows.Add(new StickyWindow(this, window));
        }
    }

    public class StickyWindow
    {
        private StickyWindowService service;

        public StickyWindow(StickyWindowService windowService, Window window)
        {
            service = windowService;
            Window = window;
        }

        public Window Window { get; }
    }
}
