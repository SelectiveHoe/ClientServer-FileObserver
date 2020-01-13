using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Emigration;
using System.Drawing;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.ComponentModel;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Collections.ObjectModel;
using WpfConnectClient.DataBase;

namespace WpfConnectClient
{
    public partial class MainWindow : MetroWindow
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        string DownloadPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        static string userName = Environment.MachineName;
        static string host = "77.93.61.5";
        static int port = 21025;
        static TcpClient client;
        static NetworkStream stream;
        string currentFTPpath = "";
        bool isFileDownload = false;

        public DownloadManager.DownloadManager DownMan;

        public MainWindow()
        {
            InitializeComponent();
            Title = "Client: Disconnect.";
            BtnChangeSaveFolder.IsEnabled = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Tabs.Visibility = Visibility.Hidden;
            string ip = ConnectionIP.Text;
            string port = ConnectionPort.Text;
            if (client != null)
                client.Close();
            client = null;
            stream = null;
            FolderTreeView.Items.Clear();
            LabelPathFolder.Content = "";
            if (DownMan != null)
            {
                for (int i = 0; i < DownMan.items.Count; i++)
                    DownMan.items[i].Disconnect();
                DownMan.items.Clear();
                DownMan = null;
            }
            Task.Run(() => Connection(ip, port));            
        }

        void Connection(string Host, string Port)
        {            
            host = Host;
            if (!int.TryParse(Port, out port))
            {
                LoadingAnim.Set(() => this.ShowMessageAsync("Error", "Connectriong port incorrect. Try again."));
                Tabs.Set(() => Tabs.Visibility = Visibility.Visible);
                return;
            }
            client = new TcpClient();
            try
            {
                client.Connect(host, port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                string message = userName;
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);


                int bytes = 0;
                data = new byte[256];
                bytes = stream.Read(data, 0, data.Length);
                message = Encoding.UTF8.GetString(data, 0, bytes);
                //MessageBox.Show(message, "Infomation", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                LoadingAnim.Set(() => this.ShowMessageAsync("Infomation", message));


                DBInjector dbi = new DBInjector();
                if(dbi.Servers.ToList().Where(a => a.host == host && a.port == port).ToList().Count <= 0)
                {
                    dbi.Servers.Add(new ServerItem() { host = Host, port = port, DBDownItems = new List<DBDownItem>() });
                    dbi.SaveChanges();
                }

                Button_Click_1(null, null);

                Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                receiveThread.Start();
                Tabs.Set(() => {
                    Tabs.Visibility = Visibility.Visible;
                    Title = "Client: Connect.";
                });
                DownMan = new DownloadManager.DownloadManager(host, port, this);
                DMList.Set(() => { 
                    DMList.ItemsSource = DownMan.items;
                    SaveFolder.Text = DownMan.DownloadFolder;
                    BtnChangeSaveFolder.IsEnabled = true;
                });
            }
            catch (Exception ex)
            {
                LoadingAnim.Set(() => this.ShowMessageAsync("Error", ex.Message));
                Tabs.Set(() => Tabs.Visibility = Visibility.Visible);
            }
            Tabs.Set(() => Tabs.SelectedIndex = 1);
        }

