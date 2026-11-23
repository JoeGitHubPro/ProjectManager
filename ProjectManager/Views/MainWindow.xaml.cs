using ProjectManager.ViewModels;
using System.Windows;

namespace ProjectManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            DataContext = new MainViewModel(serviceProvider);
        }


    }
}