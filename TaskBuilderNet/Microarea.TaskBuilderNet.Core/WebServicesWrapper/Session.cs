using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Xml;
using System.Xml.Serialization;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
    //=========================================================================
    internal class Session : ISession
    {
        private TbServices tbServices;
        private string authToken;

        //---------------------------------------------------------------------
        internal Session(string tbSoapServerUrl, int connectionTimeout, string authToken)
        {
            if (tbSoapServerUrl == null)
                throw new ArgumentNullException("Null soap server url");

            if (tbSoapServerUrl.Trim().Length == 0)
                throw new ArgumentNullException("Empty soap server url");

            this.tbServices = new TbServices(tbSoapServerUrl, connectionTimeout);

            if (authToken == null)
                throw new ArgumentNullException("Null authentication token");

            if (authToken.Trim().Length == 0)
                throw new ArgumentException("Empty authentication token");

            this.authToken = authToken;
        }

        #region ISession Members

        //---------------------------------------------------------------------
        public T[] Load<T>(IParameterBuilder parameterBuilder)
        {
            if (parameterBuilder == null)
                throw new ArgumentNullException("loadParameter");

            Object loadParameters = parameterBuilder.CreateParameters();

            XmlSerializer ser = new XmlSerializer(loadParameters.GetType());
            Stream backendStream = new MemoryStream();
            ser.Serialize(backendStream, loadParameters);
            backendStream.Seek(0, SeekOrigin.Begin);

            XmlDocument xmlLoadParameter = new XmlDocument();
            xmlLoadParameter.Load(backendStream);

            string[] docs = null;
            try
            {
                docs = tbServices.GetData(authToken, xmlLoadParameter, DateTime.Today, true);
            }
            catch (Exception exc)
            {
                throw new LoadException("Error while getting data from server", exc);
            }

            if (docs == null || docs.Length == 0)
                throw new NoDataFoundException();

            ser = new XmlSerializer(typeof(T));

            T[] returnValues = new T[docs.Length];

            try
            {
                for (int i = 0; i < docs.Length; i++)
                    returnValues[i] = (T)ser.Deserialize(new System.IO.StringReader(docs[i]));
            }
            catch (Exception exc)
            {
                throw new DataException("Error received from server while deserializing data", exc);
            }

            return returnValues;
        }

        //---------------------------------------------------------------------
        public void Save<T>(T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            XmlSerializer ser = new XmlSerializer(obj.GetType());
            Stream backendStream = new MemoryStream();
            ser.Serialize(backendStream, obj);
            backendStream.Seek(0, SeekOrigin.Begin);

            XmlDocument resultDoc = null;
            bool ok = false;

            try
            {
                ok = tbServices.SetData(
                        authToken,
                        new StreamReader(backendStream).ReadToEnd(),
                        DateTime.Today,
                        0,
                        out  resultDoc,
                        true
                        );
            }
            catch (Exception exc)
            {
                throw new DataException("Error while setting data to server", exc);
            }

            if (!ok)
                throw new DataException(resultDoc.OuterXml);
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            Dispose();
        }

        #endregion

        #region IDisposable Members

        //---------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        //---------------------------------------------------------------------
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.tbServices != null)
                {
                    this.tbServices.CloseTB(authToken);
                    this.tbServices = null;
                }
            }
        }
    }

    //=========================================================================
    [Serializable]
    public class LoadException : Exception
    {
        //---------------------------------------------------------------------
        public LoadException()
            : this(string.Empty, null)
        { }

        //---------------------------------------------------------------------
        public LoadException(string message)
            : this(message, null)
        { }

        //---------------------------------------------------------------------
        public LoadException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed for xml serialization.
        //---------------------------------------------------------------------
        protected LoadException(
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
    public class NoDataFoundException : Exception
    {
        //---------------------------------------------------------------------
        public NoDataFoundException()
            : this(string.Empty, null)
        { }

        //---------------------------------------------------------------------
        public NoDataFoundException(string message)
            : this(message, null)
        { }

        //---------------------------------------------------------------------
        public NoDataFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed for xml serialization.
        //---------------------------------------------------------------------
        protected NoDataFoundException(
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
    public class DataException : Exception
    {
        //---------------------------------------------------------------------
        public DataException()
            : this(string.Empty, null)
        { }

        //---------------------------------------------------------------------
        public DataException(string message)
            : this(message, null)
        { }

        //---------------------------------------------------------------------
        public DataException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed for xml serialization.
        //---------------------------------------------------------------------
        protected DataException(
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
