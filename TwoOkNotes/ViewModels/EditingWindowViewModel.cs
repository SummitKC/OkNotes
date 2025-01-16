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
        private readonly string FilePath = "TempNoteFolder\\TestNote.idf";

        public CanvasModel CurrentCanvasModel { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ClearInkCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand LoadNoteCommand { get;  }

        public EditingWIndowViewModel()
        {
            CurrentCanvasModel = new CanvasModel("noteName");
            SaveNoteCommand = new RelayCommand(SaveNote);
            ClearInkCommand = new RelayCommand(ClearInk);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            LoadNoteCommand = new RelayCommand(LoadNote);
        }

        private void SaveNote(object? obj)
            
        {
            Debug.WriteLine("Saving Note");
            Console.WriteLine("Saving Note!");
            using FileStream fs = new(FilePath, FileMode.Create);
            CurrentCanvasModel.Strokes.Save(fs);
        }

        private void LoadNote(object? obj)
        {
            if (File.Exists(FilePath)){
                using (FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read))
                {

                    CurrentCanvasModel.Strokes = new StrokeCollection(fs);
                }
            }
        }


        private void ClearInk(object? obj)
        {
            CurrentCanvasModel.ClearCanvas();
        }

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
