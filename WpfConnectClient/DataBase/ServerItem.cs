using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfConnectClient.DataBase
{
    public class ServerItem
    {
        public int ServerItemId { get; set; }
        public string host { get; set; }
        public int port { get; set; }

        public virtual ICollection<DBDownItem> DBDownItems { get; set; } 
    }
}
