using CefSharp;
using Ionic.Zip;
using System;
using System.IO;

namespace Microarea.Snap.Installer.UI
{
    public class ZipResourcesService : IResourcesService
    {
        string zipPath;

        public ZipResourcesService(string zipPath)
        {
            this.zipPath = zipPath;
        }

        public void InitResourceHandlers(IWebBrowser browser)
        {
            var factory = browser.ResourceHandlerFactory as DefaultResourceHandlerFactory;

            if (factory == null)
            {
                return;
            }

            string url = null;
            using (var webZip = new ZipFile(zipPath))
            {
                foreach (var entry in webZip.Entries)
                {
                    url = entry.FileName;
                    if (url.StartsWith("src", StringComparison.OrdinalIgnoreCase))
                    {
                        url = entry.FileName.Substring(4);
                    }

                    var stream = ReadFromStream(entry);
                    factory.RegisterHandler("http://local/" + url, ResourceHandler.FromStream(stream, ResourceHandler.GetMimeType(Path.GetExtension(entry.FileName))));
                }
            }
        }

        private static MemoryStream ReadFromStream(ZipEntry entry)
        {
            var inputStream = entry.OpenReader();
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, buffer.Length);
            return new MemoryStream(buffer);
        }
    }
}