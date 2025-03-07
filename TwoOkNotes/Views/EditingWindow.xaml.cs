using System.Windows.Input;
using System.Windows;
using TwoOkNotes.ViewModels;
using TwoOkNotes.Model;
using System.Windows.Ink;
using System.Collections.Generic;
namespace TwoOkNotes.Views
{
    public partial class EditingWindow : Window
    {
        public EditingWindow()
        {
            InitializeComponent();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (DataContext is EditingWIndowViewModel viewModel)
            {
                viewModel.OnKeyDown(e);
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (DataContext is EditingWIndowViewModel viewModel)
            {
                viewModel.OnKeyUp(e);
            }
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (DataContext is EditingWIndowViewModel viewModel)
            {
                viewModel.OnMouseWheel(e);
            }
        }

        // Add this method to handle the ScrollViewer's PreviewMouseWheel event
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Check if Ctrl key is pressed
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Mark the event as handled to prevent the ScrollViewer from scrolling
                e.Handled = true;
                
                // Forward the event to the window for zooming
                Window_MouseWheel(sender, e);
            }
        }

        // Title bar event handlers
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}