using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class FileMetadata
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
    }
}
