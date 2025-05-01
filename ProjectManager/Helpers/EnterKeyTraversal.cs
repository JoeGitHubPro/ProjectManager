using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ProjectManager.Helpers
{
    public static class EnterKeyTraversal
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),
            typeof(EnterKeyTraversal), new UIPropertyMetadata(false, OnIsEnabledChanged));

        private static void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement element)
            {
                if ((bool)e.NewValue)
                {
                    element.PreviewKeyDown += Element_PreviewKeyDown;
                }
                else
                {
                    element.PreviewKeyDown -= Element_PreviewKeyDown;
                }
            }
        }

        private static void Element_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var element = sender as UIElement;
                var container = FindVisualParent<Panel>(element);

                // Check if this is the last focusable element in the container
                if (container != null && IsLastFocusableElement(container, element))
                {
                    // Try to find and execute the default button (Save)
                    var saveButton = FindVisualChild<Button>(container, btn =>
                        btn.Command != null && btn.Name == "SaveButton");

                    if (saveButton?.Command?.CanExecute(saveButton.CommandParameter) == true)
                    {
                        saveButton.Command.Execute(saveButton.CommandParameter);
                        e.Handled = true;
                        return;
                    }
                }

                // For all other cases, move to next field
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                e.Handled = true;
            }
        }

        private static bool IsLastFocusableElement(Panel container, IInputElement currentElement)
        {
            var focusableElements = container.Children.OfType<UIElement>()
                .Where(el => el.Focusable && el.IsEnabled && el.IsVisible)
                .OrderBy(el => KeyboardNavigation.GetTabIndex(el))
                .ToList();

            if (focusableElements.Count == 0) return false;

            var lastElement = focusableElements.Last();
            return currentElement == lastElement;
        }

        private static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindVisualParent<T>(parentObject);
        }

        private static T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate = null)
            where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result && (predicate == null || predicate(result)))
                {
                    return result;
                }
                var childResult = FindVisualChild(child, predicate);
                if (childResult != null) return childResult;
            }
            return null;
        }
    }
}