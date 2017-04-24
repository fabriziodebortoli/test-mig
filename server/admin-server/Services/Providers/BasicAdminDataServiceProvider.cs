using Microarea.AdminServer.Services.Providers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.AdminServer.Interfaces;
using Microarea.AdminServer.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Microarea.AdminServer.Services.Providers
{
    public class BasicAdminDataServiceProvider : IAdminDataServiceProvider
    {
        IFileProvider fileProvider;
        IFileInfo fileInfo;

        public BasicAdminDataServiceProvider()
        {
            this.fileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Data"));
        }

        public IUserAccount ReadLogin(string userName, string password)
        {
            // read the storage, find a login and return a IUserAccount object

            this.fileInfo = fileProvider.GetFileInfo("accounts.json");
            //StreamReader sr = this.fileInfo.CreateReadStream();

            IUserAccount userAccount = new UserAccount();
            userAccount.Name = "Fra";
            return userAccount;
        }
    }
}
