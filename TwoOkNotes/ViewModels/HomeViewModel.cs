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

        //Command Opeaning the editing window   
        public ICommand OpenWindow { get; }

        //Initilizing the command
        public HomeViewModel()
        {
            OpenWindow = new RelayCommand(OpenNewWindow);
        }

        //Command logic for opening the new window
        private void OpenNewWindow(object? obj)
        {
            Debug.WriteLine("Opening New Window");
            EditingWindow newOpenWindow = new();
            newOpenWindow.Show();
        }
    }

}
