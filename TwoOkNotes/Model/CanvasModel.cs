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

        private double _zoomLevel = 1.0;
        private MatrixTransform _transform = new MatrixTransform();

        public string CanvasNoteName { get; set; }
        //Redo stack used for the redo button
        public int NoteID { get; set; }
        public Stack<StrokeTypeAction> RedoStack { get; set; }
        public Stack<StrokeTypeAction> UndoStack { get; set; }
        public PenViewModel PenViewModel { get; set; }
        private double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                _zoomLevel = value;
                ApplyZoom();
            }
        }

        //Initilizing the canvas
        public CanvasModel(String name)
        {
            InitilizeCanvas();
            CanvasNoteName = name;
            RedoStack = new Stack<StrokeTypeAction>();
            UndoStack = new Stack<StrokeTypeAction>();
            PenViewModel = new PenViewModel();
            this.RenderTransform = _transform;
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
        }
        //Selection Tool 
        public void SetSelectionTool(bool IsSelection)
        {
            if (IsSelection)
            {
                EditingMode = InkCanvasEditingMode.Select;
            }
            else
            {
                EditingMode = InkCanvasEditingMode.Ink;
            }
        }
        //Set the ink mode
        public void SetInk()
        {
            EditingMode = InkCanvasEditingMode.Ink;
        }

        //Clear method for the canvas
        public void ClearCanvas()
        {
            //SaveCurrentStrokes();
            Strokes.Clear();
        }

        private void ApplyZoom()
        {
            var matrix = new Matrix();
            matrix.Scale(_zoomLevel, _zoomLevel);
            _transform.Matrix = matrix;
        }

        public void ZoomIn()
        {
            ZoomLevel += 0.1;
        }

        public void ZoomOut()
        {
            ZoomLevel -= 0.1;
        }

        //private void SaveCurrentStrokes()
        //{
        //    StrokeCollection currentStrokes = new StrokeCollection(Strokes);
        //    UndoStack.Push(currentStrokes);
        //}   

    }
}
