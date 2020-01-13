using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using Emigration;
using System.Runtime.Serialization.Formatters.Binary;

namespace Updater
{
    class Program
    {
        static TcpClient _Client = new TcpClient();
        static NetworkStream stream;

        static void Main()
        {
            try
            {
                _Client.Connect("77.93.61.5", 21025);
                stream = _Client.GetStream();
                Console.WriteLine("Connection status: Online.");

                string message = Environment.MachineName;
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);

                int bytes = 0;
                data = new byte[1024];
                bytes = stream.Read(data, 0, data.Length);
                message = Encoding.UTF8.GetString(data, 0, bytes);
                Console.WriteLine(message);

                data = Encoding.UTF8.GetBytes("/GetVersion");
                stream.Write(data, 0, data.Length);

                bytes = 0;
                data = new byte[1024];
                bytes = stream.Read(data, 0, data.Length);
                string clientVersion = Encoding.UTF8.GetString(data, 0, bytes);
                string currentVersion;
                using (FileStream fs = new FileStream("version.cfg", FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        currentVersion = sr.ReadToEnd();
                    }
                }
                currentVersion = currentVersion.Replace("version: ", "");
                if (clientVersion == currentVersion)
                {
                    string path = System.Environment.CurrentDirectory + "\\App\\WpfConnectClient.exe";
                    System.Diagnostics.Process.Start(path);                    
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo("App");
                    di.GetFiles().ToList().ForEach(a => a.Delete());

                    //Шлём запрос на получение фалов программы
                    for(int i = 0; i < 9; i++)
                    {
                        message = $"/GetApp {i}";
                        data = Encoding.UTF8.GetBytes(message);
                        stream.Write(data, 0, data.Length);

                        data = new byte[500000];
                        bytes = stream.Read(data, 0, data.Length);
                        EmigrationObject eo = ByteArrayToObject(data) as EmigrationObject;
                        if (eo.type == "file")
                        {
                            EmigrationFileInfo efi = (EmigrationFileInfo)eo.message;
                            DownloadFile(efi);
                        }
                    }
                    Console.WriteLine("Update download complete...");
                    using (FileStream fs = new FileStream("version.cfg", FileMode.Create))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.Write("version: " + clientVersion);
                        }
                    }
                    string path = System.Environment.CurrentDirectory + "\\App\\WpfConnectClient.exe";
                    System.Diagnostics.Process.Start(path);
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("I can`t connect....");
            }
        }


        static void DownloadFile(EmigrationFileInfo efi)
        {
            try
            {
                using (FileStream fs = new FileStream(System.Environment.CurrentDirectory + "\\App" + "\\" + efi.FileName, FileMode.Create))
                {
                    long currentBytes = 0;
                    byte[] data;
                    while (currentBytes < efi.Size)
                    {
                        data = new byte[1];
                        stream.Read(data, 0, data.Length);
                        fs.WriteByte(data[0]);
                        currentBytes++;
                    }
                }
            }
            catch (Exception ex) { }
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
