using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoOkNotes.Util;
using System.Windows.Ink;
using System.Windows.Controls;
using System.IO;
using TwoOkNotes.Model;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using TwoOkNotes.Services;
using System.Collections.ObjectModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows;



namespace TwoOkNotes.ViewModels
{
    public class EditingWIndowViewModel : ObservableObject
    {
        private bool _isPenSettingOpen;
        private string _fileName;
        private string currFilePath;
        private NoteBookSection? currSection;
        private NoteBookPage? currPage;
        private readonly KeyHandler _keyHandler;
        private TimerHandler? _autoSaveTimer;
        private ObservableCollection<NoteBookSection> _sections;
        private ObservableCollection<NoteBookPage> _pages;
        private ObservableCollection<KeyValuePair<string, PenModel>> _penModels;
        private bool isPageGridVisible;
        private bool isSectionGridVisible;
        private bool _isNoteBook = true;
        public int _visibilityIndex = 0;
        private List<(bool section, bool page)> _visibilityStates = new List<(bool, bool)>
        {
            (true, true),
            (true, false),
            (false, false)
        };
        public WindowSettings _windowSettings { get; set; }
        private FileSavingServices _savingServices { get; set; }
        private SettingsServices _settingsSercices { get; set; }
        private int _activeSectionIndex = 0;
        private int _activePageIndex = 0;
        
        // Add properties for maximum window dimensions
        public double MaxWindowWidth { get; private set; }
        public double MaxWindowHeight { get; private set; }

