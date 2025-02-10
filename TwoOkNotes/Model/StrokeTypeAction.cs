using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace TwoOkNotes.Model
{
    public class StrokeTypeAction
    {
        public Stroke Stroke { get; set; }
        public bool TypeOfStroke { get; set; }

        public StrokeTypeAction(Stroke stroke, bool type)
        {
            Stroke = stroke;
            TypeOfStroke = type;
        }

    }
}
