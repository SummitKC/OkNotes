using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Handle exceptions thrown on the UI thread
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;

            // Handle exceptions thrown on non-UI threads
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the exception or show a message to the user
            MessageBox.Show("An unexpected error occurred: " + e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Optionally, set e.Handled to true to prevent the application from crashing
            e.Handled = true;
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            // Log the exception or show a message to the user
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show("An unexpected error occurred: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("An unmanaged error occurred.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            // Note: Cannot set e.Handled here
        }
    }
}
  