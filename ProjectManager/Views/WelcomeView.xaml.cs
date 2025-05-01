using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace ProjectManager.Views
{
    /// <summary>
    /// Interaction logic for WelcomeView.xaml
    /// </summary>
    public partial class WelcomeView : UserControl
    {
        public WelcomeView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EXPLORECONSUMERSButton_Click(object sender, RoutedEventArgs e)
        {
            var currentWindow = Application.Current.Windows
                .OfType<MainWindow>()
                .SingleOrDefault(w => w.IsActive);

            if (currentWindow != null)
            {
                currentWindow.MainTabControl.SelectedIndex = 1; // or set by name
            }
        }


        private void OPENSITEButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://icecode.site/",
                UseShellExecute = true
            });
        }
    }
}
