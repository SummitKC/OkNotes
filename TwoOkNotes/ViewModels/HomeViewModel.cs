using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoOkNotes.Util;
using TwoOkNotes.Views;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        public ICommand OpenWindow { get; }

        public HomeViewModel()
        {
            OpenWindow = new RelayCommand(OpenNewWindow);
        }

        private void OpenNewWindow(object? obj)
        {
            Debug.WriteLine("Opening New Window");
            EditingWindow newOpenWindow = new();
            newOpenWindow.Show();
        }
    }

}
