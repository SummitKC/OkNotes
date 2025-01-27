using System.Windows;
using TwoOkNotes.ViewModels;
using TwoOkNotes.Model;
using System.Windows.Ink;
using System.Collections.Generic;
using TwoOkNotes.ViewModels;
namespace TwoOkNotes.Views
{
    public partial class EditingWindow : Window
    {
        public EditingWindow(CanvasModel canvasModel, PenViewModel penViewModel)
        {
            InitializeComponent();
            DataContext = new EditingWIndowViewModel(canvasModel, penViewModel);
        }

        // Optional parameterless constructor
        public EditingWindow() : this(new CanvasModel("Untitled", new Stack<Stroke>()), new PenViewModel())
        {
        }
    }
}