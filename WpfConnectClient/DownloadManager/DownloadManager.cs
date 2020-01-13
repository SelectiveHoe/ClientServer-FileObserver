using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfConnectClient.DataBase;

namespace WpfConnectClient.DownloadManager
{
    public class DownloadManager
    {
        string _host;
        int _port;
        public string DownloadFolder { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); //по дефолту рабочий стол
        public ObservableCollection<DownloadItem> items = new ObservableCollection<DownloadItem>();
        protected MainWindow parrent;
        
        public DownloadManager(string host, int port, MainWindow _parrent)
        {
            parrent = _parrent;
            _host = host;
            _port = port;
            //<- тут должна быть загрузка из бд
            DBInjector dbi = new DBInjector();
            if (dbi.Servers.ToList().Where(a => a.host == _host && a.port == _port).ToList().Count <= 0)
            {
                dbi.Servers.Add(new ServerItem() { host = _host, port = _port, DBDownItems = new List<DBDownItem>() });
                dbi.SaveChanges();
            }
            else
            {
                int idServer = dbi.Servers.ToList().Where(a => a.host == _host && a.port == _port).FirstOrDefault().ServerItemId;
                foreach (DBDownItem item in dbi.DBDownItems.Where(a => a.ServerItemId == idServer))
                    items.Add(new DownloadItem(item, _host, _port, DownloadFolder, this));                                    
            }
        }

        public bool AddNewItem(string path)
        {
            foreach (DownloadItem di in items)
                if (di.PathOnServer == path)
                    return false;
            items.Add(new DownloadItem(path, _host, _port, DownloadFolder, this));            
            return true;
        }

        public void RemoveItem(DownloadItem Di)
        {
            items.Remove(Di);
        }

        public void Message(string message, string header)
        {
            parrent.MahAppsMessage(header, message);
        }

        public void SaveToDataBase()
        {
            foreach(DownloadItem item in items)
            {
                DBDownItem obj = new DBDownItem()
                {
                    FolderOnClient = item.FolderOnClient,
                    PathOnClient = item.PathOnClient,
                    PathOnServer = item.PathOnServer,
                    FullSize = item.FullSize,
                    Name = item.Name,
                    IsDownComplete = item.IsDownComplete
                };
                DBInjector dbi = new DBInjector();                
                if (dbi.Servers.ToList().Where(a => a.host == _host && a.port == _port).ToList().Count <= 0)
                {
                    dbi.Servers.Add(new ServerItem() { host = _host, port = _port, DBDownItems = new List<DBDownItem>() });
                    dbi.SaveChanges();
                }
                int idServer = dbi.Servers.ToList().Where(a => a.host == _host && a.port == _port).FirstOrDefault().ServerItemId;
                if (dbi.DBDownItems.ToList().Where(a => a.ServerItemId == idServer && a.PathOnClient == item.PathOnClient || a.PathOnServer == item.PathOnServer).ToList().Count <= 0)
                {
                    dbi.DBDownItems.Add(obj);
                    dbi.SaveChanges();
                    dbi.Servers.FirstOrDefault(a => a.host == _host && a.port == _port).DBDownItems.Add(obj);
                    dbi.SaveChanges();
                }
                else
                {
                    DBDownItem it = dbi.DBDownItems.FirstOrDefault(a => a.ServerItemId == idServer && a.PathOnClient == item.PathOnClient || a.PathOnServer == item.PathOnServer);
                    it.Name = obj.Name;
                    it.FolderOnClient = obj.FolderOnClient;
                    it.FullSize = obj.FullSize;
                    it.IsDownComplete = obj.IsDownComplete;
                    it.PathOnClient = obj.PathOnClient;
                    it.PathOnServer = obj.PathOnServer;                    
                    dbi.SaveChanges();
                }
            }
        }
    }
}
