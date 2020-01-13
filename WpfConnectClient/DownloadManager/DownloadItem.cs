using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfConnectClient.DataBase;

namespace WpfConnectClient.DownloadManager
{
    public class DownloadItem : INotifyPropertyChanged
    {
        FileStream fs;
        string psi = "pause.png";
        public string pauseStopIcon {
            get
            {
                return psi;
            }
            set
            {
                psi = value;
                RaisePropertyChanged("pauseStopIcon");
            }
        }
        int progress = 0;
        public int ProgressBarProccess
        {
            get { return progress; }
            set
            {
                progress = value;
                RaisePropertyChanged("ProgressBarProccess");
            }
        }

        protected TcpClient client = new TcpClient();

        protected DownloadManager parrent;


        public string PathOnServer { get; set; }
        public string PathOnClient { get; set; }
        public string Name { get; set; }
        public long FullSize { get; set; } = 0;
        public long currSize { get; set; }
        public bool IsDownComplete { get; set; }
        public bool IsPause { get; set; }  
        public string FolderOnClient { get; set; }

        protected bool IsTimeOut = true;
        
        //info about server
        string _hostName { get; set; }
        int _port { get; set; }

        //CancellationTokenSource ts = new CancellationTokenSource();
        //CancellationToken ct;

        public DownloadItem(string pathOnServer, string hostname, int port, string pathFolderOnClient, DownloadManager par)
        {
            //ct = ts.Token;
            _hostName = hostname;
            _port = port;
            parrent = par;

            FolderOnClient = pathFolderOnClient;
            PathOnServer = pathOnServer;
            IsPause = true;
            IsDownComplete = false;
            ProgressBarProccess = 0;

            Task.Run(() => ProcessOfDownload());
        }

        public DownloadItem(DBDownItem item, string hostname, int port, string pathFolderOnClient, DownloadManager par)
        {
            //ct = ts.Token;
            _hostName = hostname;
            _port = port;
            parrent = par;

            if (item.FolderOnClient == null)
                FolderOnClient = pathFolderOnClient;
            else
                FolderOnClient = item.FolderOnClient;

            if(item.IsDownComplete)
            {
                if(File.Exists(item.PathOnClient))
                {
                    PathOnClient = item.PathOnClient;
                    PathOnServer = item.PathOnServer;
                    Name = new FileInfo(item.PathOnClient).Name;
                    FullSize = new FileInfo(item.PathOnClient).Length;
                    currSize = FullSize;
                    IsDownComplete = true;
                    IsPause = false;
                    ProgressBarProccess = 100;
                    pauseStopIcon = "CheckMark.png";
                    return;
                }
            }

            Task.Run(() => CheckDBFile(item));            
        }

