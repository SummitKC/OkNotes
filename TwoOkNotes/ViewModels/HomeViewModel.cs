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
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {

        private string iconPath = "pack://application:,,,/Assets/Images/Testingbook.png";
        //Command Opeaning the editing window   
        public ICommand OpenWindow { get; }

        public ICommand LoadCurrentFileCommand { get; }

        //Initilizing the command
        public HomeViewModel()
        {
            OpenWindow = new RelayCommand(OpenNewWindow);
            LoadCurrentFileCommand = new RelayCommand(LoadCurrentFile);
        }

        private void LoadCurrentFile(object? obj)
        {
            EditingWindow editingWindow = new();
            EditingWIndowViewModel editingWindowViewModel = new();
            editingWindow.DataContext = editingWindowViewModel;
            editingWindow.Show();

            editingWindowViewModel.LoadNoteCommand.Execute(obj);
        }


        public string ImagePath
        {
            get => iconPath;
            set
            {
                iconPath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
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
