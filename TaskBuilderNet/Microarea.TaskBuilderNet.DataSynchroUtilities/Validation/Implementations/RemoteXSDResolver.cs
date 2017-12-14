using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.IO.Compression;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.Xml.Schema;
using System.Xml.XPath;
using Microarea.TaskBuilderNet.Interfaces;
namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
    public class RemoteXSDResolver : IXSDResolver
    {
       
        public Uri InfinityCRMUri { get; protected set; }

        public RemoteXSDResolver()
        {
            
        }

       static byte[] Decompress(byte[] data)
        {
            MemoryStream expms = new MemoryStream(data);
            MemoryStream expdata = new MemoryStream();
            byte[] expbuf = new byte[114];
            expms.ReadByte();
            expms.ReadByte();
            
            using (DeflateStream expstr = new DeflateStream(expms, CompressionMode.Decompress))
            {
                for (; ; )
                {
                    int len = expstr.Read(expbuf, 0, expbuf.Length); // Error Message came here
                    if (len == 0) break;
                    expdata.Write(expbuf, 0, len);
                }
            }
            byte[] outbuf = expdata.ToArray();

            return outbuf;
        }


       public IBusinessObjectXSDInfo Resolve(IBusinessObjectInfo bOInfo)
        {
            IBusinessObjectXSDInfo result = Factory.Instance.CreateBusinessObjectXSDInfo();
            try
            {
                IRemoteBusinessObjectInfo remoteBOInfo = bOInfo as IRemoteBusinessObjectInfo;

                byte[] rawResponse = null;
                byte[] response = null;

                IValidationProxy validationProxy = remoteBOInfo.ProviderImplementation as IValidationProxy;
                if (validationProxy == null)
                {
                    result.XsdContenct = string.Format("Provider {0} does not implement validation.", 
                                                       remoteBOInfo.ProviderImplementation.ProviderName);
                    result.Found = false;
                    return result;
                }

                rawResponse = validationProxy.GetXsd(remoteBOInfo.SpecificFileRequest, new object[] { "MAGO", bOInfo.Name });
                
                response = Decompress(rawResponse);

                if (response.Length == 0)
                {
                    result.Found = false;
                    return result;
                }
                
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(response));
                
                XmlNode toBeRemoved = doc.SelectSingleNode("//@*[local-name()='schemaLocation']");
                if (toBeRemoved != null)
                    toBeRemoved.RemoveAll();

                result.Found = true;
                result.XsdContenct = doc.InnerXml;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR XSD Resolve" + e);
                result.Found = false;
            }

            return result;
        }

      
    }
}
