using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoOkNotes.Model;

namespace TwoOkNotes.ViewModels
{
    class PenViewModel : EditingWIndowViewModel
    {

        public PenModel CurrentPen { get; set; }

        public PenViewModel(PenModel pen)
        {
            CurrentPen = pen;
        }

    }
}
