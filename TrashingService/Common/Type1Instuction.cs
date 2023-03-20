using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrashingService.Common
{
    internal class Type1Instuction:IInstruction
    {
        public string BasePath { get; set; }
        public int NumOfDirectories { get; set; }
        public int NumOfSubdirectories { get; set; }
        public FileInstruction FileInstruction { get; set; }
        
    }
}
