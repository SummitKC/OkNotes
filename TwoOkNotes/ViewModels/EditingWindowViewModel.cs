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


namespace TwoOkNotes.ViewModels
{
    public class EditingWIndowViewModel : ObservableObject
    {

        private bool _isPenSettingOpen;
        //List the current Canvas Model, and I commands for the buttons
        public CanvasModel CurrentCanvasModel { get; set; }
        public PenViewModel CurrentPenModel { get; set; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand LoadNoteCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand TogglePenSettingsCommand { get; }
        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel(CanvasModel _currentCanvasModel, PenViewModel currentPenModel)
        {
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
        }


        ////Creating a file
        //private void SaveNote(object? obj)

        //{
        //    Debug.WriteLine("Saving Note");
        //    using FileStream fs = new(FilePath, FileMode.Create);
        //    CurrentCanvasModel.Strokes.Save(fs);
        //}

        ////If the given file exists for the filepath, load the note
        //public void LoadNote(object? obj)
        //{
        //    Debug.WriteLine("Loading Note");
        //    if (File.Exists(FilePath))
        //        Debug.WriteLine("L");

        //        using (FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read))
        //        {
        //            CurrentCanvasModel.Strokes = new StrokeCollection(fs);
        //        }
        //}

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