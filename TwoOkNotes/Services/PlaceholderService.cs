using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TwoOkNotes.Services
{
    public static class PlaceholderService
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.RegisterAttached(
                "Placeholder",
                typeof(string),
                typeof(PlaceholderService),
                new UIPropertyMetadata(string.Empty, OnPlaceholderChanged));

        public static string GetPlaceholder(UIElement element)
        {
            return (string)element.GetValue(PlaceholderProperty);
        }

        public static void SetPlaceholder(UIElement element, string value)
        {
            element.SetValue(PlaceholderProperty, value);
        }

        private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                if (e.NewValue is string placeholderText)
                {
                    textBox.GotFocus += (sender, args) =>
                    {
                        if (textBox.Text == placeholderText)
                        {
                            textBox.Text = string.Empty;
                            textBox.Foreground = Brushes.Black;
                        }
                    };

                    textBox.LostFocus += (sender, args) =>
                    {
                        if (string.IsNullOrWhiteSpace(textBox.Text))
                        {
                            textBox.Text = placeholderText;
                            textBox.Foreground = Brushes.Gray;
                        }
                    };

                    // Initialize placeholder
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        textBox.Text = placeholderText;
                        textBox.Foreground = Brushes.Gray;
                    }
                }
            }
        }
    }
}