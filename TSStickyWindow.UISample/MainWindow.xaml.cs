using System;
using System.Runtime.CompilerServices;
using System.Windows;
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

            LocationChanged += OnLocationChanged;
        }

        private void OnLocationChanged(object? sender, EventArgs e)
        {
            PositionX.Content = Left.ToString("0.#");
            PositionY.Content = Top.ToString("0.#");
        }

        private void Title_lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);

            var pos = e.GetPosition(this);
            //PositionX.Content = Left.ToString("0.#");
            //PositionY.Content = Top.ToString("0.#");

            DragMove();
        }
        


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
