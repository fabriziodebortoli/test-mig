using CefSharp;
using System.IO;
using System.Linq;

namespace Microarea.Snap.Installer.UI
{
    public class FileSystemResourcesService : IResourcesService
    {
        string rootFolder;

        public FileSystemResourcesService(string rootFolder)
        {
            this.rootFolder = rootFolder;
        }
        public void InitResourceHandlers(IWebBrowser browser)
        {
            var factory = browser.ResourceHandlerFactory as DefaultResourceHandlerFactory;

            if (factory == null)
            {
                return;
            }

            var rootDirInfo = new DirectoryInfo(rootFolder);
            RegisterHandlers(rootDirInfo, rootDirInfo, factory);

            var srcDirInfo = new DirectoryInfo(Path.Combine(rootDirInfo.FullName, "src"));
            RegisterHandlers(srcDirInfo, srcDirInfo, factory);
        }

        private void RegisterHandlers(DirectoryInfo rootDirInfo, DirectoryInfo dirInfo, DefaultResourceHandlerFactory factory)
        {
            string url = null;
            foreach (var fileInfo in dirInfo.GetFiles().Where(f => !f.Name.StartsWith(".")))
            {
                url = CalculateUrl(rootDirInfo, fileInfo);
                factory.RegisterHandler("http://local" + url, ResourceHandler.FromFilePath(fileInfo.FullName, ResourceHandler.GetMimeType(fileInfo.Extension)));
            }

            var subDirInfos = dirInfo.GetDirectories().Where(s => !s.Name.StartsWith("."));
            foreach (var subDirInfo in subDirInfos)
            {
                RegisterHandlers(rootDirInfo, subDirInfo, factory);
            }
        }

        private static string CalculateUrl(DirectoryInfo rootDirInfo, FileInfo fileInfo)
        {
            return fileInfo
                .FullName
                .Replace(rootDirInfo.FullName, string.Empty)
                .Replace("\\", "/");
        }
    }
}