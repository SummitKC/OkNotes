using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.IO;
using System.Windows.Ink;
using System.Windows.Media;

namespace TwoOkNotes.Model
{
    public class CanvasModel : InkCanvas
    {
        public string CanvasNoteName { get; set; }

        public CanvasModel(String name)
        {
            CanvasNoteName = name;
            InitilizeCanvas(); 
        }

        private void InitilizeCanvas()
        {
            Background = Brushes.Black;
            DefaultDrawingAttributes = new DrawingAttributes
            {
                Color = Colors.White,
                Height = 2,
                Width = 2,
                StylusTip = StylusTip.Ellipse
            };
            
        }

        public void ClearCanvas()
        {
            Strokes.Clear();
        }

    }
}
