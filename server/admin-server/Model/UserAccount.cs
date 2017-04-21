using Microarea.AdminServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microarea.AdminServer.Model
{
    public class UserAccount : IUserAccount
    {
        string id;
        string name;
        string password;
        bool userCannotChangePassword;
        bool userMustChangePassword;
        bool isProvisioningAdmin;
        string applicationLanguage;
        string preferredLanguage;
        bool disabled;
        bool passwordNeverExpires;
        DateTime expireDatePassword;
        bool isWindowsAuthentication;

        public string ApplicationLanguage
        {
            get { return this.applicationLanguage; }
            set { this.applicationLanguage = value; }
        }

        public bool Disabled
        {
            get { return this.disabled; }
        }

        public DateTime ExpireDatePassword
        {
            get { return this.expireDatePassword; }
            set { this.expireDatePassword = value; }
        }

        public string Id
        {
            get { return this.id; }
        }

        public bool IsProvisioningAdmin
        {
            get { return this.isProvisioningAdmin; }
            set { this.isProvisioningAdmin = value; }
        }

        public bool IsWindowsAuthentication
        {
            get { return this.isWindowsAuthentication; }
        }

        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        public string Password
        {
            get { return this.password; }
        }

        public bool PasswordNeverExpires
        {
            get { return this.passwordNeverExpires; }
        }

        public string PreferredLanguage
        {
            get { return this.preferredLanguage; }
            set { this.preferredLanguage = value; }
        }

        public bool UserCannotChangePassword
        {
            get { return this.userCannotChangePassword; }
        }

        public bool UserMustChangePassword
        {
            get { return this.userMustChangePassword; }
        }
    }
}
