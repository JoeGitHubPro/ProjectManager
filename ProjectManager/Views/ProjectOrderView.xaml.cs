using System.Windows.Controls;

namespace ProjectManager.Views
{
    /// <summary>
    /// Interaction logic for ProjectOrderView.xaml
    /// </summary>
    public partial class ProjectOrderView : UserControl
    {
        public ProjectOrderView()
        {
            InitializeComponent();
            SearchTextBox.Focus();
        }
    }
}