        void CheckDBFile(DBDownItem item)
        {
            // Connecting
            client.Connect(_hostName, _port);
            PathOnServer = item.PathOnServer;
            string name = Environment.MachineName + PathOnServer;
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            client.GetStream().Write(bytes, 0, bytes.Length);
            bytes = new byte[2048];
            client.GetStream().Read(bytes, 0, bytes.Length);
            string mess = Encoding.UTF8.GetString(bytes);
            // Connecting

            //Check file exist on server
            mess = $"/FileExist {item.PathOnServer}";
            bytes = Encoding.UTF8.GetBytes(mess);
            client.GetStream().Write(bytes, 0, bytes.Length);
            bytes = new byte[2048];
            client.GetStream().Read(bytes, 0, bytes.Length);
            mess = Encoding.UTF8.GetString(bytes);
            mess = mess.Replace("\0", "");
            //Check file exist on server
            if (mess == "true")
            {
                if (File.Exists(item.PathOnClient))
                {
                    currSize = new FileInfo(item.PathOnClient).Length;
                    IsPause = false;
                    pauseStopIcon = "play.png";
                    
                    //Take info about file
                    
                    mess = $"/GetFile {PathOnServer}";
                    bytes = Encoding.UTF8.GetBytes(mess);
                    client.GetStream().Write(bytes, 0, bytes.Length);
                    bytes = new byte[500000];

                    Task.Run(() => ServerTimeOut());
                    client.GetStream().Read(bytes, 0, bytes.Length);
                    IsTimeOut = false;

                    Emigration.EmigrationObject takedinfo;
                    takedinfo = ByteArrayToObject(bytes) as Emigration.EmigrationObject;
                    if (takedinfo.type == "file")
                    {
                        Emigration.EmigrationFileInfo efi = (Emigration.EmigrationFileInfo)takedinfo.message;
                        name = efi.FileName;
                        FullSize = efi.Size;
                        if (DriveInfo.GetDrives().Where(a => a.Name == FolderOnClient).ToList().Count > 0)
                            PathOnClient = FolderOnClient + name;
                        else
                            PathOnClient = FolderOnClient + @"\" + name;
                        bytes = new byte[currSize];
                        client.GetStream().Read(bytes, 0, bytes.Length);
                        try
                        { 
                            using (fs = new FileStream(PathOnClient, FileMode.OpenOrCreate))
                            {
                                fs.Position = fs.Length;
                                ProgressBarProccess = CalculateValueProgressBar();
                                while (currSize < FullSize)
                                {
                                    //if (ct.IsCancellationRequested)
                                    //{
                                    //    break;
                                    //}
                                    if (IsPause)
                                    {
                                        bytes = new byte[1];
                                        client.GetStream().Read(bytes, 0, bytes.Length);

                                        fs.WriteByte(bytes[0]);

                                        currSize++;
                                        if (currSize % 1024 == 0)
                                            ProgressBarProccess = CalculateValueProgressBar();
                                    }
                                    else
                                        Thread.Sleep(1000);
                                }
                                ProgressBarProccess = 100;
                                IsDownComplete = true;
                                pauseStopIcon = "CheckMark.png";
                                parrent.SaveToDataBase();
                                client.Close();
                                client = null;
                            }
                        }
                    catch (Exception ex)
                    {
                        parrent.Message($"Can't open file {PathOnClient}, probably file busy.", "Client: ERROR");
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DeleteDownload()));
                    }
                }
                }
                else
                {
                    PathOnServer = item.PathOnServer;
                    IsPause = false;
                    pauseStopIcon = "play.png";
                    IsDownComplete = false;
                    ProgressBarProccess = 0;
                    client.Close();
                    client = new TcpClient();
                    ProcessOfDownload();
                }
            }
            else
            {
                DBInjector dbi = new DBInjector();
                dbi.DBDownItems.Remove(dbi.DBDownItems.FirstOrDefault(a => a.PathOnServer == item.PathOnServer && a.ServerItemId == item.ServerItemId));
                dbi.SaveChanges();
                parrent.Message($"File {PathOnServer} is not exist on server...", "Server: Error");
                DeleteDownload();
            }
        }

        public void StartStopDownload()
        {
            if (!IsDownComplete)
            {
                if (IsPause)
                {
                    IsPause = false;
                    pauseStopIcon = "play.png";
                }
                else
                {
                    IsPause = true;                    
                    pauseStopIcon = "pause.png";
                }
            }
        }


        public void DeleteDownload()
        {
            DBInjector dbi = new DBInjector();
            int idServer = dbi.Servers.ToList().Where(a => a.host == _hostName && a.port == _port).FirstOrDefault().ServerItemId;
            dbi.DBDownItems.Remove(dbi.DBDownItems.FirstOrDefault(a => a.PathOnServer == PathOnServer && a.ServerItemId == idServer));
            dbi.SaveChanges();
            parrent.RemoveItem(this);
        }

        public void Disconnect()
        {
            //ts.Cancel();
            if (client != null)
                client.Close();
        }

        public void OpenFolderWithFile()
        {
            if(File.Exists(PathOnClient))
                Process.Start(new ProcessStartInfo("explorer.exe", " /select, " + PathOnClient));
        }

        public void ProcessOfDownload()
        {
            parrent.SaveToDataBase();
            // Connecting
            client.Connect(_hostName, _port);
            string name = Environment.MachineName + PathOnServer;
            byte[] bytes = Encoding.UTF8.GetBytes(name);
            client.GetStream().Write(bytes, 0, bytes.Length);
            bytes = new byte[2048];
            client.GetStream().Read(bytes, 0, bytes.Length);
            string mess = Encoding.UTF8.GetString(bytes);
            // Connecting

            //Check file exist on server
            mess = $"/FileExist {PathOnServer}";
            bytes = Encoding.UTF8.GetBytes(mess);
            client.GetStream().Write(bytes, 0, bytes.Length);
            bytes = new byte[2048];
            client.GetStream().Read(bytes, 0, bytes.Length);
            mess = Encoding.UTF8.GetString(bytes);
            mess = mess.Replace("\0", "");
            //Check file exist on server
            if (mess == "true")
            {
                //Take info about file
                mess = $"/GetFile {PathOnServer}";
                bytes = Encoding.UTF8.GetBytes(mess);
                client.GetStream().Write(bytes, 0, bytes.Length);
                bytes = new byte[500000];

                Task.Run(() => ServerTimeOut());
                client.GetStream().Read(bytes, 0, bytes.Length);
                IsTimeOut = false;

                Emigration.EmigrationObject takedinfo;
                takedinfo = ByteArrayToObject(bytes) as Emigration.EmigrationObject;
                if(takedinfo.type == "file")
                {
                    Emigration.EmigrationFileInfo efi = (Emigration.EmigrationFileInfo)takedinfo.message;
                    name = efi.FileName;
                    FullSize = efi.Size;
                    if(DriveInfo.GetDrives().Where(a => a.Name == FolderOnClient).ToList().Count > 0)                    
                        PathOnClient = FolderOnClient + name;                    
                    else                    
                        PathOnClient = FolderOnClient + @"\" + name;
                    try
                    {
                        using (FileStream fs = new FileStream(PathOnClient, FileMode.Create))
                        {
                            while (currSize < FullSize)
                            {
                                //if (ct.IsCancellationRequested)
                                //{
                                //    break;
                                //}
                                if (IsPause)
                                {
                                    bytes = new byte[1];
                                    client.GetStream().Read(bytes, 0, bytes.Length);
                                    fs.WriteByte(bytes[0]);
                                    currSize++;
                                    if (currSize % 1024 == 0)
                                        ProgressBarProccess = CalculateValueProgressBar();
                                }
                                else
                                    Thread.Sleep(1000);
                            }
                            ProgressBarProccess = 100;
                            IsDownComplete = true;
                            pauseStopIcon = "CheckMark.png";
                            parrent.SaveToDataBase();
                            client.Close();
                            client = null;
                        }
                    }
                    catch(Exception ex)
                    {
                        parrent.Message($"Can't open file {PathOnClient}, probably file busy.", "Client: ERROR");
                        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => DeleteDownload()));
                    }
                }
            }
            else
            {
                parrent.Message($"File {PathOnServer} is not exist on server...", "Server: Error");
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(DeleteDownload));
            }
            
        }

        void ServerTimeOut()
        {
            Thread.Sleep(1000);
            if (IsTimeOut)
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background,new Action(DeleteDownload));
                parrent.Message($"Download time out, server can't to read {PathOnServer}, probably:\n-Server can't to open file.\n-File is busy another process.\n-File not exist.", "Server: Error");
            }
        }


        protected int CalculateValueProgressBar()
        {
            if(FullSize != 0)
            {
                double currProgress = ((double)currSize / (double)FullSize) * (double)100;
                return Convert.ToInt32(currProgress);
            }
            else
            {
                DeleteDownload();
                return 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private static Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }
    }
}
