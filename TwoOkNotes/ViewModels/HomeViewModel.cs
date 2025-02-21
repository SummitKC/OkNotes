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
using System.Windows;

namespace TwoOkNotes.ViewModels
{
    public class HomeViewModel : ObservableObject
    {
        private string iconPath = "pack://application:,,,/Assets/Images/Testingbook.png";
        private FileSavingServices fileSavingServices { get; } = new();
        private double numberOfPagesl;
        private CanvasModel canvasModel;
        public PenViewModel CurrentPenModel { get; set; }


        private string _selectedSort; 
        public List<string> SortOptions { get; set; }

        public string SelectedSort
        {
            get => _selectedSort;
            set
            {
                _selectedSort = value;
                OnPropertyChanged(nameof(SelectedSort));
                sortSavedPages(_selectedSort);
            }
        }

        private bool _isNotebookInputVisible;
        public bool IsNotebookInputVisible
        {
            get => _isNotebookInputVisible;
            set
            {
                _isNotebookInputVisible = value;
                OnPropertyChanged(nameof(IsNotebookInputVisible));
            }
        }

        private bool _isPageInputVisible;
        public bool IsPageInputVisible
        {
            get => _isPageInputVisible;
            set
            {
                _isPageInputVisible = value;
                OnPropertyChanged(nameof(IsPageInputVisible));
            }
        }

        // User input properties
        private string _newNotebookName;
        public string NewNotebookName
        {
            get => _newNotebookName;
            set
            {
                _newNotebookName = value;
                OnPropertyChanged(nameof(NewNotebookName));
            }
        }

        private string _newPageName;
        public string NewPageName
        {
            get => _newPageName;
            set
            {
                _newPageName = value;
                OnPropertyChanged(nameof(NewPageName));
            }
        }


        //
        public ICommand LoadCurrentFileCommand { get; }
        public ICommand LoadAFileCommand { get;  }
        public ICommand CreateAPageCommand { get; }
        public ICommand CreateANoteBookCommand { get; }

        public ICommand ToggleNotebookInputCommand { get; }
        public ICommand CreateNotebookCommand { get; }
        public ICommand CancelNotebookCreationCommand { get; }

        public ICommand TogglePageInputCommand { get; }
        public ICommand CreatePageCommand { get; }
        public ICommand CancelPageCreationCommand { get; }

        private ObservableCollection<PageModel> _savedPages;
        public ObservableCollection<PageModel> SavedPages
        {
            get => _savedPages;
            set
            {
                _savedPages = value;
                OnPropertyChanged(nameof(SavedPages));
            }
        }


        //Initilizing the command
        public HomeViewModel()
        {
            SavedPages = new ObservableCollection<PageModel>();
            LoadSavedPages();

            IsNotebookInputVisible = false;
            IsPageInputVisible = false;

            // Initialize commands
            ToggleNotebookInputCommand = new RelayCommand(ToggleNotebookInput);
            CreateNotebookCommand = new RelayCommand(CreateNotebookAsync);
            CancelNotebookCreationCommand = new RelayCommand(CancelNotebookCreation);

            TogglePageInputCommand = new RelayCommand(TogglePageInput);
            CreatePageCommand = new RelayCommand(CreatePageAsync);
            CancelPageCreationCommand = new RelayCommand(CancelPageCreation);

            LoadCurrentFileCommand = new RelayCommand(LoadCurrentFile);
            LoadAFileCommand = new RelayCommand(FindAndLoadFile);

            CurrentPenModel = new PenViewModel();
           

            SortOptions = new List<string> { "Name", "Date"  };
        }

