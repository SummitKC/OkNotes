using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwoOkNotes.Model
{
    public class DisplayingPagesModel
    {
        public required string Name { get; set; }
        public required string FilePath { get; set; }
        public required DateTime LastUpdatedDate { get; set; }
    }
}
