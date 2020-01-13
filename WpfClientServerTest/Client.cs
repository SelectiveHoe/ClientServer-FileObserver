using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WpfClientServerTest
{
    class Client
    {
        public int id { get; set; }
        public NetworkStream stream { get; set; }
        public string name { get; set; }
        public TcpClient client { get; set; }
        public bool IsBusy { get; set; } = false;
    }
}
