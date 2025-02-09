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
using TwoOkNotes.Services;
using System.Configuration;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private string iconPath = "pack://application:,,,/Assets/Images/Testingbook.png";
        private FileSavingServices fileSavingServices { get; } = new();
        private double numberOfPagesl;
        private CanvasModel canvasModel;
        public PenViewModel CurrentPenModel { get; set; }

        //
        public ICommand OpenWindow { get; }
        public ICommand LoadCurrentFileCommand { get; }
        public ICommand LoadAFile { get;  }
        public ObservableCollection<PageModel> SavedPages { get; set;  }
        

        //Initilizing the command
        public HomeViewModel()
        {
            SavedPages = new ObservableCollection<PageModel>();
            LoadSavedPages();
            OpenWindow = new RelayCommand(OpenNewWindow);
            LoadCurrentFileCommand = new RelayCommand(LoadCurrentFile);
            LoadAFile = new RelayCommand(FindAndLoadFile);
            CurrentPenModel = new PenViewModel();
        }


        //just gives a new canvas model
        private CanvasModel GetCanvasModel()
        {
            return canvasModel = new("Untitled", new Stack<Stroke>());

        }

        //TODO: Change this to get it from the file services 
        private StrokeCollection GetCurrentStrokes(PageModel page)
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
                editingWindow.Title = "Untitled";
                GetCanvasModel().Strokes = GetCurrentStrokes(page);
                EditingWIndowViewModel editingWindowViewModel = new(canvasModel, CurrentPenModel, page.FilePath);
                editingWindow.DataContext = editingWindowViewModel;
                editingWindow.Show();
            }
        }

        //Get the directory of the notes and load them into the saved pages
        private async void LoadSavedPages()
        {
            Debug.WriteLine("gets to here 7");
            //Change later 
            var currFilesTest = await Task.Run(() => fileSavingServices.GetMetadataNameAndFilePathAsync());
            numberOfPagesl = currFilesTest.Count();
            Debug.WriteLine("gets to here 8", currFilesTest.Keys, currFilesTest.Values);

            foreach (var page in currFilesTest)
            {
                Debug.WriteLine(page.Key + page.Value);
                SavedPages.Add(new PageModel { Name = page.Key, FilePath = page.Value });
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
        private void FindAndLoadFile(object? obj)
        {
            string uFilePath = FileSavingServices.GetFilePathFromUser();
            OpenNewWindow(null); //remove this later 
        }

        //TODO: Move creating the file here
        private void OpenNewWindow(object? obj)
        {
            string filePath = $"{fileSavingServices.GetDefaultFilePath()}Untitled{numberOfPagesl}.isf";
            EditingWindow newOpenWindow = new();
            fileSavingServices.createFile(filePath);
            EditingWIndowViewModel editingWindowViewModel = new(GetCanvasModel(), CurrentPenModel, filePath);
            newOpenWindow.DataContext = editingWindowViewModel;
            newOpenWindow.Show();
        }
    }

}
