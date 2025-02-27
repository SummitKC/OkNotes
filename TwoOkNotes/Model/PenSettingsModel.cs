using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class PenSettingsModel
    {
        public PenModel LastUsedPen { get; set; } = new ();
        public List<PenModel> Pens { get; set; } = new ();
    }
}
