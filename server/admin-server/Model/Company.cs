using Microarea.AdminServer.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace provisioning_server.Model
{
    public class Company : ICompany
    {
        string id;
        string instanceId;
        string companyName;
        string description;
        ICompanyDatabaseInfo companyDatabaseInfo;
        ICompanyDatabaseInfo dMSDatabaseInfo;
        bool disabled;
        int databaseCulture;
        bool unicode;
        string provider;

        public ICompanyDatabaseInfo CompanyDatabaseInfo { get { return this.companyDatabaseInfo; } }
        public int DatabaseCulture { get { return this.databaseCulture; } }
        public string Description { get { return this.description; } }
        public bool Disabled { get { return this.disabled; }}
        public ICompanyDatabaseInfo DMSDatabaseInfo { get { return this.dMSDatabaseInfo; } }
        public string Id { get { return this.id; } }
        public string InstanceId { get { return this.instanceId; } }
        public string Provider { get { return this.provider; } }
        public bool Unicode { get { return this.unicode; } }
        public string CompanyName { get { return this.companyName; } }
    }
}
