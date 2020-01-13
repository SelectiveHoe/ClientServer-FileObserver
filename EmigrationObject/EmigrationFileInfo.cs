using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emigration
{
    [Serializable]
    public class EmigrationFileInfo
    {
        public long Size { get; set; }
        public string FileName { get; set; }
    }
}
