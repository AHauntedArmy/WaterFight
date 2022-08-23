using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace WaterFight.Tools
{
    internal static class FileTools
    {
        public static readonly string AssemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string ReadAllText(string fileName)
        {
            string filelocation = Path.Combine(AssemblyLocation, fileName);
            string fileDirectory = Path.GetDirectoryName(filelocation);
            
            if (!Directory.Exists(fileDirectory) || !File.Exists(filelocation)) {
                return null;
            }

            return File.ReadAllText(filelocation);
        }

        public static bool WriteAllText(string fileName, string text)
        {
            string fileLocation = Path.Combine(AssemblyLocation, fileName);

            if (!VerifyOrCreateFile(fileLocation)) {
                return false;
            }

            try {
                File.WriteAllText(fileLocation, text);

            } catch {
                return false;
            }

            return true;
        }

        public static bool VerifyOrCreateDirectory(string path)
        {
            string directory = Path.GetDirectoryName(path);
            if (directory == null)
                return false;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return Directory.Exists(directory);
        }

        public static bool VerifyOrCreateFile(string path)
        {
            if (!VerifyOrCreateDirectory(path))
                return false;

            FileStream fs = null;
            if (!File.Exists(path))
                fs = File.Create(path);

            if (fs == null)
                return File.Exists(path);

            fs.Dispose();
            fs.Close();

            return true;
        }
    }
}
