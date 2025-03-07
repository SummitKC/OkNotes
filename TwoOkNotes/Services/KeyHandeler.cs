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
        private bool shiftKey = false;

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
            
            // Track modifier keys
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                ctlrKey = true;
            }
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftKey = true;
            }
            
            // Common shortcuts with Ctrl key
            if (ctlrKey)
            {
                switch (e.Key)
                {
                    case Key.Z:
                        _eViewModel.UndoCommand.Execute(null);
                        break;
                    case Key.Y:
                        _eViewModel.RedoCommand.Execute(null);
                        break;
                    case Key.S:
                        if (shiftKey && _eViewModel.IsNoteBook == true)
                            _eViewModel.NewSectionCommand.Execute(null); // Ctrl+Shift+S: New Section
                        else
                            _eViewModel.SaveNoteCommand.Execute(null);   // Ctrl+S: Save
                        break;
                    case Key.O:
                        //_eViewModel.OpenNoteCommand.Execute(null); //implement later 
                        break;
                    case Key.N:
                        if (shiftKey && _eViewModel.IsNoteBook == true)
                            _eViewModel.NewPageCommand.Execute(null);    // Ctrl+Shift+N: New Page
                        //_eViewModel.NewNoteCommand.Execute(null); //implement later
                        break;
                    case Key.P:
                        //_eViewModel.PrintNoteCommand.Execute(null); //implement later
                        break;
                    case Key.OemPlus:
                        _canvasModel.ZoomIn();
                        break;
                    case Key.OemMinus:
                        _canvasModel.ZoomOut();
                        break;
                    case Key.Add:
                        _canvasModel.ZoomIn();
                        break;
                    case Key.Subtract:
                        _canvasModel.ZoomOut();
                        break;
                    case Key.D0:
                        _canvasModel.ZoomLevel = 1.0; // Reset zoom to 100%
                        break;
                    case Key.E:
                        _eViewModel.ToggleEraserCommand.Execute("True"); // Ctrl+E: Toggle Eraser
                        break;
                    case Key.A:
                        _eViewModel.ToggleSelectionToolCommand.Execute("True"); // Ctrl+A: Toggle Selection Tool
                        break;
                    case Key.Delete:
                        _eViewModel.ClearInkCommand.Execute(null); // Ctrl+Delete: Clear All Ink
                        break;
                    case Key.V:
                        _eViewModel.ToggleVisibilityCommand.Execute(null); // Ctrl+V: Toggle Sidebar Visibility
                        break;
                }
            }
            else
            {
                // Shortcuts without modifiers
                switch (e.Key)
                {
                    case Key.F1:
                        // Toggle pen settings for current pen
                        if (_eViewModel.CurrentPenModel != null)
                            _eViewModel.TogglePenSettingsCommand.Execute(null);
                        break;
                    case Key.Escape:
                        // Cancel selection mode
                        _canvasModel.SetInk();
                        break;
                    case Key.D1:
                    case Key.D2:
                    case Key.D3:
                    case Key.D4:
                    case Key.D5:
                        // Switch to pen 1-5 if they exist
                        int penIndex = e.Key - Key.D1;
                        var pens = _eViewModel.PenModels;
                        if (pens != null && penIndex >= 0 && penIndex < pens.Count)
                        {
                            _eViewModel.SwitchPenCommand.Execute(pens[penIndex].Key);
                        }
                        break;
                }
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
            if (e.Key == Key.LeftShift || e.Key == Key.RightShift)
            {
                shiftKey = false;
            }
        }
        
        // Updated method name and return type
        public bool OnMouseWheel(MouseWheelEventArgs e)
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
                return true; // Return true to indicate the event was handled
            }
            return false; // Return false to allow ScrollViewer to handle the event
        }
    }
}
