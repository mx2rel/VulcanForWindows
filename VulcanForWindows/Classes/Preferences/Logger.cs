using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VulcanTest.Vulcan;

namespace VulcanForWindows.Preferences
{
    public static class Logger
    {
        public static string LogsPath
        {
            get
            {
               var p = Path.Combine(PreferencesManager.folder, "Logs");
                if(!Directory.Exists(p)) Directory.CreateDirectory(p);
                return p;
            }
        }

        public static string FilePath
        {
            get
            {
                if (string.IsNullOrEmpty(_filePath))
                {
                    var d = GetDir(Path.Combine(LogsPath, AppWide.AppVersion));
                    d = GetDir(Path.Combine(d, DateTime.Now.ToString("dd/MM/yy")));
                    var fp = Path.Combine(d, DateTime.Now.ToString("HH mm ss")) + ".txt";
                    if (!File.Exists(fp)) { var f = File.Create(fp); f.Close(); }
                    _filePath = fp;
                }

                return _filePath;
            }
        }

        static string GetDir(string dir)
        {
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return dir;
        }

        static string _filePath;


        public static void Log(string message)
        {

            using (StreamWriter sw = new StreamWriter(FilePath, true))
            {
                sw.WriteLine(DateTime.Now.ToString("HH:mm:ss: ") + message);
            }
        }
    }
}
