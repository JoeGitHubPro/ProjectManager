using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProjectManager.Views
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Find LogoAnimation storyboard
            Storyboard logoAnimation = (Storyboard)FindResource("LogoAnimation");

            // When LogoAnimation completes, start LoadingAnimation
            logoAnimation.Completed += (s, e) =>
            {
                Storyboard loadingAnimation = (Storyboard)FindResource("LoadingAnimation");
                loadingAnimation.Begin();
            };

            // Start the first animation (Logo)
            logoAnimation.Begin();
        }
    }
}
