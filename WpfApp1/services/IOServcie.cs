using Search.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search.services
{
    public class IOServcie
    {
        public void OpenFile(IndexedFile file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = file.FullPath,
                UseShellExecute = true
            };
            Process.Start(process);
        }
        public void OpenFileFolder(IndexedFile file)
        {
            var process = new ProcessStartInfo()
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{file.FullPath}\"",
                UseShellExecute = true
            };
            Process.Start(process);
        }
    }
}