        //List the current Canvas Model, and I commands for the buttons
        public CanvasModel CurrentCanvasModel { get; set; }
        public PenViewModel CurrentPenModel { get; set; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand ToggleEraserCommand { get; }
        public ICommand TogglePenSettingsCommand { get; }
        public ICommand ToggleSelectionToolCommand { get; }
        public ICommand ToggleInkCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand NewPageCommand { get; }
        public ICommand NewSectionCommand { get; }
        public ICommand SwitchSectionCommand { get; }
        public ICommand SwitchPagesCommand { get; }
        public ICommand SwitchPenCommand { get; }
        public ICommand AddPenCommand { get; }
        public ICommand ToggleVisibilityCommand { get; }
        public ICommand RenameSectionCommand { get; }
        public ICommand DeleteSectionCommand { get; }
        public ICommand RenamePageCommand { get; }
        public ICommand DeletePageCommand { get; }

        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel(CanvasModel _currentCanvasModel, string filePath, string fileName)
        {
            //Metadata and Settings 
            _savingServices = new FileSavingServices();
            _settingsSercices = new SettingsServices();
            _windowSettings = new WindowSettings();

            // Initialize max window dimensions to the primary screen size
            MaxWindowWidth = SystemParameters.PrimaryScreenWidth;
            MaxWindowHeight = SystemParameters.PrimaryScreenHeight;

            //Setting required things to models, or arguements being passed through 
            currFilePath = filePath;
            _fileName = fileName;
            CurrentCanvasModel = _currentCanvasModel;
            CurrentPenModel = new PenViewModel();
            CurrentCanvasModel.SetPen(CurrentPenModel);
            _penModels = CurrentPenModel.GetAvailablePens();
            _keyHandler = new KeyHandler(CurrentCanvasModel, CurrentPenModel, this);

            //Initilizing the commands for the buttons
            ClearInkCommand = new RelayCommand(ClearInk);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            TogglePenSettingsCommand = new RelayCommand(TogglePenSettings);
            ToggleEraserCommand = new RelayCommand(ToggleEraser);
            ToggleSelectionToolCommand = new RelayCommand(ToggleSelectionTool);
            ToggleInkCommand = new RelayCommand(ToggleInk);
            ZoomInCommand = new RelayCommand(_ => CurrentCanvasModel.ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => CurrentCanvasModel.ZoomOut());
            SaveNoteCommand = new RelayCommand(_ => SaveNote());
            NewPageCommand = new RelayCommand(CreateNewPage);
            NewSectionCommand = new RelayCommand(CreateNewSection);
            SwitchSectionCommand = new RelayCommand(SwitchSections);
            SwitchPagesCommand = new RelayCommand(SwitchPages);
            SwitchPenCommand = new RelayCommand(SwitchPen);
            AddPenCommand = new RelayCommand(AddNewPen);
            ToggleVisibilityCommand = new RelayCommand(ToggleVisibility);
            RenameSectionCommand = new RelayCommand(RenameSection);
            DeleteSectionCommand = new RelayCommand(DeleteSection);
            RenamePageCommand = new RelayCommand(RenamePage);
            DeletePageCommand = new RelayCommand(DeletePage);

            //Subscribing to the events for the pen model
            CurrentPenModel.PenDeleted += OnPenDeleted;
            CurrentPenModel.PenChanged += OnPenChanged;

            //Initilizing the window dimentions and pens                
            InitAutoSaveTimer();
            SubscribeToStrokeEvents();
            InitilizeWindowDimentionsAndPens();
            InitilizeSectionsAndPages();

        }

        //Initilizing the window dimentions and pens
        private async void InitilizeWindowDimentionsAndPens()
        {
            _windowSettings = await _settingsSercices.LoadEditingWindowSettings();

            _penModels = CurrentPenModel.GetAvailablePens();
            int attempts = 0;
            while (attempts < 3)
            {
                await Task.Delay(200);
                _penModels = CurrentPenModel.GetAvailablePens();
                attempts++;
            }
            OnPropertyChanged(nameof(PenModels));
        }

        //Initilizing the sections and pages
        private async void InitilizeSectionsAndPages()
        {
            await InitializeSections();
            await InitializePages(currSection);
        }

        //Initilizing the sections
        private async Task InitializeSections()
        {
            var result = await _savingServices.GetNotebookMetadata(_fileName);

            if (result.sections.Count > 0)
            {
                Sections = result.sections;
                // Use the stored active index
                _activeSectionIndex = result.activeIndex;
                
                // Validate the index is within bounds
                if (_activeSectionIndex < 0 || _activeSectionIndex >= Sections.Count)
                    _activeSectionIndex = 0;
                
                currSection = _sections[_activeSectionIndex];
                
                // Update UI active states
                UpdateSectionActiveState();
            }
            else
            {
                //if it's not a note book, no need to add sections
                _isNoteBook = false;
            }
        }

        //Initilizing the pages
        private async Task InitializePages(NoteBookSection? section)
        {
            if (section == null)
                return;

            var result = await _savingServices.GetSectionMetadata(_fileName, section.Name);
            Pages = result.pages;
            
            if (Pages.Count > 0)
            {
                // Use the stored active index
                _activePageIndex = result.activeIndex;
                
                // Validate the index is within bounds
                if (_activePageIndex < 0 || _activePageIndex >= Pages.Count)
                    _activePageIndex = 0;
                    
                currPage = Pages[_activePageIndex];
                
                // Update UI active states
                UpdatePageActiveState();
                
                await LoadPageContent(currPage);
            }
        }

        //Update the active state of the section to save to metadata so the user can begin where they left off last time
        private void UpdateSectionActiveState()
        {
            // Update IsActive flag based on current active index for UI
            for (int i = 0; i < Sections.Count; i++)
            {
                Sections[i].IsActive = (i == _activeSectionIndex);
            }
        }
        
        //Update the active state of the section to save to metadata so the user can begin where they left off last time
        private void UpdatePageActiveState()
        {
            // Update IsActive flag based on current active index for UI
            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].IsActive = (i == _activePageIndex);
            }
        }

        //Load the page content after switching 
        private async Task LoadPageContent(NoteBookPage page)
        {
            if (page != null && currSection != null)
            {
                // Deactivate current page if exists and different
                if (currPage != null && currPage != page)
                {
                    currPage.IsActive = false;
                }
                //Setting it's active flag to true for the Button Binding and 
                currPage = page;
                currPage.IsActive = true;
                
                string updateFP = _savingServices.GetCurrFilePath(_fileName, currSection.Name, currPage.Name);
                
                // Update the current file path
                currFilePath = updateFP;
                
                Debug.WriteLine($"Loading page from: {currFilePath}");
                var pageContent = await FileSavingServices.GetFileContents(updateFP);
                CurrentCanvasModel.Strokes = pageContent;
                SubscribeToStrokeEvents();
            }
        }

