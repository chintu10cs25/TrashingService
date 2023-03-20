using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrashingService.Common
{
    internal class Type2Instruction
    {
        public Type2Instruction() { }
        public string BasePath { get; set; }
        public int Breadth { get; set; }
        public int Depth { get; set; }
        public long TotalSize { get; set; }
    }
}
