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