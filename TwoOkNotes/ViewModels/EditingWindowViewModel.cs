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

        //List the current Canvas Model, and I commands for the buttons
        public CanvasModel CurrentCanvasModel { get; set; }
        public PenViewModel CurrentPenModel { get; set; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        public ICommand DeleteNoteCommand { get; }
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
        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel(CanvasModel _currentCanvasModel, string filePath, string fileName)
        {
            //Metadata 
            _savingServices = new FileSavingServices();
            _settingsSercices = new SettingsServices();
            _windowSettings = new WindowSettings();

            currFilePath = filePath;
            _fileName = fileName;
            CurrentCanvasModel = _currentCanvasModel;
            CurrentPenModel = new PenViewModel();
            CurrentCanvasModel.SetPen(CurrentPenModel);
            _penModels = CurrentPenModel.GetAvailablePens();
            _keyHandler = new KeyHandler(CurrentCanvasModel, CurrentPenModel, this);

            ClearInkCommand = new RelayCommand(ClearInk);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
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

            CurrentPenModel.PenDeleted += OnPenDeleted;
            CurrentPenModel.PenChanged += OnPenChanged;

            SaveNote();
            InitAutoSaveTimer();
            SubscribeToStrokeEvents();
            InitilizeWindowDimentionsAndPens();
            InitilizeSectionsAndPages();

        }

        private async void InitilizeWindowDimentionsAndPens()
        {
            _windowSettings = await _settingsSercices.LoadEditingWindowSettings();
            _penModels = CurrentPenModel.GetAvailablePens();
            OnPropertyChanged(nameof(PenModels));
        }

        private async void InitilizeSectionsAndPages()
        {
            await InitializeSections();
            await InitializePages(currSection);
        }

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
                _isNoteBook = false;
            }
        }

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

        private void UpdateSectionActiveState()
        {
            // Update IsActive flag based on current active index for UI
            for (int i = 0; i < Sections.Count; i++)
            {
                Sections[i].IsActive = (i == _activeSectionIndex);
            }
        }

        private void UpdatePageActiveState()
        {
            // Update IsActive flag based on current active index for UI
            for (int i = 0; i < Pages.Count; i++)
            {
                Pages[i].IsActive = (i == _activePageIndex);
            }
        }

        private async Task LoadPageContent(NoteBookPage page)
        {
            if (page != null && currSection != null)
            {
                // Deactivate current page if exists and different
                if (currPage != null && currPage != page)
                {
                    currPage.IsActive = false;
                }
                
                currPage = page;
                currPage.IsActive = true;
                
                string updateFP = _savingServices.GetCurrFilePath(_fileName, currSection.Name, currPage.Name);
                currFilePath = updateFP;
                var pageContent = await FileSavingServices.GetFileContents(updateFP);
                CurrentCanvasModel.Strokes = pageContent;
                SubscribeToStrokeEvents();
            }
        }
        
        public double WindowWidth
        {
            get => _windowSettings._windowWidth;
            set
            {
                _windowSettings._windowWidth = value;
                OnPropertyChanged(nameof(WindowWidth));
                SaveWindowSettings();
            }
        }

        public double WindowHeight
        {
            get => _windowSettings._windowHeight;
            set
            {
                _windowSettings._windowHeight = value;
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

        public bool IsNoteBook
        {
            get => _isNoteBook;
            set
            {
                _isNoteBook = value;
                OnPropertyChanged(nameof(IsNoteBook));
            }
        }
        public bool IsPagesGridVisible
        {
            get => isPageGridVisible;
            set
            {
                isPageGridVisible = value;
                OnPropertyChanged(nameof(IsPagesGridVisible));
            }
        }

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

        public ObservableCollection<NoteBookSection> Sections
        {
            get => _sections;
            set
            {
                _sections = value;
                OnPropertyChanged(nameof(Sections));
            }
        }

        public ObservableCollection<NoteBookPage> Pages
        {
            get => _pages;
            set
            {
                _pages = value;
                OnPropertyChanged(nameof(Pages));
            }
        }

        public ObservableCollection<KeyValuePair<string, PenModel>> PenModels
        {
            get => _penModels;
            set
            {
                _penModels = value;
                OnPropertyChanged(nameof(PenModels));
            }
        }

        public void ToggleVisibility(object? obj)
        {
            if (_visibilityIndex > 2) _visibilityIndex = 0;
            var visibilityState = _visibilityStates[_visibilityIndex];
            IsSectionsGridVisible = visibilityState.section;
            IsPagesGridVisible = visibilityState.page;
            _visibilityIndex++;
        }

        //temp only to test move out of this class later 
        private void DeleteNote(object? obj)
        {
            _savingServices.DeleteFile(currFilePath);
            _ = InitializePages(currSection);
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

        //
        public void OnKeyDown(KeyEventArgs e)
        {
            _keyHandler.OnKeyDown(e);
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            _keyHandler.OnKeyUp(e);
        }

        public void OnMouseWheel(MouseWheelEventArgs e)
        {
            _keyHandler.onMouseWheal(e);
        }


        //subscribe to the stroke events and call the save note method when the strokes are changed
        private void SubscribeToStrokeEvents()
        {
            CurrentCanvasModel.Strokes.StrokesChanged += Strokes_StrokesChanged;
        }

        private void OnPenDeleted(object? sender, EventArgs e)
        {
            PenModels = CurrentPenModel.GetAvailablePens();
        }

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
                CurrentCanvasModel.SetEraser(isEraser, 0);
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

        private async void CreateNewPage(object? obj)
        {
            if (currSection == null) return;
            
            int numPages = Pages.Count;
            string newPageName = $"NewPage{numPages + 1}.isf";
            if (await _savingServices.CreatePage(_fileName, currSection.Name, newPageName))
            {
                InitilizeSectionsAndPages();
                var newPages = await _savingServices.GetSectionMetadata(_fileName, currSection.Name);
                var newPage = newPages.pages.FirstOrDefault(p => p.Name == newPageName);
                if (newPage != null)
                {
                    SwitchPages(newPage);
                }
            }
            else Debug.WriteLine("Creating New Page");
        }

        private async void CreateNewSection(object? obj)
        {
            int numSections = Sections.Count;
            string newSectionName = $"NewSection{numSections}";
            await _savingServices.CreateSection(newSectionName, _fileName);
            await InitializeSections();
            var newSection = _sections.FirstOrDefault(s => s.Name == newSectionName);
            if (newSection != null)
            {
                SwitchSections(newSection);
            }
        }

        private async void SwitchSections(object? obj)
        {
            int newIndex = -1;
            
            if (obj is string sectionName)
            {
                newIndex = Sections.ToList().FindIndex(s => s.Name == sectionName);
            }
            else if (obj is NoteBookSection secObj)
            {
                newIndex = Sections.IndexOf(secObj);
            }

            if (newIndex >= 0 && newIndex != _activeSectionIndex)
            {
                _activeSectionIndex = newIndex;
                currSection = Sections[_activeSectionIndex];
                
                UpdateSectionActiveState();
                
                // Save metadata with updated active index
                await _savingServices.UpdateSectionMetadata(_fileName, Sections, _activeSectionIndex);
                
                _ = InitializePages(currSection);
            }
        }

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

        public void AddNewPen(object? obj)
        {
            CurrentPenModel.AddNewPen();
            PenModels = CurrentPenModel.GetAvailablePens();
        }
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
        public async void SaveWindowSettings()
        {
            await _settingsSercices.SaveEditingWindowSettings(_windowSettings);
        }

    }
}