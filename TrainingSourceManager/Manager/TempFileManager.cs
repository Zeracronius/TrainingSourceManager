using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrainingSourceManager.Manager
{
    public static class TempFileManager
    {
        private const string _directory = "TrainingSourceManager";

        /// <summary>
        /// Returns a path to the directory where the program stores temp files.
        /// </summary>
        /// <returns></returns>
        public static string GetTempDirectoryPath()
        {
            string directoryPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _directory);
            if (System.IO.Directory.Exists(directoryPath) == false)
                System.IO.Directory.CreateDirectory(directoryPath);

            return System.IO.Path.Combine(System.IO.Path.GetTempPath(), _directory);
        }


        /// <summary>
        /// Attempts to delete all files in the temporary directory.
        /// </summary>
        public static void Clean()
        {
            string directoryPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), _directory);
            if (System.IO.Directory.Exists(directoryPath) == false)
                return;

            string[] tempFiles = System.IO.Directory.GetFiles(directoryPath);
            foreach (string file in tempFiles)
            {
                try
                {
                    System.IO.File.Delete(file);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
