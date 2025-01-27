using System.Configuration;
using System.Data;
using System.Windows;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private PenViewModel _penViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _penViewModel = new PenViewModel();
            // Other startup logic
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _penViewModel.SavePenSettings();
            base.OnExit(e);
        }

    }

}
