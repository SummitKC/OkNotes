using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class NoteBookSection
    {
        public required string Name { get; set; }
        public required double ID { get; set; }
        public required string FolderPath { get; set; }
        public required List<FileMetadata> FileMetadata { get; set; }
        public required DateTime LastUpdated { get; set; }

    }
}
