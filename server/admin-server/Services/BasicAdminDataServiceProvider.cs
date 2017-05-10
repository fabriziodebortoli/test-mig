using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microarea.AdminServer.Model.Interfaces;
using Microarea.AdminServer.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microarea.AdminServer.Services.Interfaces;

namespace Microarea.AdminServer.Services
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

        public IAccount ReadLogin(string userName, string password)
        {
            // read the storage, find a login and return a IUserAccount object

            this.fileInfo = fileProvider.GetFileInfo("accounts.json");
            //StreamReader sr = this.fileInfo.CreateReadStream();

            IAccount userAccount = new Account();
            return userAccount;
        }
    }
}
