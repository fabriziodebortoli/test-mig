using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    internal class FileSystemService : IFileSystemService
    {
        IPathProviderService pathProviderService;
        ISettings settings;

        public FileSystemService(IPathProviderService pathProviderService, ISettings settings)
        {
            this.pathProviderService = pathProviderService;
            this.settings = settings;
        }
        public string CalculateProductInstallationPath()
        {
            var pathToCalculateOn = pathProviderService.RetrievePathToCalculateOn();

            if (pathToCalculateOn.Trim().Length == 0)
            {
                throw new ArgumentException("Empty path received, I cannot calcuate installation path");
            }

            if (!System.IO.Path.IsPathRooted(pathToCalculateOn))
            {
                throw new ArgumentException("Not rooted path received, I cannot calcuate installation path");
            }

            var tokens = pathToCalculateOn.Split(System.IO.Path.DirectorySeparatorChar);

            if (tokens == null || tokens.Length == 0)
            {
                return null;
            }
            var tokensStack = new Stack<string>();
            var drive = tokens[0];
            for (int i = 1; i < tokens.Length; i++)
            {
                tokensStack.Push(tokens[i]);
            }

            tokensStack.Pop();//skip di SnapInstaller.exe
            if (tokensStack.Pop() != "SnapInstaller")
            {
                return null;
            }
            if (tokensStack.Pop() != "Apps")
            {
                return null;
            }

            var path = tokensStack.Pop();
            while (tokensStack.Count > 0)
            {
                path = System.IO.Path.Combine(tokensStack.Pop(), path);
            }
            
            return string.Concat(drive, System.IO.Path.DirectorySeparatorChar, path);
        }

        public IFile InstallationVersionFile
        {
            get
            {
                return new File(System.IO.Path.Combine(this.settings.ProductInstanceFolder, "Standard", "Installation.ver"));
            }
        }
    }
}
