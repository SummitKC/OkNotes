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
    }
}