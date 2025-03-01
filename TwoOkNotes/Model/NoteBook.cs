﻿using System;
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
        public List<string> Sections { get; set; } = new();
    }

    public class SectionMetaData 
    {
        public List<string> Pages { get; set; } = new();
    }

}