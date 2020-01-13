using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Emigration
{
    [Serializable]
    public class EmigrationObject
    {
        public string type { get; set; }
        public object message { get; set; }
    }
}
