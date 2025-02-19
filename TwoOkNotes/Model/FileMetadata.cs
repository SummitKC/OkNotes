using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class FileMetadata
    {
        public string FileName { get; set; } = "Untitled";
        public string FilePath { get; set; } = "";
        public int Id { get; set; } = 1;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
    }
}
