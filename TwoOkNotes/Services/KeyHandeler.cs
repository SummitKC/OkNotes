using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using TwoOkNotes.Model;
using TwoOkNotes.ViewModels;

namespace TwoOkNotes.Services
{
    public class KeyHandler
    {
        private readonly CanvasModel _canvasModel;
        private readonly PenViewModel _penViewModel;
        private readonly EditingWIndowViewModel _eViewModel;

        private bool ctlrKey = false;

        public KeyHandler(CanvasModel canvasModel, PenViewModel penViewModel, EditingWIndowViewModel eViewModel)
        {
            _canvasModel = canvasModel;
            _penViewModel = penViewModel;
            _eViewModel = eViewModel;
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
            if (ctlrKey && e.Key == Key.Z)
            {
                _eViewModel.UndoCommand.Execute(null);
            }
            if (ctlrKey && e.Key == Key.Y)
            {
                _eViewModel.RedoCommand.Execute(null);
            }
            if (ctlrKey && e.Key == Key.S)
            {
                _eViewModel.SaveNoteCommand.Execute(null);
            }
            if (ctlrKey && e.Key == Key.O)
            {
                //_eViewModel.OpenNoteCommand.Execute(null); //implement later 
            }
            if (ctlrKey && e.Key == Key.N)
            {
                //_eViewModel.NewNoteCommand.Execute(null); //implement later
            }
            if (ctlrKey && e.Key == Key.P)
            {
                //_eViewModel.PrintNoteCommand.Execute(null); //implement later
            }
            if (ctlrKey && e.Key == Key.OemPlus)
            {
                _canvasModel.ZoomIn();
            }
            if (ctlrKey && e.Key == Key.OemMinus)
            {
                _canvasModel.ZoomOut();
            }
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctlrKey = true;
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
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctlrKey = false;
            }
        }
        public void onMouseWheal(MouseWheelEventArgs e)
        {
            if (ctlrKey)
            {
                if (e.Delta > 0)
                {
                    _canvasModel.ZoomLevel += 0.1;
                }
                else
                {
                    _canvasModel.ZoomLevel -= 0.1;
                }
            }
        }
    }
}
