using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfConnectClient.DataBase
{
    public class DBDownItem
    {
        public int DBDownItemId { get; set; }
        public string PathOnServer { get; set; }
        public string PathOnClient { get; set; }
        public string Name { get; set; }
        public long FullSize { get; set; }
        public bool IsDownComplete { get; set; }
        public string FolderOnClient { get; set; }

        public int ServerItemId { get; set; }
        public virtual ServerItem ServerItem { get; set; }
    }
}