        //Window Width and Height properties
        public double WindowWidth
        {
            get => _windowSettings._windowWidth;
            set
            {
                // Ensure the window width doesn't exceed the screen width
                _windowSettings._windowWidth = Math.Min(value, MaxWindowWidth);
                OnPropertyChanged(nameof(WindowWidth));
                SaveWindowSettings();
            }
        }

        public double WindowHeight
        {
            get => _windowSettings._windowHeight;
            set
            {
                // Ensure the window height doesn't exceed the screen height
                _windowSettings._windowHeight = Math.Min(value, MaxWindowHeight);
                OnPropertyChanged(nameof(WindowHeight));
                SaveWindowSettings();
            }
        }

        public string WindowTitle
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        //Setting the visibility of the toggle button that toggles the visibility of the sections and pages
        public bool IsNoteBook
        {
            get => _isNoteBook;
            set
            {
                _isNoteBook = value;
                OnPropertyChanged(nameof(IsNoteBook));
            }
        }
        //Page Visiability toggle button
        public bool IsPagesGridVisible
        {
            get => isPageGridVisible;
            set
            {
                isPageGridVisible = value;
                OnPropertyChanged(nameof(IsPagesGridVisible));
            }
        }

        //Section Visiability toggle button
        public bool IsSectionsGridVisible
        {
            get => isSectionGridVisible;
            set
            {
                isSectionGridVisible = value;
                IsPagesGridVisible = value;
                OnPropertyChanged(nameof(IsSectionsGridVisible));
            }
        }

        //List of the sections
        public ObservableCollection<NoteBookSection> Sections
        {
            get => _sections;
            set
            {
                _sections = value;
                OnPropertyChanged(nameof(Sections));
            }
        }

        //List of the pages
        public ObservableCollection<NoteBookPage> Pages
        {
            get => _pages;
            set
            {
                _pages = value;
                OnPropertyChanged(nameof(Pages));
            }
        }
        //List of the pen models
        public ObservableCollection<KeyValuePair<string, PenModel>> PenModels
        {
            get => _penModels;
            set
            {
                _penModels = value;
                OnPropertyChanged(nameof(PenModels));
            }
        }

        //Cycle through the visibility states of the sections and pages
        public void ToggleVisibility(object? obj)
        {
            if (_visibilityIndex > 2) _visibilityIndex = 0;
            var visibilityState = _visibilityStates[_visibilityIndex];
            IsSectionsGridVisible = visibilityState.section;
            IsPagesGridVisible = visibilityState.page;
            _visibilityIndex++;
        }


        //Tick system for autosaving the note
        private void InitAutoSaveTimer()
        {
            _autoSaveTimer = new TimerHandler(TimeSpan.FromSeconds(5), Timer_Tick);
            _autoSaveTimer.Start();
        }

        //Calls the save note method when the timer ticks
        private void Timer_Tick(object? sender, EventArgs e)
        {
            SaveNote();
        }

        //On key down and key up events
        public void OnKeyDown(KeyEventArgs e)
        {
            _keyHandler.OnKeyDown(e);
        }

        //On key up events
        public void OnKeyUp(KeyEventArgs e)
        {
            _keyHandler.OnKeyUp(e);
        }

