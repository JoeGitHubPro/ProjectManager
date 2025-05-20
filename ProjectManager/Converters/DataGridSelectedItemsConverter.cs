using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace ProjectManager.Converters
{
    public static class DataGridSelectedItemsConverter
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached("SelectedItems",
                typeof(IList),
                typeof(DataGridSelectedItemsConverter),
                new PropertyMetadata(null, OnSelectedItemsChanged));

        public static void SetSelectedItems(DependencyObject element, IList value) =>
            element.SetValue(SelectedItemsProperty, value);

        public static IList GetSelectedItems(DependencyObject element) =>
            (IList)element.GetValue(SelectedItemsProperty);

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.SelectionChanged -= DataGrid_SelectionChanged;
                dataGrid.SelectionChanged += DataGrid_SelectionChanged;
            }
        }

        private static void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                IList selectedItems = GetSelectedItems(dataGrid);
                selectedItems?.Clear();
                if (selectedItems != null)
                {
                    foreach (var item in dataGrid.SelectedItems)
                    {
                        selectedItems.Add(item);
                    }
                }
            }
        }
    }
}
