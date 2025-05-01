using System.Windows.Controls;

namespace ProjectManager.Views
{
    /// <summary>
    /// Interaction logic for ProjectView.xaml
    /// </summary>
    public partial class ProjectView : UserControl
    {
        public ProjectView()
        {
            InitializeComponent();
            SearchTextBox.Focus();
        }
    }
}
