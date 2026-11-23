using System.Windows.Controls;

namespace ProjectManager.Views
{
    public partial class ConsumerView : UserControl
    {
        public ConsumerView()
        {
            InitializeComponent();
            //  DataContext = App.Current.Services.GetRequiredService<ConsumerViewModel>();
            SearchTextBox.Focus();
        }
    }
}