        //Отправка сообщений
        void SendMessage(string message)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
            }
            catch(Exception ex)
            {
                Tabs.Set(() => this.ShowMessageAsync("Error", "Please, first connect to server."));
            }
        }

        static void SendMessage(byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        // получение сообщений
        void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    EmigrationObject message;
                    int bytes = 0;
                    do
                    {
                        byte[] data = new byte[500000];
                        bytes = stream.Read(data, 0, data.Length);
                        message = ByteArrayToObject(data) as EmigrationObject;
                        if (message.type == "file")
                        {
                            EmigrationFileInfo efi = (EmigrationFileInfo)message.message;
                            Tabs.Set(() => Tabs.Visibility = Visibility.Hidden);
                            DownloadFile(efi);
                            Tabs.Set(() => Tabs.Visibility = Visibility.Visible);
                        }
                    }
                    while (stream.DataAvailable);

                    switch (message.type)
                    {
                        case "string":

                            break;
                        case "List<EmigrationPathInfo>":
                            List<EmigrationPathInfo> list = (List<EmigrationPathInfo>)message.message;
                            currentFTPpath = list[0].Name;
                            LabelPathFolder.Set(() => LabelPathFolder.Content = currentFTPpath);
                            list.RemoveAt(0);
                            FolderTreeView.Set(() => FolderTreeView.Items.Clear());
                            FolderTreeView.Set(() => {
                                list.ForEach(a => {
                                    TemplateFileElement tfe = new TemplateFileElement();
                                    tfe.isFile = a.isFile;
                                    tfe.path = a.Name;
                                    tfe.icon = ToImageSource(BytesToIcon(a.ImagePath));
                                    FolderTreeView.Items.Add(tfe);
                                });
                            });
                            break;
                        case "DownloadComplete":
                            Tabs.Set(() => Tabs.Visibility = Visibility.Visible);                            
                            break;
                    }
            }
                catch (Exception ex)
            {
                    Tabs.Set(() => {
                        Tabs.Set(() => Tabs.SelectedIndex = 0);
                        Title = "Client: Disconnect.";
                        this.ShowMessageAsync("Error", ex.Message);
                    });
                return;
            }
        }
        }

        void DownloadFile(EmigrationFileInfo efi)
        {                        
            //CancelDown.Set(() => CancelDown.IsEnabled = true);
            try
            {
                isFileDownload = true;
                using (FileStream fs = new FileStream(DownloadPath + "\\" + efi.FileName, FileMode.Create))
                {
                    //PgDownloadFile.Set(() =>
                    //{
                    //    PgDownloadFile.Maximum = efi.Size;
                    //    PgDownloadFile.Minimum = 0;
                    //});
                    long currentBytes = 0;
                    byte[] data;
                    while (currentBytes < efi.Size)
                    {
                        data = new byte[1];
                        stream.Read(data, 0, data.Length);
                        fs.WriteByte(data[0]);
                        currentBytes++;
                        //if (currentBytes % 1000000 == 0)
                        //{
                        //    PgDownloadFile.Set(() =>
                        //    {
                        //        PgDownloadFile.Value = currentBytes;
                        //    });
                        //}
                    }
                    //PgDownloadFile.Set(() =>
                    //{
                    //    PgDownloadFile.Value = 0;
                    //});
                }
                isFileDownload = false;
            }
            catch (Exception ex) { Title = ex.Message; }
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

        byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SendMessage("/FolderInfo");
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (DownMan != null)
                DownMan.SaveToDataBase();
            Disconnect();
        }

        static void Disconnect()
        {
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента         

            stream = null;
            client = null;               
        }

        private void FolderTreeView_Drop(object sender, DragEventArgs e)
        {
            Tabs.Visibility = Visibility.Hidden;
            string[] pathes = (e.Data.GetData(DataFormats.FileDrop) as string[]);
            Task.Run(() => DropFile(pathes.ToList()));
        }

        void DropFile(List<string> files)
        {
            SendMessage("/DropFile"); // отправляем команду серверу на получение информации об транзакции
            Thread.Sleep(50);
            EmigrationFileInfo efi = new EmigrationFileInfo(); //формируем информацию об транзакции
            efi.Size = files.Count;
            efi.FileName = currentFTPpath;
            SendMessage(ObjectToByteArray(efi)); // отправляем информацию серверу про количество файлов и пути для сохранения
            Thread.Sleep(50);
            foreach (string filepath in files)
            {
                try
                {
                    using (FileStream fs = new FileStream(filepath, FileMode.Open)) //проверяем все файлы на то можем ли мы их открыть, если нет выходим из функции
                    {

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Exception with drop file", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            // поочерёдная передача файлов
            foreach (string filepath in files)
            {
                FileInfo fi = new FileInfo(filepath);
                efi = new EmigrationFileInfo() { FileName = fi.Name, Size = fi.Length };
                SendMessage(ObjectToByteArray(efi));//отправляем информацию про файл
                Thread.Sleep(50);
                using (FileStream fs = new FileStream(filepath, FileMode.Open))
                {
                    byte[] File = new byte[fs.Length];
                    fs.Read(File, 0, File.Length);
                    SendMessage(File);
                    Thread.Sleep(50);
                }
            }
            Button_Click_1(null, null);
        }

        private void FolderTreeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FolderTreeView.SelectedIndex != -1)
            {
                if (!isFileDownload)
                {
                    if ((FolderTreeView.SelectedItem as TemplateFileElement).isFile == true)
                    {
                        //using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                        //{
                        //    if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        //    {
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        DownloadPath = dialog.SelectedPath;
                        //    }
                        //}
                    }
                    else
                    {
                        SendMessage("/FolderInfo " + currentFTPpath + (FolderTreeView.SelectedItem as TemplateFileElement).path);
                        return;
                    }

                    if(!DownMan.AddNewItem(currentFTPpath + (FolderTreeView.SelectedItem as TemplateFileElement).path))
                        LoadingAnim.Set(() => this.ShowMessageAsync("Error", "The file is already in DownloadManager"));
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                string BackPath = currentFTPpath.Remove(currentFTPpath.LastIndexOf("\\"));
                BackPath = BackPath.Remove(BackPath.LastIndexOf("\\"));
                SendMessage("/FolderInfo " + BackPath);
            }
            catch (Exception ex)
            {
                Title = ex.Message;
                SendMessage("/HardDriveInfo");
            }
        }

        public static ImageSource ToImageSource(Icon icon)
        {
            if (icon == null)
            {
                return new BitmapImage(new Uri("folder.png", UriKind.RelativeOrAbsolute));
            }
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }

        public static Icon BytesToIcon(byte[] bytes)
        {
            if (bytes == null)
            {
                return null;
            }
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                return new Icon(ms);
            }
        }

        class TemplateFileElement
        {
            public string path { get; set; }
            public ImageSource icon { get; set; }
            public bool isFile { get; set; }
        }        

        private void CancelDown_Click(object sender, RoutedEventArgs e)
        {
            Disconnect();            
            Task.Run(() => Connection(host, port.ToString()));
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {

            DownMan.items[DMList.SelectedIndex].DeleteDownload();
        }

        private void Image_MouseUp_1(object sender, MouseButtonEventArgs e) // Пауза, возобновление загрузки
        {
            DownMan.items[DMList.SelectedIndex].StartStopDownload();

        }

        private void Image_MouseUp_2(object sender, MouseButtonEventArgs e)
        {
            DownMan.items[DMList.SelectedIndex].OpenFolderWithFile();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if(result == System.Windows.Forms.DialogResult.OK)
                {
                    SaveFolder.Text = dialog.SelectedPath;
                    DownMan.DownloadFolder = dialog.SelectedPath;
                }
            }
        }

        public void MahAppsMessage(string message, string header)
        {
            LoadingAnim.Set(() => this.ShowMessageAsync(message, header));
        }
    }
}