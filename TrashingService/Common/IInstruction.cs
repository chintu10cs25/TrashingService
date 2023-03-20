using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrashingService.Common
{
    internal interface IInstruction
    {
        string BasePath { get; set; }
    }
}
