using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfConnectClient
{
    class Download
    {
        public long FullSize { get; set; }
        public long CurrentPos { get; set; }
        public string PathOnServer { get; set; }
        public string PathOnClient { get; set; }

        public int Progress { get; set; } //от 0 до 100

        Download()
        {

        }
    }
}
