﻿using System.Windows;
using System.Windows.Input;

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

            StickyWindowService.Instance.AddNewWindow(this);
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
