using System;
using Microarea.TaskBuilderNet.Core.WebServicesWrapper;

namespace RESTGate.OrganizerCore
{
    //================================================================================
    public class TokenValidator
    {
        LoginManager loginManager;
        bool isTokenValid;

        public bool IsTokenValid { get { return this.isTokenValid; } set { this.isTokenValid = value; } }

        public string CompanyName { 
            get 
            {
                if (!this.isTokenValid)
                {
                    return String.Empty;
                }

                return loginManager.CompanyName;
            } 
         }

        public string ConnectionString
        {
            get
            {
                if (!this.isTokenValid)
                {
                    return String.Empty;
                }

                return loginManager.NonProviderCompanyConnectionString;
            }
        }

        public TokenValidator(string token)
        {
            this.loginManager = new LoginManager();
            this.isTokenValid = this.loginManager.GetLoginInformation(token);
        }
    }
}