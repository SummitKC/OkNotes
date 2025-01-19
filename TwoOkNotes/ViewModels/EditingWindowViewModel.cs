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


namespace TwoOkNotes.ViewModels
{
    public class EditingWIndowViewModel : ObservableObject
    {
        //TODO: Change this based on the user's chosen location 
        private readonly string FilePath = "TempNoteFolder\\TestNote.idf";

        //List the current Canvas Model, and I commands for the buttons
        public CanvasModel CurrentCanvasModel { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand LoadNoteCommand { get;  }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        //Setting commands for the buttons and Initilizing the Canvas Model
        public EditingWIndowViewModel()
        {
            CurrentCanvasModel = new CanvasModel("noteName", new Stack<Stroke>());
            SaveNoteCommand = new RelayCommand(SaveNote);
            ClearInkCommand = new RelayCommand(ClearInk);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            LoadNoteCommand = new RelayCommand(LoadNote);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);

        }


        //Creating a file
        private void SaveNote(object? obj)
            
        {
            Debug.WriteLine("Saving Note");
            Console.WriteLine("Saving Note!");
            using FileStream fs = new(FilePath, FileMode.Create);
            CurrentCanvasModel.Strokes.Save(fs);
        }

        //If the given file exists for the filepath, load the note
        private void LoadNote(object? obj)
        {
            if (File.Exists(FilePath)){
                using (FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read))
                {
                    CurrentCanvasModel.Strokes = new StrokeCollection(fs);
                    // TODO: Implement RedoStack
                    /*
                    CurrentCanvasModel.RedoStack = new Stack<Stroke>( Get from db);
                    */

                    /* foreach (Stroke stroke in CurrentCanvasModel.Strokes)
                    {
                        foreach (StylusPoint point in stroke.StylusPoints)
                        {
                            Debug.WriteLine(point.X + " " + point.Y);
                        }
                    } */
                }
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
        private void DeleteNote(object? obj)
        {
            Debug.WriteLine("Deleting File");

            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
                Debug.WriteLine("File Deleted");
            }
            else
            {
                Debug.WriteLine("File does not exist");
            }
        }
    }
}
