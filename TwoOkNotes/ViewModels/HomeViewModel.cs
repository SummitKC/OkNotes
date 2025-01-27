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
using System.Windows.Ink;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private readonly string notesDirectory = @"C:\Users\Summit\Source\Repos\OkNotes\TwoOkNotes\TempNoteFolder";

        private string iconPath = "pack://application:,,,/Assets/Images/Testingbook.png";
        //Command Opeaning the editing window
        private PenViewModel CurrentPenModel { get; set; }
        public ICommand OpenWindow { get; }
        public ICommand LoadCurrentFileCommand { get; }
        public ObservableCollection<PageModel> SavedPages { get; set;  }
        

        //Initilizing the command
        public HomeViewModel()
        {
            SavedPages = new ObservableCollection<PageModel>();
            LoadSavedPages();
            OpenWindow = new RelayCommand(OpenNewWindow);
            LoadCurrentFileCommand = new RelayCommand(LoadCurrentFile);
            CurrentPenModel = new PenViewModel();
        }

        private StrokeCollection getCurrentStrokes(PageModel page)
        {
            if (page != null)
            {
                    using FileStream fs = new(page.FilePath, FileMode.Open);
                    return new StrokeCollection(fs);
            }
            else return new StrokeCollection();
        }

        //Create a new editingWindow and it's viewmodel, set it's data context to the viewmodel, so the window is binded to that instance of the viewmodel and loads it in the window
        private void LoadCurrentFile(object? obj)
        {
            if (obj is PageModel page)
            {
                EditingWindow editingWindow = new();
                CanvasModel canvasModel = new("Untitled", new Stack<Stroke>());
                canvasModel.Strokes = getCurrentStrokes(page);
                EditingWIndowViewModel editingWindowViewModel = new(canvasModel, CurrentPenModel);
                editingWindow.DataContext = editingWindowViewModel;
                editingWindow.Show();
            }
        }
        //TODO: look into using db to store name and filepath? should be more efficient
        //Get the directory of the notes and load them into the saved pages
        private void LoadSavedPages()
        {
            //Change later 
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
