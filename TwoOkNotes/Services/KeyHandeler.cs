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
    }
}
