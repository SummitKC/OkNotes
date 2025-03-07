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
        private CanvasModel canvasModel;


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
        public ICommand CreateAPageCommand { get; }
        public ICommand CreateANoteBookCommand { get; }

        public ICommand ToggleNotebookInputCommand { get; }
        public ICommand CreateNotebookCommand { get; }
        public ICommand CancelNotebookCreationCommand { get; }

        public ICommand TogglePageInputCommand { get; }
        public ICommand CreatePageCommand { get; }
        public ICommand CancelPageCreationCommand { get; }

        public ICommand CycleSortCommand { get; }
        
        // New commands for rename and delete
        public ICommand RenameItemCommand { get; }
        public ICommand DeleteItemCommand { get; }

        private ObservableCollection<DisplayingPagesModel> _savedPages;
        public ObservableCollection<DisplayingPagesModel> SavedPages
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
            SavedPages = new ObservableCollection<DisplayingPagesModel>();
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
            
            // Initialize new commands
            RenameItemCommand = new RelayCommand(RenameItemAsync);
            DeleteItemCommand = new RelayCommand(DeleteItemAsync);

            SortOptions = new List<string> { "Name", "Date"  };
            SelectedSort = "Name";
        }

        //just gives a new canvas model
        private CanvasModel GetCanvasModel()
        {
            return canvasModel = new("Untitled");

        }

        //TODO: Change this to get it from the file services 


        //Create a new editingWindow and it's viewmodel, set it's data context to the viewmodel, so the window is binded to that instance of the viewmodel and loads it in the window
        private async void LoadCurrentFile(object? obj)
        {
            if (obj is DisplayingPagesModel page)
            {
                EditingWindow editingWindow = new();
                editingWindow.Title = "Untitled";
                GetCanvasModel().Strokes = await FileSavingServices.GetFileContents(page.FilePath);
                EditingWIndowViewModel editingWindowViewModel = new(canvasModel, page.FilePath, page.Name);
                editingWindow.DataContext = editingWindowViewModel;
                editingWindow.Show();
            }
        }

        //Get the directory of the notes and load them into the saved pages
        private async void LoadSavedPages()
        {
            SavedPages.Clear();
            var currFilesTest = await fileSavingServices.GetMetadataNameAndFilePathAsync();
            foreach (var item in currFilesTest)
            {
                if (item.Value.type == "OrphanPage")
                {
                    SavedPages.Add(new DisplayingPagesModel { Name = item.Key, FilePath = item.Value.Fileapth, LastUpdatedDate = item.Value.LastAccessTime, Icon = "📝" });
                }
                else
                {
                    var notebookMetadata = await fileSavingServices.GetNotebookMetadata(item.Key);
                    if (notebookMetadata.sections.Count > 0)
                    {
                        NoteBookSection Section = notebookMetadata.sections[0];
                        var sectionMetadata = await fileSavingServices.GetSectionMetadata(item.Key, Section.Name);
                        if (sectionMetadata.pages.Count > 0)
                        {
                            NoteBookPage Page = sectionMetadata.pages[0];
                            string filePath = fileSavingServices.GetCurrFilePath(item.Key, Section.Name, Page.Name);
                            SavedPages.Add(new DisplayingPagesModel { Name = item.Key, FilePath = filePath, LastUpdatedDate = item.Value.LastAccessTime, Icon = "📒" });
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
                        SavedPages = new ObservableCollection<DisplayingPagesModel>(SavedPages.OrderBy(x => x.Name));
                        break;
                    case "Date":
                        SavedPages = new ObservableCollection<DisplayingPagesModel>(SavedPages.OrderBy(x => x.LastUpdatedDate));
                        break;
                    default:
                        SavedPages = new ObservableCollection<DisplayingPagesModel>(SavedPages.OrderBy(x => x.LastUpdatedDate));
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

        //TODO: Move creating the file here
        private void OpenNewWindow(string name, string filePath)
        {
            EditingWindow newOpenWindow = new();
            newOpenWindow.Title = name;
            EditingWIndowViewModel editingWindowViewModel = new(GetCanvasModel(), filePath, name);
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
                    string filePath = fileSavingServices.GetCurrFilePath(notebookName, "Section 1", "Page 1.isf");
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
                    string filePath = fileSavingServices.GetCurrFilePath(null, null, $"{pageName}");
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

        // Rename a page or notebook
        private async void RenameItemAsync(object? parameter)
        {
            if (parameter is DisplayingPagesModel item)
            {
                // Get the current item name without extension for display
                string currentName = item.Name;
                /*
                 * Due to how it's set up earlier in developement, 
                 * notebook's filepath is set to it's first page so it will have the .isf extension
                 * while the page's file path is set up so the .isf will get added later on
                 * making .isf check is the opposite of what it should be
                 */
                string itemType = item.FilePath.Contains(".isf") ? "notebook" : "page";
                
                // Create and configure the rename dialog
                var dialog = new Views.RenameDialog(currentName);
                
                // Find the current window to use as owner
                Window? currentWindow = Application.Current.MainWindow;
                
                dialog.Owner = currentWindow;
                
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.NewName) && dialog.NewName != currentName)
                {
                    // Check if name already exists
                    if (SavedPages.Any(p => p.Name.Equals(dialog.NewName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show($"A {itemType} with this name already exists. Please choose a different name.",
                            "Name Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    bool result = false;
                    
                    // Determine if we're renaming a page or notebook
                    if (itemType == "page")
                    {
                        // For orphan pages
                        result = await fileSavingServices.RenameOrphanPage(currentName, dialog.NewName);
                    }
                    else
                    {
                        // For notebooks
                        result = await fileSavingServices.RenameNotebook(currentName, dialog.NewName);
                    }
                    
                    if (result)
                    {
                        LoadSavedPages(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show($"Failed to rename {itemType}. The name may be invalid or already exists.",
                            "Rename Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        // Delete a page or notebook
        private async void DeleteItemAsync(object? parameter)
        {
            if (parameter is DisplayingPagesModel item)
            {
                /*
                * Due to how it's set up earlier in developement, 
                * notebook's filepath is set to it's first page so it will have the .isf extension
                * while the page's file path is set up so the .isf will get added later on
                * making .isf check is the opposite of what it should be
                */
                string itemType = item.FilePath.Contains(".isf") ? "notebook" : "page";
                
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete {itemType} '{item.Name}'? This action cannot be undone.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = false;
                    
                    // Determine if we're deleting a page or notebook
                    if (itemType == "page")
                    {
                        // For orphan pages
                        deleted = await fileSavingServices.DeleteOrphanPage(item.Name);
                    }
                    else
                    {
                        // For notebooks
                        deleted = await fileSavingServices.DeleteNotebook(item.Name);
                    }
                    
                    if (deleted)
                    {
                        LoadSavedPages(); // Refresh the list
                    }
                    else
                    {
                        MessageBox.Show($"Failed to delete {itemType}.",
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }

}
