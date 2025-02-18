using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class NoteBook
    {
        public required List<NoteBookSection> Setctions { get; set; }
    }
}