        //just gives a new canvas model
        private CanvasModel GetCanvasModel()
        {
            return canvasModel = new("Untitled");

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
            SavedPages.Clear();
            var currFilesTest = await fileSavingServices.GetMetadataNameAndFilePathAsync();
            numberOfPagesl = currFilesTest.Count;
            foreach (var item in currFilesTest)
            {
                if (item.Value.type == "OrphanPage")
                {
                    SavedPages.Add(new PageModel { Name = item.Key, FilePath = item.Value.Fileapth, LastUpdatedDate = item.Value.LastAccessTime });
                }
                else
                {
                    var notebookMetadata = await fileSavingServices.GetNotebookMetadata(item.Key);
                    if (notebookMetadata.Count > 0)
                    {
                        string Section = notebookMetadata[0];
                        var sectionMetadata = await fileSavingServices.GetSectionMetadata(item.Key, Section);
                        if (sectionMetadata.Count > 0)
                        {
                            string Page = sectionMetadata[0];
                            string filePath = fileSavingServices.GetCurrFilePath(item.Key, Section, Page);
                            SavedPages.Add(new PageModel { Name = item.Key, FilePath = filePath, LastUpdatedDate = item.Value.LastAccessTime });
                        }
                    }
                }
            }
        }

        private void sortSavedPages(string sortBy)
        {
            if (sortBy is string sortOption)
            {
                switch (sortOption)
                {
                    case "Name":
                        SavedPages = new ObservableCollection<PageModel>(SavedPages.OrderBy(x => x.Name));
                        break;
                    case "Date":
                        SavedPages = new ObservableCollection<PageModel>(SavedPages.OrderBy(x => x.LastUpdatedDate));
                        break;
                    default:
                        SavedPages = new ObservableCollection<PageModel>(SavedPages.OrderBy(x => x.LastUpdatedDate));
                        break;
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
        private void FindAndLoadFile(object? obj)
        {
            string uFilePath = FileSavingServices.GetFilePathFromUser();
        }

        //TODO: Move creating the file here
        private void OpenNewWindow(string name, string filePath)
        {
            EditingWindow newOpenWindow = new();
            newOpenWindow.Title = name;
            EditingWIndowViewModel editingWindowViewModel = new(GetCanvasModel(), CurrentPenModel, filePath);
            newOpenWindow.DataContext = editingWindowViewModel;
            newOpenWindow.Show();
        }

        // Toggle visibility of notebook input grid
        private void ToggleNotebookInput(object? obj)
        {
            IsNotebookInputVisible = !IsNotebookInputVisible;
            IsPageInputVisible = false; 
        }

        // Cancel notebook creation
        private void CancelNotebookCreation(object? obj)
        {
            IsNotebookInputVisible = false;
            NewNotebookName = string.Empty;
        }

        // Create notebook
        private async void CreateNotebookAsync(object? obj)
        {
            string notebookName = NewNotebookName?.Trim();
            if (!string.IsNullOrWhiteSpace(notebookName))
            {
                if (await fileSavingServices.CreateNotebook(notebookName))
                {
                    string filePath = fileSavingServices.GetCurrFilePath(notebookName, "Section1", "Page1.isf");
                    OpenNewWindow(notebookName, filePath);
                    NewNotebookName = string.Empty;
                    IsNotebookInputVisible = false;
                    LoadSavedPages(); 
                }
                else
                {
                    MessageBox.Show($"A notebook named '{notebookName}' already exists.", "Notebook Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please enter a notebook name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Toggle visibility of page input grid
        private void TogglePageInput(object? obj)
        {
            IsPageInputVisible = !IsPageInputVisible;
            IsNotebookInputVisible = false; 
        }

        // Cancel page creation
        private void CancelPageCreation(object? obj)
        {
            IsPageInputVisible = false;
            NewPageName = string.Empty;
        }

        // Create page
        private async void CreatePageAsync(object? obj)
        {
            string pageName = NewPageName?.Trim();
            if (!string.IsNullOrWhiteSpace(pageName))
            {
                if (await fileSavingServices.CreatePage(null, null, pageName))
                {
                    string filePath = fileSavingServices.GetCurrFilePath(null, null, pageName);
                    OpenNewWindow(pageName, filePath);
                    NewPageName = string.Empty;
                    IsPageInputVisible = false;
                    LoadSavedPages(); // Refresh the list
                }
                else
                {
                    MessageBox.Show($"A page named '{pageName}' already exists.", "Page Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please enter a page name.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

}
