using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
    //=========================================================================
    internal class SessionFactory : ISessionFactory
    {
        private Configuration configuration;
        private string tbSoapServerUrl;
        private string authenticationServiceUrl;
        private string domain;
        private string username;
        private string password;
        private bool integratedWinAuth;
        private string installation;
        private string companyName;
        private string producerKey;

        private LoginManager loginManager;
        private int connectionTimeout;

        private object lockTicket = new object();

        //---------------------------------------------------------------------
        /// <exception cref="System.ArgumentNullException">if Configuration is null.</exception>
        /// <exception cref="SessionFactoryException">if Configuration contains bad parameters.</exception>
        internal SessionFactory(Configuration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            this.configuration = configuration;

            tbSoapServerUrl = configuration.GetPropertyValue(Configuration.SoapServerUrl);
            if (tbSoapServerUrl == null)
                throw new SessionFactoryException("Cannot find soap server url");

            if (tbSoapServerUrl.Trim().Length == 0)
                throw new SessionFactoryException("Empty soap server url found");

            try
            {
                new Uri(tbSoapServerUrl);
            }
            catch (Exception exc)
            {
                throw new SessionFactoryException("Invalid soap server url found", exc);
            }

            authenticationServiceUrl = configuration.GetPropertyValue(Configuration.AuthenticationServiceUrl);
            if (authenticationServiceUrl == null)
                throw new SessionFactoryException("Cannot find authentication service url");

            if (authenticationServiceUrl.Trim().Length == 0)
                throw new SessionFactoryException("Empty authentication service url found");

            try
            {
                new Uri(authenticationServiceUrl);
            }
            catch (Exception exc)
            {
                throw new SessionFactoryException("Invalid authentication service url found", exc);
            }

            //Domain can be null or empty
            domain = configuration.GetPropertyValue(Configuration.Domain);
            if (domain == null)
                domain = string.Empty;

            username = configuration.GetPropertyValue(Configuration.Username);
            if (username == null)
                throw new SessionFactoryException("Cannot find username");

            if (username.Trim().Length == 0)
                throw new SessionFactoryException("Empty username found");

            password = configuration.GetPropertyValue(Configuration.Password);
            if (password == null)
                password = string.Empty;

            installation = configuration.GetPropertyValue(Configuration.Installation);
            if (installation == null)
                throw new SessionFactoryException("Cannot find installation");

            if (installation.Trim().Length == 0)
                throw new SessionFactoryException("Empty installation found");

            companyName = configuration.GetPropertyValue(Configuration.CompanyName);
            if (companyName == null)
                throw new SessionFactoryException("Cannot find company name");

            if (companyName.Trim().Length == 0)
                throw new SessionFactoryException("Empty company name found");

            producerKey = configuration.GetPropertyValue(Configuration.ProducerKey);
            if (producerKey == null)
                throw new SessionFactoryException("Cannot find producer key");

            if (producerKey.Trim().Length == 0)
                throw new SessionFactoryException("Empty producer key found");

            try
            {
                connectionTimeout = Int32.Parse(configuration.GetPropertyValue(Configuration.ConnectionTimeout));
                if (connectionTimeout <= 0)
                    connectionTimeout = Int32.MaxValue;
            }
            catch (Exception)
            {
                connectionTimeout = Int32.MaxValue;
            }

            try
            {
                integratedWinAuth = Boolean.Parse(configuration.GetPropertyValue(Configuration.IntegratedWindowsAuthentication));
            }
            catch (Exception exc)
            {
                throw new SessionFactoryException("Invalid value for 'IntegratedWindowsAuthentication'", exc);
            }

            loginManager = new LoginManager(authenticationServiceUrl, connectionTimeout);
            Login();
        }

        #region ISessionFactory Members

        //---------------------------------------------------------------------
        public ISession OpenSession()
        {
            lock (lockTicket)
            {
                if (!loginManager.IsValidToken(loginManager.AuthenticationToken))
                    Login();

                return new Session(
                    tbSoapServerUrl,
                    connectionTimeout,
                    loginManager.AuthenticationToken
                    );
            }
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            lock (lockTicket)
            {
                Dispose();
            }
        }

        #endregion

        #region IDisposable Members

        //---------------------------------------------------------------------
        public void Dispose()
        {
            lock (lockTicket)
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        //---------------------------------------------------------------------
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                LogOff();
            }
        }

        //-------------------------------------------------------------------------
        private void Login()
        {
            string fullUsername = username;
            int loginResult = 0;
            if (domain != null && domain.Trim().Length > 0)
                fullUsername = String.Format("{0}\\{1}", domain, fullUsername);

            try
            {
                loginResult = loginManager.Login(
                    fullUsername,
                    password,
                    true,
                    companyName,
                    producerKey,
                    true
                    );
            }
            catch (Exception exc)
            {
                throw new AuthenticationException("Exception during authentication", exc);
            }

            switch (loginResult)
            {
                case (int)LoginReturnCodes.NoError:
                    break;

                case (int)LoginReturnCodes.AlreadyLoggedOnDifferentCompanyError:
                    throw new AuthenticationException("User already logged on different company");

                case (int)LoginReturnCodes.NoCalAvailableError:
                    throw new AuthenticationException("No cals available");

                case (int)LoginReturnCodes.CalManagementError:
                    throw new AuthenticationException("CALs management failed");

                case (int)LoginReturnCodes.NoLicenseError:
                    throw new AuthenticationException("No licenses available");

                case (int)LoginReturnCodes.UserAssignmentToArticleFailure:
                    throw new AuthenticationException("User to article assignement failed");

                case (int)LoginReturnCodes.ProcessNotAuthenticatedError:
                    throw new AuthenticationException("Process not authenticated");

                case (int)LoginReturnCodes.InvalidUserError:
                    throw new AuthenticationException("Invalid user");

                case (int)LoginReturnCodes.InvalidProcessError:
                    throw new AuthenticationException("Invalid process name specified");

                case (int)LoginReturnCodes.LockedDatabaseError:
                    throw new AuthenticationException("Databse locked");

                case (int)LoginReturnCodes.UserMustChangePasswordError:
                    throw new AuthenticationException("User must change password");

                case (int)LoginReturnCodes.InvalidCompanyError:
                    throw new AuthenticationException("Invalid company name");

                case (int)LoginReturnCodes.ProviderError:
                    throw new AuthenticationException("Provider error");

                case (int)LoginReturnCodes.ConnectionParamsError:
                    throw new AuthenticationException("Invalid connection parameters");

                case (int)LoginReturnCodes.CompanyDatabaseNotPresent:
                    throw new AuthenticationException("Company database not present");

                case (int)LoginReturnCodes.CompanyDatabaseTablesNotPresent:
                    throw new AuthenticationException("Company database tables not present");

                case (int)LoginReturnCodes.InvalidDatabaseForActivation:
                    throw new AuthenticationException("Invalid database for current activation");

                case (int)LoginReturnCodes.WebApplicationAccessDenied:
                    throw new AuthenticationException("Web application access denied");

                case (int)LoginReturnCodes.GDIApplicationAccessDenied:
                    throw new AuthenticationException("GDI application access denied");

                case (int)LoginReturnCodes.LoginLocked:
                    throw new AuthenticationException("Login locked");

                case (int)LoginReturnCodes.InvalidDatabaseError:
                    throw new AuthenticationException("Invalid database");

                case (int)LoginReturnCodes.NoAdmittedCompany:
                    throw new AuthenticationException("No admitted company");

                case (int)LoginReturnCodes.NoOfficeLicenseError:
                    throw new AuthenticationException("No office licences available");

                case (int)LoginReturnCodes.TooManyAssignedCAL:
                    throw new AuthenticationException("Too many assigned cals");

                case (int)LoginReturnCodes.ImagoCompanyNotCorresponding:
                    throw new AuthenticationException("Company not corresponding to the company previously specified in configuration wizard.");

                default:
                    throw new AuthenticationException("Unknown error, cannot log in");
            }
        }

        //-------------------------------------------------------------------------
        private void LogOff()
        {
            loginManager.LogOff();
        }
    }

    //=========================================================================
    [Serializable]
    public class SessionFactoryException : Exception
    {
        //---------------------------------------------------------------------
        public SessionFactoryException()
            : this(string.Empty, null)
        { }

        //---------------------------------------------------------------------
        public SessionFactoryException(string message)
            : this(message, null)
        { }

        //---------------------------------------------------------------------
        public SessionFactoryException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed for xml serialization.
        //---------------------------------------------------------------------
        protected SessionFactoryException(
            SerializationInfo info,
            StreamingContext context
            )
            : base(info, context)
        { }

        //---------------------------------------------------------------------
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context
            )
        { }
    }

    //=========================================================================
    [Serializable]
    public class AuthenticationException : Exception
    {
        //---------------------------------------------------------------------
        public AuthenticationException()
            : this(string.Empty, null)
        { }

        //---------------------------------------------------------------------
        public AuthenticationException(string message)
            : this(message, null)
        { }

        //---------------------------------------------------------------------
        public AuthenticationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed for xml serialization.
        //---------------------------------------------------------------------
        protected AuthenticationException(
            SerializationInfo info,
            StreamingContext context
            )
            : base(info, context)
        { }

        //---------------------------------------------------------------------
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info,
            StreamingContext context
            )
        { }
    }
}
