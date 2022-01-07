using System.Windows;
using System.Windows.Input;

namespace TSStickyWindow.UISample
{
    /// <summary>
    /// Interaction logic for SubWindow.xaml
    /// </summary>
    public partial class SubWindow : Window
    {
        public SubWindow()
        {
            InitializeComponent();
        }

        private void Title_lbl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //OnMouseLeftButtonDown(e);
            //DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
