﻿using Microarea.AdminServer.Model.Interfaces;
using System;

namespace Microarea.AdminServer.Model
{
    //================================================================================
    public class UserAccount : IUserAccount
    {
        string id;
        string name;
        bool isAdmin;
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
        }
        public bool IsAdmin
        {
            get { return this.IsAdmin; }
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