        //On mouse wheel events
        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            // Mark the event as handled based on KeyHandler's return value
            e.Handled = _keyHandler.OnMouseWheel(e);
        }


        //subscribe to the stroke events and call the save note method when the strokes are changed
        private void SubscribeToStrokeEvents()
        {
            CurrentCanvasModel.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        //When a pen is deleted, update the pen models
        private void OnPenDeleted(object? sender, EventArgs e)
        {
            PenModels = CurrentPenModel.GetAvailablePens();
        }

        //When a pen is changed, update the pen models
        private void OnPenChanged(object? sender, EventArgs e)
        {
            PenModels = CurrentPenModel.GetAvailablePens();
        }

        // when event is triggered, check if strokes are added or removed, if so, save the note
        private void Strokes_StrokesChanged(object? sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Added.Count > 0)
            {
                CurrentCanvasModel.UndoStack.Push(new StrokeTypeAction(e.Added[0], true));
                SaveNote();
            }

            if (e.Removed.Count > 0)
            {
                CurrentCanvasModel.UndoStack.Push(new StrokeTypeAction(e.Removed[0], false));
                SaveNote();
            }
        }

        //Save the note to the file
        private async void SaveNote()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                CurrentCanvasModel.Strokes.Save(ms);
                byte[] fileContent = ms.ToArray();
                await FileSavingServices.SaveFileAsync(currFilePath, fileContent);
            }
        }

        //Undo, check if there are any strokes in the canvas, if so, push the last stroke to the redo stack and remove it from the canvas
        private void Undo(object? obj)
        {
            CurrentCanvasModel.Strokes.StrokesChanged -= Strokes_StrokesChanged;

            if (CurrentCanvasModel.UndoStack.Count > 0)
            {
                var action = CurrentCanvasModel.UndoStack.Pop();
                if (action.TypeOfStroke)
                {
                    CurrentCanvasModel.Strokes.Remove(action.Stroke);
                }
                else
                {
                    // If it was a removal, add the stroke back
                    CurrentCanvasModel.Strokes.Add(action.Stroke);
                }
                CurrentCanvasModel.RedoStack.Push(action);
            }
            // Resubscribe to the event
            SubscribeToStrokeEvents();

            // Save the note state after undo since the event arg is not being called 
            SaveNote();
        }

        //Redo, checks if there is anything in the redo stack, if so, add the last stroke to the canvas
        private void Redo(object? obj)
        {
            //Need to unsubscribe from the event so this this stroke does not get pushed to the stack from the Removed event 
            CurrentCanvasModel.Strokes.StrokesChanged -= Strokes_StrokesChanged;

            if (CurrentCanvasModel.RedoStack.Count > 0)
            {
                var action = CurrentCanvasModel.RedoStack.Pop();
                if (action.TypeOfStroke)
                {
                    // If it was an addition, add the stroke back
                    CurrentCanvasModel.Strokes.Add(action.Stroke);
                }
                else
                {
                    // If it was a removal, remove the stroke
                    CurrentCanvasModel.Strokes.Remove(action.Stroke);
                }
                CurrentCanvasModel.UndoStack.Push(action);
            }
            // Resubscribe to the event
            SubscribeToStrokeEvents();

            // Save the note state after undo since the event arg is not being called 
            SaveNote();
        }

        private void ToggleEraser(object? obj)
        {
            if (obj is string str && bool.TryParse(str, out bool isEraser))
            {
                CurrentCanvasModel.SetEraser(isEraser, 1);
            }
        }

        private void ToggleSelectionTool(object? obj)
        {
            //TODO: Don't think the true or false arguements are needed here 
            CurrentCanvasModel.SetSelectionTool(true);
        }

        private void ToggleInk(object? obj)
        {
            CurrentCanvasModel.SetInk();
        }


        //Clear the ink from the canvas
        private void ClearInk(object? obj)
        {
            CurrentCanvasModel.ClearCanvas();
        }

        //Create new page
        private async void CreateNewPage(object? obj)
        {
            //Validate the current section
            if (currSection == null) return;

            //Get a unique page name since pages now can be deleted, setting it to Page Length + 1 would confligt with the existing names and would not create new files
            string newPageName = GenerateUniquePageName();
            //Call the create page method from the saving services
            if (await _savingServices.CreatePage(_fileName, currSection.Name, newPageName))
            {
                //Update the Page observable list Switch to the new page
                var newPage = new NoteBookPage { Name = newPageName };
                Pages.Add(newPage);
                SwitchPages(newPage);
            }
            else Debug.WriteLine("Creating New Page");
        }

        //Create new section
        private async void CreateNewSection(object? obj)
        {
            string newSectionName = GenerateUniqueSectionName();
            await _savingServices.CreateSection(newSectionName, _fileName);
            await InitializeSections();
            var newSection = _sections.FirstOrDefault(s => s.Name == newSectionName);
            if (newSection != null)
            {
                SwitchSections(newSection);
            }
        }

        //Generate a unique section name by checking the existing section names
        private string GenerateUniqueSectionName()
        {
            int highestNumber = 0;
            
            // Regular expression to match "Section X" where X is a number
            var regex = new System.Text.RegularExpressions.Regex(@"Section\s+(\d+)");
            
            foreach (var section in Sections)
            {
                var match = regex.Match(section.Name);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                {
                    if (number > highestNumber)
                    {
                        highestNumber = number;
                    }
                }
            }
            
            return $"Section {highestNumber + 1}";
        }

        //Generate a unique page name by checking the existing page names
        private string GenerateUniquePageName()
        {
            int highestNumber = 0;
            
            // Regular expression to match "Page X.isf" where X is a number
            var regex = new System.Text.RegularExpressions.Regex(@"Page\s+(\d+)");
            
            foreach (var page in Pages)
            {
                // Extract the name without extension for comparison
                string nameWithoutExt = Path.GetFileNameWithoutExtension(page.Name);
                var match = regex.Match(nameWithoutExt);
                if (match.Success && int.TryParse(match.Groups[1].Value, out int number))
                {
                    if (number > highestNumber)
                    {
                        highestNumber = number;
                    }
                }
            }
            
            return $"Page {highestNumber + 1}.isf";
        }

        //Switch the sections
        private async void SwitchSections(object? obj)
        {
            //Set the new index to -1 so if the incex is not found, it will not switch to another random page
            int newIndex = -1;
            
            if (obj is string sectionName)
            {
                newIndex = Sections.ToList().FindIndex(s => s.Name == sectionName);
            }
            else if (obj is NoteBookSection secObj)
            {
                newIndex = Sections.IndexOf(secObj);
            }

            if (newIndex >= 0)
            {
                // Save the current page content before switching
                if (currSection != null && currPage != null)
                {
                    SaveNote();
                }

                // Deactivate current section if and activate and switch to the new one 
                _activeSectionIndex = newIndex;
                currSection = Sections[_activeSectionIndex];
                
                UpdateSectionActiveState();
                
                // Save metadata with updated active index
                await _savingServices.UpdateSectionMetadata(_fileName, Sections, _activeSectionIndex);
                
                // Initialize pages with the new section
                await InitializePages(currSection);
            }
        }

        //Same as the switch sections but for the pages
        private async void SwitchPages(object? obj)
        {
            int newIndex = -1;
            
            if (obj is string pageName)
            {
                newIndex = Pages.ToList().FindIndex(p => p.Name == pageName);
            }
            else if (obj is NoteBookPage pageObj)
            {
                newIndex = Pages.IndexOf(pageObj);
                Debug.WriteLine(newIndex);
            }

            if (newIndex >= 0 && currSection != null)
            {
                _activePageIndex = newIndex;
                currPage = Pages[_activePageIndex];
                
                UpdatePageActiveState();
                
                string updateFP = _savingServices.GetCurrFilePath(_fileName, currSection.Name, currPage.Name);
                currFilePath = updateFP;
                var pageContent = await FileSavingServices.GetFileContents(updateFP);
                CurrentCanvasModel.Strokes = pageContent;
                SubscribeToStrokeEvents();
                
                // Save metadata with updated active index
                await _savingServices.UpdatePageMetadata(_fileName, currSection.Name, Pages, _activePageIndex);
            }
        }

        //Add a new pen to the pen models
        public void AddNewPen(object? obj)
        {
            CurrentPenModel.AddNewPen();
            PenModels = CurrentPenModel.GetAvailablePens();
        }

        //Switch the pen
        public void SwitchPen(object? obj)
        {
            if (obj is string penName)
            {
                CurrentCanvasModel.SetInk(); 
                CurrentPenModel.SwitchPen(penName);
            }
        }

        //Toggle the pen settings, and calls the OnPropertyChanged method when the state changes 
        public bool IsPenSettingOpen
        {
            get => _isPenSettingOpen;
            set
            {
                _isPenSettingOpen = value;
                OnPropertyChanged(nameof(IsPenSettingOpen));
            }
        }

        //When called the visivility of the pen settings will change to the opposite of what it is currently
        public void TogglePenSettings(object? obj)
        {
            // If a pen key is provided, switch to that pen first
            if (obj is string penKey)
            {
                CurrentPenModel.SwitchPen(penKey);
            }
            
            // Toggle the pen settings panel
            IsPenSettingOpen = !IsPenSettingOpen;
        }

        //Saving window settings 
        public async void SaveWindowSettings()
        {
            await _settingsSercices.SaveEditingWindowSettings(_windowSettings);
        }

        //Rename the section
        private async void RenameSection(object? parameter)
        {
            if (parameter is NoteBookSection section)
            {
                string oldName = section.Name;
                
                // Use the editing window as the owner instead of Application.Current.MainWindow
                var dialog = new Views.RenameDialog(oldName);
                
                // Find the current EditingWindow to use as owner
                Window? currentWindow = null;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.EditingWindow)
                    {
                        currentWindow = window;
                        break;
                    }
                }
                
                dialog.Owner = currentWindow;
                
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.NewName) && dialog.NewName != oldName)
                {
                    // Check if the section name already exists
                    if (Sections.Any(s => s.Name.Equals(dialog.NewName, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A section with this name already exists. Please choose a different name.",
                            "Name Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Save current page content before renaming
                    SaveNote();
                    
                    bool result = await _savingServices.RenameSection(_fileName, oldName, dialog.NewName);
                    if (result)
                    {
                        // Store the current page name before refreshing
                        string currentPageName = currPage?.Name ?? string.Empty;
                        
                        // Refresh sections and pages after rename
                        InitilizeSectionsAndPages();
                        
                        // Find the renamed section and select it
                        var renamedSection = _sections.FirstOrDefault(s => s.Name == dialog.NewName);
                        if (renamedSection != null)
                        {
                            SwitchSections(renamedSection);
                            
                            // If we had an active page, try to reactivate it with the updated path
                            if (!string.IsNullOrEmpty(currentPageName) && Pages.Any(p => p.Name == currentPageName))
                            {
                                var pageToSelect = Pages.First(p => p.Name == currentPageName);
                                SwitchPages(pageToSelect);
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to rename section. The name may be invalid or already exists.",
                            "Rename Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private async void DeleteSection(object? parameter)
        {

            if (parameter is NoteBookSection section)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete section '{section.Name}' and all its pages? This action cannot be undone.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = await _savingServices.DeleteSection(_fileName, section.Name);

                    if (deleted)
                    {
                        InitilizeSectionsAndPages();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete section.",
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private async void RenamePage(object? parameter)
        {
            if (parameter is NoteBookPage page && currSection != null)
            {
                // Extract the page name without extension for display
                string fileName = page.Name;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                
                // Use the existing RenameDialog window
                var dialog = new Views.RenameDialog(fileNameWithoutExt);
                
                // Find the current EditingWindow to use as owner
                Window? currentWindow = null;
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is Views.EditingWindow)
                    {
                        currentWindow = window;
                        break;
                    }
                }
                
                dialog.Owner = currentWindow;
                
                if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.NewName) && dialog.NewName != fileNameWithoutExt)
                {
                    // Add .isf extension if needed
                    string newNameWithExt = dialog.NewName.EndsWith(".isf", StringComparison.OrdinalIgnoreCase) 
                        ? dialog.NewName : dialog.NewName + ".isf";
                    
                    // Check if the page name already exists in this section
                    if (Pages.Any(p => p.Name.Equals(newNameWithExt, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A page with this name already exists in this section. Please choose a different name.",
                            "Name Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                        
                    bool result = await _savingServices.RenamePage(_fileName, currSection.Name, fileName, newNameWithExt);
                    if (result)
                    {
                        // Save any pending changes to the current page before refreshing
                        SaveNote();
                        
                        // Refresh pages after rename
                        await InitializePages(currSection);
                        
                        // Find and select the renamed page
                        var renamedPage = _pages.FirstOrDefault(p => p.Name == newNameWithExt);
                        if (renamedPage != null)
                        {
                            SwitchPages(renamedPage);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to rename page. The name may be invalid or already exists.",
                            "Rename Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private async void DeletePage(object? parameter)
        {
            if (parameter is NoteBookPage page && currSection != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"Are you sure you want to delete page '{Path.GetFileNameWithoutExtension(page.Name)}'? This action cannot be undone.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    bool deleted = await _savingServices.DeletePage(_fileName, currSection.Name, page.Name);
                    if (deleted)
                    {
                        await InitializePages(currSection);
                        
                        // If there are still pages, select the first one
                        if (_pages.Count > 0)
                        {
                            SwitchPages(_pages[0]);
                        }
                        else
                        {
                            // No pages left, create a new one
                            CreateNewPage(null);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete page.",
                            "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

    }
}



