using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emigration;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace WpfClientServerTest
{
    public partial class MainWindow : Window
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);
        static string currentVersion = "1.0.0.3";
        string ServerFolderPath = "Server\\";
        TcpListener tcpListener;

        List<Client> Clients = new List<Client>();

        public MainWindow()
        {
            InitializeComponent();
            tcpListener = new TcpListener(IPAddress.Any, 21025);
            tcpListener.Start();
            ConsoleLog.Text += $"Сервер запущен успешно, порт: {21025}\n";
            Task.Run(() => SrverListen());
            Task.Run(() => ReceiveMessage());
        }

        void SrverListen()
        {

                while (true)
                {
                    try
                    {
                        TcpClient client = tcpListener.AcceptTcpClient();
                        NetworkStream ClientStream = client.GetStream();

                        int bytes = 0;
                        byte[] get = new byte[2048];
                        bytes = ClientStream.Read(get, 0, get.Length);
                        string ClientName = Encoding.UTF8.GetString(get, 0, bytes);                    
                        ConsoleLog.Set(() =>
                        {
                            ConsoleLog.Text += $"{ClientName} подключился\n";
                        });
                        Clients.Add(new Client() { name = ClientName, stream = ClientStream, id = Clients.Count(), client = client });

                        string message = $"Подключение произошло успешно...";
                        byte[] data = Encoding.UTF8.GetBytes(message);
                        ClientStream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                {
                    //тута проблемма
                }
            }

        }

        //Читаем все потоки
        void ReceiveMessage()
        {
            while (true)
            {
                for (int i = 0; i < Clients.Count; i++)
                {
                    if (!Clients[i].IsBusy)
                    {
                        if (!IsConnected(Clients[i].client.Client))
                        {
                            AddConsoleMessage($"{Clients[i].name} отключился...");
                            Clients.RemoveAt(i);
                            break;
                        }
                        if (Clients[i].stream.DataAvailable)
                        {
                            string message = "";
                            int bytes = 0;
                            byte[] data = new byte[256];
                            bytes = Clients[i].stream.Read(data, 0, data.Length);
                            message = Encoding.UTF8.GetString(data, 0, bytes);

                            useCommand(message, i);
                        }
                    }
                }
                //Thread.Sleep(50);
            }
        }

        void useCommand(string message, int useID)
        {
            string command = message.Split()[0];
            switch (command)
            {
                case "/GetFile":
                    DropFile(message, useID);
                    break;
                case "/SayHello":
                    EmigrationObject obj = new EmigrationObject() { type = "string", message = "Hello user!!!" };
                    byte[] bytes = ObjectToByteArray(obj);
                    Clients[useID].stream.Write(bytes, 0, bytes.Length);
                    AddConsoleMessage($"{Clients[useID].name} use say hello");
                    break;
                case "/FolderInfo":
                    if (message == "/FolderInfo")
                    {
                        List<EmigrationPathInfo> allfilesDirectory = new List<EmigrationPathInfo>();
                        DirectoryInfo di = new DirectoryInfo(ServerFolderPath);

                        allfilesDirectory.Add(new EmigrationPathInfo() { Name = System.IO.Path.GetFullPath(ServerFolderPath) });
                        di.GetDirectories().ToList().ForEach(a => allfilesDirectory.Add(new EmigrationPathInfo() { Name = a.Name, isFile = false }));
                        di.GetFiles().ToList().ForEach(a => allfilesDirectory.Add(new EmigrationPathInfo()
                        {
                            Name = a.Name,
                            isFile = true,
                            ImagePath = IconToBytes(System.Drawing.Icon.ExtractAssociatedIcon(a.FullName))
                        }));

                        EmigrationObject listobjj = new EmigrationObject() { type = "List<EmigrationPathInfo>", message = allfilesDirectory };
                        bytes = ObjectToByteArray(listobjj);
                        Clients[useID].stream.Write(bytes, 0, bytes.Length);
                        AddConsoleMessage($"{Clients[useID].name} use fileinfo");
                    }
                    else
                    {
                        string path = message.Replace("/FolderInfo ", "");
                        if (File.Exists(path))
                        {
                            DropFile(path, useID);
                        }
                        else if (Directory.Exists(path + "\\") || DriveInfo.GetDrives().Where(a => a.Name == path + "\\").Count() != 0)
                        {
                            path = path + "\\";
                            List<EmigrationPathInfo> allfilesDirectory = new List<EmigrationPathInfo>();
                            try
                            {
                                DirectoryInfo di = new DirectoryInfo(path);

                                allfilesDirectory.Add(new EmigrationPathInfo() { Name = System.IO.Path.GetFullPath(message.Replace("/FolderInfo ", "") + @"\") });

                                di.GetDirectories().ToList().ForEach(a => allfilesDirectory.Add(new EmigrationPathInfo() { Name = a.Name, isFile = false }));
                                di.GetFiles().ToList().ForEach(a => allfilesDirectory.Add(new EmigrationPathInfo()
                                {
                                    isFile = true,
                                    Name = a.Name,
                                    ImagePath = IconToBytes(System.Drawing.Icon.ExtractAssociatedIcon(a.FullName))
                                }));

                                EmigrationObject throwlist = new EmigrationObject() { type = "List<EmigrationPathInfo>", message = allfilesDirectory };
                                bytes = ObjectToByteArray(throwlist);
                                Clients[useID].stream.Write(bytes, 0, bytes.Length);
                                //AddConsoleMessage($"{Clients[useID].name} use fileinfo");
                            }
                            catch (Exception ex)
                            {
                                
                                return;
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    break;
                case "/GetVersion":
                    bytes = Encoding.UTF8.GetBytes(currentVersion);
                    Clients[useID].stream.Write(bytes, 0, bytes.Length);
                    AddConsoleMessage($"{Clients[useID].name} use get version");
                    break;
                case "/GetApp":
                    int num = int.Parse(message.Split()[1]);
                    DirectoryInfo dio = new DirectoryInfo(@"C:\Users\SelectiveHoe\Documents\visual studio 2015\Projects\WpfClientServerTest\WpfConnectClient\bin\Debug");
                    DropFile(dio.GetFiles()[num].FullName, useID);
                    break;
                case "/HardDriveInfo":
                    var bitmap = new Bitmap("Drive-icon.png");
                    var iconHandle = bitmap.GetHicon();
                    var icon = System.Drawing.Icon.FromHandle(iconHandle);
                    List<EmigrationPathInfo> alldrivers = new List<EmigrationPathInfo>();
                    alldrivers.Add(new EmigrationPathInfo() { Name = "" });
                    DriveInfo.GetDrives().ToList().ForEach(a => {
                        alldrivers.Add(new EmigrationPathInfo() { Name = a.Name, ImagePath = IconToBytes(icon), isFile = false});
                    });
                    EmigrationObject listobj = new EmigrationObject() { type = "List<EmigrationPathInfo>", message = alldrivers };
                    bytes = ObjectToByteArray(listobj);
                    Clients[useID].stream.Write(bytes, 0, bytes.Length);
                    break;
                case "/DropFile":
                    Clients[useID].IsBusy = true;
                    Task.Run(() => TakeFile(useID));
                    break;
                case "/FileExist":
                    string pathF = message.Replace("/FileExist", "");
                    if (File.Exists(pathF))
                    {
                        bytes = Encoding.UTF8.GetBytes("true");
                        Clients[useID].stream.Write(bytes, 0, bytes.Length);
                    }
                    else
                    {
                        bytes = Encoding.UTF8.GetBytes("false");
                        Clients[useID].stream.Write(bytes, 0, bytes.Length);
                    }
                    break;
                default:
                    AddConsoleMessage(message);
                    break;
            }
        }

        //Отправка обычного сообщения
        private void ConsoleMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                EmigrationObject obj = new EmigrationObject() { type = "string", message = ConsoleMessage.Text };
                byte[] data = ObjectToByteArray(obj);

                for (int i = 0; i < Clients.Count; i++)
                {
                    try
                    {
                        Clients[i].stream.Write(data, 0, data.Length);
                    }
                    catch (Exception ex)
                    {
                        Title = ex.Message;
                        AddConsoleMessage($"{Clients[i].name} отключен");
                        Clients.RemoveAt(i);
                    }
                    ConsoleMessage.Text = "";
                }

            }
        }

        void TakeFile(int userID)
        {            
            NetworkStream currStrm = Clients[userID].stream;
            byte[] buf = new byte[11000];
            currStrm.Read(buf, 0, buf.Length);
            EmigrationFileInfo efi = (EmigrationFileInfo)ByteArrayToObject(buf);
            string path = efi.FileName;
            int cntFiles = (int)efi.Size;
            for(int i = 0; i < cntFiles; i++)
            {
                buf = new byte[11000];
                currStrm.Read(buf, 0, buf.Length);
                efi = (EmigrationFileInfo)ByteArrayToObject(buf);
                using (FileStream fs = new FileStream(path + "\\" + efi.FileName, FileMode.Create))
                {
                    long currentBytes = 0;
                    byte[] data;
                    while (currentBytes < efi.Size)
                    {
                        data = new byte[1];
                        currStrm.Read(data, 0, data.Length);
                        fs.WriteByte(data[0]);
                        currentBytes++;
                    }
                }
            }
            //DownloadComplete
            EmigrationObject em = new EmigrationObject() { type = "DownloadComplete" };
            buf = ObjectToByteArray(em);
            Clients[userID].stream.Write(buf, 0, buf.Length);
            Clients[userID].IsBusy = false;
        }

        void DropFile(string path, int useID)
        {
            try
            {
                string filename = path.Replace("/GetFile ", "");
                FileInfo fi = new FileInfo(filename);
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                { }
                EmigrationFileInfo efi = new EmigrationFileInfo() { FileName = fi.Name, Size = fi.Length };
                EmigrationObject getfile = new EmigrationObject() { type = "file", message = efi };
                byte[] infofilebytes = ObjectToByteArray(getfile);
                Clients[useID].stream.Write(infofilebytes, 0, infofilebytes.Length);
                AddConsoleMessage($"{Clients[useID].name} begin to download {fi.Name}");
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    byte[] PieceOfFile = new byte[fs.Length];
                    fs.Read(PieceOfFile, 0, PieceOfFile.Length);
                    Clients[useID].stream.Write(PieceOfFile, 0, PieceOfFile.Length);
                }
            }
            catch(Exception ex)
            {
                
                //Если не можем открыть файл
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tcpListener.Stop();
        }

        private void AddConsoleMessage(string mess)
        {
            ConsoleLog.Set(() =>
            {
                ConsoleLog.Text += $"{mess}\n";
            });
        }


        public static bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        private Object ByteArrayToObject(byte[] arrBytes)
        {
            MemoryStream memStream = new MemoryStream();
            BinaryFormatter binForm = new BinaryFormatter();
            memStream.Write(arrBytes, 0, arrBytes.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            Object obj = (Object)binForm.Deserialize(memStream);

            return obj;
        }

        public static byte[] IconToBytes(Icon icon)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                icon.Save(ms);
                return ms.ToArray();
            }
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

        public static ImageSource ToImageSource(System.Drawing.Icon icon)
        {
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
    }

    [Serializable]
    static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
}
