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

namespace TwoOkNotes.Model
{
    public class CanvasModel : InkCanvas
    {
        //The name of the note
        public string CanvasNoteName { get; set; }
        //Redo stack used for the redo button
        public Stack<Stroke> RedoStack { get; set; }
        public PenModel PenModel { get; set; }

        //Initilizing the canvas
        public CanvasModel(String name, Stack<Stroke> redoStack)
        {
            CanvasNoteName = name;
            InitilizeCanvas();
            RedoStack = redoStack;
        }

        //default settings for the canvas
        private void InitilizeCanvas()
        {
            Background = Brushes.Black;
        }

        public void SetPen (PenModel penModel)
        {
            PenModel = penModel;
            DefaultDrawingAttributes = penModel.GetDrawingAttributes();
            PenModel.PropertyChanged += PenModelChanged;
        }

        private void PenModelChanged(object? sender, PropertyChangedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                DefaultDrawingAttributes = PenModel.GetDrawingAttributes();
            });
        }

        //Clear method for the canvas
        public void ClearCanvas()
        {
            this.Strokes.Clear();
        }

    }
}
