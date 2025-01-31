﻿using System;
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


namespace TwoOkNotes.ViewModels
{
    public class EditingWIndowViewModel : ObservableObject
    {
        private DispatcherTimer _timer;
        private bool _isPenSettingOpen;
        private string currFilePath;

        private FileSavingServices _savingServices { get; set; }
        //List the current Canvas Model, and I commands for the buttons
        public CanvasModel CurrentCanvasModel { get; set; }
        public PenViewModel CurrentPenModel { get; set; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        //public ICommand DeleteNoteCommand { get; }
        //public ICommand LoadNoteCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand TogglePenSettingsCommand { get; }
        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel(CanvasModel _currentCanvasModel, PenViewModel currentPenModel, string filePath)
        {
            _savingServices = new FileSavingServices();
            currFilePath = filePath;
            CurrentCanvasModel = _currentCanvasModel;
            CurrentPenModel = currentPenModel;
            CurrentCanvasModel.SetPen(currentPenModel);
            //SaveNoteCommand = new RelayCommand(SaveNote);
            ClearInkCommand = new RelayCommand(ClearInk);
            //DeleteNoteCommand = new RelayCommand(DeleteNote);
            //LoadNoteCommand = new RelayCommand(LoadNote);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            TogglePenSettingsCommand = new RelayCommand(TogglePenSettings);
            CreateNote();
            SaveNote();
            InitTimer();
            SubscribeToStrokeEvents();
        }

        //Tick system for autosaving the note
        private void InitTimer()
        {
                _timer = new DispatcherTimer();
                _timer.Interval = TimeSpan.FromSeconds(20);
                _timer.Tick += Timer_Tick;
                _timer.Start();
        }

        //Calls the save note method when the timer ticks
        private void Timer_Tick(object? sender, EventArgs e)
        {
            SaveNote();
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
                Debug.WriteLine("Strokes added");
                SaveNote();
            }

            if (e.Removed.Count > 0)
            {
                Debug.WriteLine("Strokes removed");
                SaveNote();
            }
        }
        //TODO: Move to homeviewmodel, for testing only 
        private void CreateNote()

        {
            using FileStream fs = new(currFilePath, FileMode.Create);
            CurrentCanvasModel.Strokes.Save(fs);
        }

        private async void SaveNote()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                CurrentCanvasModel.Strokes.Save(ms);
                byte[] fileContent = ms.ToArray();
                Debug.WriteLine("Does it get here?");
                await _savingServices.SaveFileAsync(currFilePath, fileContent);
            }
        }

        //Undo, check if there are any strokes in the canvas, if so, push the last stroke to the redo stack and remove it from the canvas
        public void Undo(object? obj)
        {
            //TODO: Add error handling
            if (CurrentCanvasModel.Strokes.Count > 0)
            {
                CurrentCanvasModel.RedoStack.Push(CurrentCanvasModel.Strokes[CurrentCanvasModel.Strokes.Count - 1]);
                CurrentCanvasModel.Strokes.RemoveAt(CurrentCanvasModel.Strokes.Count - 1);
            }
        }

        //Redo, checks if there is anything in the redo stack, if so, add the last stroke to the canvas
        public void Redo(object? obj)
        {
            //TODO: Add error handling
            if (CurrentCanvasModel.RedoStack.Count > 0)
            {
                CurrentCanvasModel.Strokes.Add(CurrentCanvasModel.RedoStack.Pop());
            }
        }

        //Clear the ink from the canvas
        private void ClearInk(object? obj)
        {
            CurrentCanvasModel.ClearCanvas();
        }

        //Delete file 
        //private void DeleteNote(object? obj)
        //{
        //    Debug.WriteLine("Deleting File");

        //    if (File.Exists(FilePath))
        //    {
        //        File.Delete(FilePath);
        //        Debug.WriteLine("File Deleted");
        //    }
        //    else
        //    {
        //        Debug.WriteLine("File does not exist");
        //    }
        //}

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
    }
}