using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Emigration
{
    [Serializable]
    public class EmigrationPathInfo
    {
        public byte[] ImagePath { get; set; }
        public string Name { get; set; }
        public bool isFile { get; set; }
    }
}
