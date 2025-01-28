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
        public Stack<Stroke> RedoStack { get; set; }
        public PenViewModel PenViewModel { get; set; }


        //Initilizing the canvas
        public CanvasModel(String name, Stack<Stroke> redoStack)
        {
            InitilizeCanvas();
            CanvasNoteName = name;
            RedoStack = redoStack;
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

        //Clear method for the canvas
        public void ClearCanvas()
        {
            Strokes.Clear();
        }

    }
}
