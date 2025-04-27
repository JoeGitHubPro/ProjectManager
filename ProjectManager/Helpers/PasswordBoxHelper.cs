using System.Windows;
using System.Windows.Controls;

namespace ProjectManager.Helpers
{
    public static class PasswordBoxHelper
    {
        // DependencyProperty for binding the password
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

        // DependencyProperty to enable/disable password binding
        public static readonly DependencyProperty BindPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BindPassword",
                typeof(bool),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(false, OnBindPasswordChanged));

        // Attached property getter for BoundPassword
        public static string GetBoundPassword(DependencyObject obj)
        {
            return (string)obj.GetValue(BoundPasswordProperty);
        }

        // Attached property setter for BoundPassword
        public static void SetBoundPassword(DependencyObject obj, string value)
        {
            obj.SetValue(BoundPasswordProperty, value);
        }

        // Attached property getter for BindPassword
        public static bool GetBindPassword(DependencyObject obj)
        {
            return (bool)obj.GetValue(BindPasswordProperty);
        }

        // Attached property setter for BindPassword
        public static void SetBindPassword(DependencyObject obj, bool value)
        {
            obj.SetValue(BindPasswordProperty, value);
        }

        // Handler for when the BoundPassword property changes

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                // Unsubscribe to prevent infinite loop
                passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;

                // Only update if necessary
                if (passwordBox.Password != e.NewValue?.ToString())
                {
                    passwordBox.Password = e.NewValue?.ToString() ?? string.Empty;
                }

                // Re-subscribe to event
                passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }
        // Handler for when the BindPassword property changes
        private static void OnBindPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox)
            {
                if ((bool)e.OldValue)
                {
                    // Unsubscribe from the PasswordChanged event if BindPassword is being disabled
                    passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
                }

                if ((bool)e.NewValue)
                {
                    // Subscribe to the PasswordChanged event if BindPassword is being enabled
                    passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
                }
            }
        }

        // Handler for the PasswordChanged event
        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                // Update the BoundPassword property when the PasswordBox's password changes
                SetBoundPassword(passwordBox, passwordBox.Password);
            }
        }
    }
}