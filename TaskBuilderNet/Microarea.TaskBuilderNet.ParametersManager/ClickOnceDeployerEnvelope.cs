using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Microarea.TaskBuilderNet.ParametersManager
{
    //=========================================================================
    [Serializable]
    public class ClickOnceDeployerEnvelope : ISerializable
    {
        const string fileTag = "file";
        const string errorTag = "error";
        const string errorMessageTag = "errorMessage";
        const string productNameTag = "productName";
        const string versionTag = "version";

        public byte[] File { get; set; }
        public bool Error { get; set; }
        public string ErrorMessage { get; set; }
        public string ProductName { get; set; }
        public string Version { get; set; }

        //---------------------------------------------------------------------
        public ClickOnceDeployerEnvelope()
        {
            File = new byte[] { };
            ErrorMessage = ProductName = Version = String.Empty;
        }

        //---------------------------------------------------------------------
        protected ClickOnceDeployerEnvelope(
            SerializationInfo info,
            StreamingContext context
            )
        {
            try  { File = (byte[])info.GetValue(fileTag, typeof(byte[]));  }
            catch {}

            if (File == null)
            {
                File = new byte[] { };
            }

            try  { Error = (bool)info.GetValue(errorTag, typeof(bool)); }
            catch { }

            try { ErrorMessage = (string)info.GetValue(errorMessageTag, typeof(string)); }
            catch { }

            if (ErrorMessage == null)
            {
                ErrorMessage = String.Empty;
            }

            try { ProductName = (string)info.GetValue(productNameTag, typeof(string)); }
            catch { } 
            
            if (ProductName == null)
            {
                ProductName = String.Empty;
            }

            try { Version = (string)info.GetValue(versionTag, typeof(string)); }
            catch { } 
            
            if (Version == null)
            {
                Version = String.Empty;
            }
        }

        //---------------------------------------------------------------------
        [
        SecurityPermission(
            SecurityAction.Demand,
            SerializationFormatter = true
            )
        ]
        public void GetObjectData(
            SerializationInfo info,
            StreamingContext context
            )
        {
            if (File != null)
            {
                info.AddValue(fileTag, File);
            }

            info.AddValue(errorTag, Error);

            if (ErrorMessage != null)
            {
                info.AddValue(errorMessageTag, ErrorMessage);
            }
            if (ProductName != null)
            {
                info.AddValue(productNameTag, ProductName);
            }
            if (Version != null)
            {
                info.AddValue(versionTag, Version);
            }
        }
    }
}
