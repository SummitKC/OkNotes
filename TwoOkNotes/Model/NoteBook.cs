using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class GlobalMetaData
    {
        public List<string> NoteBooks { get; set; } = new();
        public List<string> OrphanPages { get; set; } = new();
    }

    public class NoteBookMetaData
    {
        public List<NoteBookSection> Sections { get; set; } = new();
        public int ActiveSectionIndex { get; set; } = 0;
    }

    public class SectionMetaData 
    {
        public List<NoteBookPage> Pages { get; set; } = new();
        public int ActivePageIndex { get; set; } = 0;
    }

}