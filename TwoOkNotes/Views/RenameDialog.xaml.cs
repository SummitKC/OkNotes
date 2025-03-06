using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace TwoOkNotes.Views
{
    public partial class RenameDialog : Window
    {
        public string NewName { get; private set; }
        private readonly string _originalName;

        public RenameDialog(string currentName)
        {
            InitializeComponent();
            _originalName = currentName;
            
            // Set the current name as the default text
            NewNameTextBox.Text = currentName;
            
            // Focus and select all text for quick editing
            NewNameTextBox.Loaded += (s, e) => 
            {
                NewNameTextBox.Focus();
                NewNameTextBox.SelectAll();
            };
            
            // Handle key presses
            this.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    TryAcceptAndClose();
                    e.Handled = true;
                }
                else if (e.Key == System.Windows.Input.Key.Escape)
                {
                    DialogResult = false;
                    Close();
                    e.Handled = true;
                }
            };
        }

        // Title bar drag functionality
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            TryAcceptAndClose();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void TryAcceptAndClose()
        {
            string newText = NewNameTextBox.Text?.Trim();
            
            // Basic validation
            if (string.IsNullOrWhiteSpace(newText))
            {
                MessageBox.Show("Name cannot be empty.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Check for invalid characters (works for both sections and pages)
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (newText.IndexOfAny(invalidChars) >= 0)
            {
                MessageBox.Show($"Name contains invalid characters. The following characters are not allowed: {string.Join(" ", invalidChars)}",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Different from original?
            if (newText.Equals(_originalName, StringComparison.OrdinalIgnoreCase))
            {
                DialogResult = false;
                Close();
                return;
            }
            
            NewName = newText;
            DialogResult = true;
            Close();
        }
    }
}