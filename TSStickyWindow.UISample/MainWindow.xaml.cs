﻿using System.Windows;
using System.Windows.Input;
using TSStickyWindow.Messages;

namespace TSStickyWindow.UISample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Todo: initialize the Sticky Window Service with options (optional)
            StickyWindowService.Instance = new StickyWindowService(new StickyWindowOptions(10, 50, 50))
            {
                WindowSticked = WindowSticked,
                WindowUnsticked = WindowUnsticked
            };


            StickyWindowService.Instance.AddNewWindow(this);
        }

        private void WindowSticked(WindowStickedMessage message)
        {

        }
        private void WindowUnsticked(WindowUnstickedMessage message)
        {

        }

        private void Title_lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Btn_NewWindow_Click(object sender, RoutedEventArgs e)
        {
            StickyWindowService.Instance.AddNewWindow(new SubWindow());
        }
    }
}
