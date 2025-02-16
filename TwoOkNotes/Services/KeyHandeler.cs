using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TwoOkNotes.Model;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.Services
{
    public class KeyHandler
    {
        private readonly CanvasModel _canvasModel;
        private readonly PenViewModel _penViewModel;

        private double _zoomLevel = 1.0;

        public KeyHandler(CanvasModel canvasModel, PenViewModel penViewModel)
        {
            _canvasModel = canvasModel;
            _penViewModel = penViewModel;
        }

        public void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.L) //temp for test only 
            {
                _canvasModel.SetEraser(true, 1);
            }
            if (e.Key == Key.H)
            {
                _penViewModel.IsHighlighter = true;
            }

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                _canvasModel.ZoomLevel = _zoomLevel;
            }
        }

        public void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.L) //temp for test only 
            {
                _canvasModel.SetEraser(false, 1);
            }
            if (e.Key == Key.H)
            {
                _penViewModel.IsHighlighter = true;
            }
        }

        public void onMouseWheal(MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _zoomLevel += 0.1;
            }
            else
            {
                _zoomLevel -= 0.1;
            }
        }
    }
}
