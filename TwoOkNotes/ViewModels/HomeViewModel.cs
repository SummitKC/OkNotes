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
using System.Collections.ObjectModel;
using TwoOkNotes.Model;
using System.IO;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {

        private string iconPath = "pack://application:,,,/Assets/Images/Testingbook.png";
        //Command Opeaning the editing window   
        public ICommand OpenWindow { get; }
        public ICommand LoadCurrentFileCommand { get; }



        public ObservableCollection<PageModel> SavedPages { get; set;  }

        //Initilizing the command
        public HomeViewModel()
        {
            SavedPages = new ObservableCollection<PageModel>();
            OpenWindow = new RelayCommand(OpenNewWindow);
            LoadSavedPages();
            LoadCurrentFileCommand = new RelayCommand(LoadCurrentFile);
        }


        //Create a new editingWindow and it's viewmodel, set it's data context to the viewmodel, so the window is binded to that instance of the viewmodel and loads it in the window
        private void LoadCurrentFile(object? obj)
        {
            if (obj is PageModel page)
            {
                EditingWindow editingWindow = new();
                EditingWIndowViewModel editingWindowViewModel = new();
                editingWindowViewModel.FilePath = page.FilePath;
                editingWindow.DataContext = editingWindowViewModel;
                editingWindow.Show();
                editingWindowViewModel.LoadNoteCommand.Execute(obj);
            }
        }
        //TODO: look into using db to store name and filepath? should be more efficient
        //Get the directory of the notes and load them into the saved pages
        private void LoadSavedPages()
        {
            //Change later 
            string notesDirectory = @"C:\Users\wajee\Source\Repos\OkNotes\TwoOkNotes\TempNoteFolder";

            if (Directory.Exists(notesDirectory))
            {
                var currentFiles = Directory.GetFiles(notesDirectory, "*.idf");
                foreach (var file in currentFiles)
                {
                    SavedPages.Add(new PageModel { Name = Path.GetFileNameWithoutExtension(file), FilePath = file });
                }
            }
        }

        //Icon's Getter and Setter
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
            EditingWindow newOpenWindow = new();
            newOpenWindow.Show();
        }
    }

}
