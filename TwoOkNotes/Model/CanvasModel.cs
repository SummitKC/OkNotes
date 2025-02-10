//CanvasModel
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows.Ink;
using System.Windows.Media;
using System.ComponentModel;
using TwoOkNotes.ViewModels;
using System.Diagnostics;

namespace TwoOkNotes.Model
{
    public class CanvasModel : InkCanvas
    {
        //The name of the note
        public string CanvasNoteName { get; set; }
        //Redo stack used for the redo button
        public int NoteID { get; set; }
        public Stack<StrokeTypeAction> RedoStack { get; set; }
        public Stack<StrokeTypeAction> UndoStack { get; set; }
        public PenViewModel PenViewModel { get; set; }


        //Initilizing the canvas
        public CanvasModel(String name)
        {
            InitilizeCanvas();
            CanvasNoteName = name;
            RedoStack = new Stack<StrokeTypeAction>();
            UndoStack = new Stack<StrokeTypeAction>();
            PenViewModel = new PenViewModel();
        }

        //default settings for the canvas
        private void InitilizeCanvas()
        {
            Background = Brushes.Black;
        }

        //Set pen with the current, and update on when any aspect of the pen changes
        public void SetPen(PenViewModel penModel)
        {
            PenViewModel = penModel;
            Debug.WriteLine("gets to here");
            DefaultDrawingAttributes = penModel.GetDrawingAttributes();
            PenViewModel.PropertyChanged += PenModelChanged;
        }

        //Event listener for when the pen model changes
        private void PenModelChanged(object? sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DefaultDrawingAttributes = PenViewModel.GetDrawingAttributes();
            });
        }

        //Set the eraser mode
        public void SetEraser(bool isEraser, int mode) //add a parameter to erase by point aswell 
        {
            if (isEraser && mode == 0)
            {
                //SaveCurrentStrokes();
                EditingMode = InkCanvasEditingMode.EraseByStroke;
            }
            else if (isEraser && mode == 1)
            {
                //SaveCurrentStrokes();
                EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else
            {
                EditingMode = InkCanvasEditingMode.Ink;
            }
        }

        //Clear method for the canvas
        public void ClearCanvas()
        {
            //SaveCurrentStrokes();
            Strokes.Clear();
        }

        //private void SaveCurrentStrokes()
        //{
        //    StrokeCollection currentStrokes = new StrokeCollection(Strokes);
        //    UndoStack.Push(currentStrokes);
        //}   

    }
}
