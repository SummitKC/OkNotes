using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using TwoOkNotes.Model;
using TwoOkNotes.Util;

namespace TwoOkNotes.ViewModels
{
    public class PenViewModel : ObservableObject
    {

        public readonly PenModel _penModel = new();
        //public ICommand ChangePenColorCommand { get; }

        public PenViewModel(PenModel penModel)
        {

            _penModel = penModel;

        }

        public Color PenColor
        {
            get => _penModel.PenColor;
            set
            {
                _penModel.PenColor = value;
                OnPropertyChanged(nameof(PenColor));
            }

        }
      
    }
}

