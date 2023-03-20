using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrashingService.Common
{
    internal class FileInstruction
    {
        public FileInstruction() { }
        public string StorageUnit { get; set; }
        public int FileSize { get; set; }
        public int NumOfFiles { get; set; }
        public string FileType { get; set; }
    }
}
