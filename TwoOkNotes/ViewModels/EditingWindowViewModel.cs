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



namespace TwoOkNotes.ViewModels
{
    public class EditingWIndowViewModel : ObservableObject
    {
        private bool _isPenSettingOpen;
        private string _fileName;
        private string currFilePath;
        private string? currSection;
        private string? currPage;
        private readonly KeyHandler _keyHandler;
        private TimerHandler? _autoSaveTimer;
        private ObservableCollection<string> _sections;
        private ObservableCollection<string> _pages;
        private bool isPageGridVisible;
        private bool isSectionGridVisible;
        public WindowSettings _windowSettings { get; set; }
        private FileSavingServices _savingServices { get; set; }
        private SettingsServices _settingsSercices { get; set; }

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
        public ICommand ToggleHighlighterCommand { get; }
        public ICommand ToggleSelectionToolCommand { get; }
        public ICommand ToggleInkCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand NewPageCommand { get; }
        public ICommand NewSectionCommand { get; }
        public ICommand SwitchSectionCommand { get; }
        public ICommand SwitchPagesCommand { get; }
        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel(CanvasModel _currentCanvasModel, PenViewModel currentPenModel, string filePath, string fileName)
        {
            //Metadata 
            _savingServices = new FileSavingServices();
            _settingsSercices = new SettingsServices();
            _windowSettings = new WindowSettings();

            currFilePath = filePath;
            _fileName = fileName;
            CurrentCanvasModel = _currentCanvasModel;
            CurrentPenModel = currentPenModel;
            CurrentCanvasModel.SetPen(currentPenModel);
            _keyHandler = new KeyHandler(CurrentCanvasModel, CurrentPenModel, this);

            ClearInkCommand = new RelayCommand(ClearInk);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            TogglePenSettingsCommand = new RelayCommand(TogglePenSettings);
            ToggleEraserCommand = new RelayCommand(ToggleEraser);
            ToggleHighlighterCommand = new RelayCommand(ToggleHighlighter);
            ToggleSelectionToolCommand = new RelayCommand(ToggleSelectionTool);
            ToggleInkCommand = new RelayCommand(ToggleInk);
            ZoomInCommand = new RelayCommand(_ => CurrentCanvasModel.ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => CurrentCanvasModel.ZoomOut());
            SaveNoteCommand = new RelayCommand(_ => SaveNote());
            NewPageCommand = new RelayCommand(CreateNewPage);
            NewSectionCommand = new RelayCommand(CreateNewSection);
            SwitchSectionCommand = new RelayCommand(SwitchSections);
            SwitchPagesCommand = new RelayCommand(SwitchPages);

            SaveNote();
            InitAutoSaveTimer();
            SubscribeToStrokeEvents();
            InitilizeWindowDimentions();
            InitilizeSectionsAndPages();
          
        }

        private async void InitilizeWindowDimentions()
        {
            _windowSettings = await _settingsSercices.LoadEditingWindowSettings();
        }

        private async void InitilizeSectionsAndPages()
        {
            var sectionsList = await _savingServices.GetNotebookMetadata(_fileName);

            if (sectionsList.Count > 0) 
            {
            Sections = new ObservableCollection<string>(sectionsList);
                currSection = currSection ?? _sections[0];
                string sectionToLoad = currSection;
                var pagesList = await _savingServices.GetSectionMetadata(_fileName, sectionToLoad);
            Pages = new ObservableCollection<string>(pagesList);
                    }
            else {
                Pages = new ObservableCollection<string>();
                Pages.Add(_fileName);
                //TODO: Change this to toggle the visiability of the section grid to false
                //      Also lock the toggle button for the section grid to false 
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

        public ObservableCollection<string> Sections
        {
            get => _sections;
            set
            {
                _sections = value;
                OnPropertyChanged(nameof(Sections));
            }
        }

        public ObservableCollection<string> Pages
        {
            get => _pages;
            set
            {
                _pages = value;
                OnPropertyChanged(nameof(Pages));
            }
        }

        //temp only to test move out of this class later 
        private void DeleteNote(object? obj)
        {
            _savingServices.DeleteFile(currFilePath);
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

        private void ToggleHighlighter(object? obj)
        {
            //TODO: I don't think this is needed look into it later 
            if (obj is string str && bool.TryParse(str, out bool isHighlighter))
            {
                CurrentPenModel.IsHighlighter = isHighlighter;
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
            int numPages = Pages.Count;
            if (await _savingServices.CreatePage(_fileName, currSection, $"NewPage{numPages+1}.isf"))
            {
                InitilizeSectionsAndPages();
                SwitchPages($"NewPage{numPages+1}.isf");
            }
            else Debug.WriteLine("Creating New Page");
        }

        private async void CreateNewSection(object? obj)
        {
            int numSections = Sections.Count;
            await _savingServices.CreateSection($"NewSection{numSections}", _fileName);
            InitilizeSectionsAndPages();
            SwitchSections($"NewSection{numSections}");

        }

        private void SwitchSections(object? obj)
        {
            if (obj is string section && section != currSection)
            {
                currSection = section;
                InitilizeSectionsAndPages();
                SwitchPages(Pages[0]);
            }
        }

        private async void SwitchPages(object? obj)
        {
            if (obj is string page)
            {
                currPage = page;
                string updateFP = _savingServices.GetCurrFilePath(_fileName, currSection, currPage);
                currFilePath = updateFP;
                var pageContent = await FileSavingServices.GetFileContents(updateFP);
                CurrentCanvasModel.Strokes = pageContent;
                SubscribeToStrokeEvents();
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
            IsPenSettingOpen = !IsPenSettingOpen;
        }

        public async void SaveWindowSettings()
        {
            await _settingsSercices.SaveEditingWindowSettings(_windowSettings);
        }
    }
}