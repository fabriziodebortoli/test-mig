using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
    public class FileSystemXSDResolver : IXSDResolver
    {
        public IBusinessObjectXSDInfo Resolve(IBusinessObjectInfo bOInfo)
        {
            string pathFileXSD = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), @"SynchroConnector\SynchroProviders\CRMInfinity\Files");

            IBusinessObjectXSDInfo result = Factory.Instance.CreateBusinessObjectXSDInfo();
            try
            {
                string xsdFileCompleteName = string.Empty;
                if(!Path.HasExtension(bOInfo.Name))
                    xsdFileCompleteName = Path.Combine(pathFileXSD, String.Concat(bOInfo.Name, "_", bOInfo.Name, "_in.xsd"));
                else
                    xsdFileCompleteName = Path.Combine(pathFileXSD, bOInfo.Name);

                if (File.Exists(xsdFileCompleteName))
                {
                    result.XsdContenct = File.ReadAllText(xsdFileCompleteName);
                    result.Found = true;
                }
                else
                    result.Found = false;
            }
            catch (Exception e)
            {
                
                Console.WriteLine("ERROR XSD Resolve"+e);
                result.Found = false;
            }

            return result;
        }
    }
}
