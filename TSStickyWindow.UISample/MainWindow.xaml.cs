using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using TSStickyWindow.Layout;
using TSStickyWindow.Messages;

namespace TSStickyWindow.UISample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Todo: initialize the Sticky Window Service with options (optional)
            StickyWindowService.Instance = new StickyWindowService(new StickyWindowOptions(10, 50, 50) { MainWindowType = typeof(MainWindow) })
            {
                WindowSticked = WindowSticked,
                WindowUnsticked = WindowUnsticked,
            };

            LoadSavedLayout();
        }

        private void WindowSticked(WindowStickedMessage message)
        {
            // Todo: Handle the event
        }
        private void WindowUnsticked(WindowUnstickedMessage message)
        {
            // Todo: Handle the event
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


        #region Layout Management

        private string savedLayout;

        private void LoadSavedLayout()
        {
            try
            {
                savedLayout = File.ReadAllText("tsstickywindowlayouts.txt");
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            if (string.IsNullOrWhiteSpace(savedLayout))
                BtnLoadLayout.Visibility = Visibility.Collapsed;
            else
                BtnLoadLayout.Visibility = Visibility.Visible;
        }

        private void BtnLoadLayout_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var layout = JsonSerializer.Deserialize<StickyLayout>(savedLayout);
                foreach (var stickyLayoutWindow in layout!.Windows)
                {
                    stickyLayoutWindow.Window = GetWindowByName(stickyLayoutWindow.WindowTypeName);
                }

                StickyWindowService.Instance.LoadLayout(layout);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void BtnSaveLayout_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var json = StickyWindowService.Instance.GetLayout();
                File.WriteAllText("tsstickywindowlayouts.txt", json);

                BtnLoadLayout.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        #endregion

        #region Window Type Service

        private Window GetWindowByName(string type)
        {
            if (type == nameof(MainWindow))
                return this;
            if (type == nameof(SubWindow))
                return new SubWindow();

            throw new Exception("Window is not registered");
        }

        #endregion

    }
}
