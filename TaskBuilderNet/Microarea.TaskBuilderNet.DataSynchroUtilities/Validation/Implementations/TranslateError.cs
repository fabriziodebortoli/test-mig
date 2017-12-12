using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using Microarea.TaskBuilderNet.Core.NameSolver;
using System.IO;
using System.Xml;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
    class TranslateError : ITranslate
    {
        public TranslateResult Translate(string action, string field)
        {
            TranslateResult result = new TranslateResult();

            try
            {
                
                result.IsOK = false;
                result.ResultMessage = "";
                string strActionErrorPath = @"SynchroConnector\SynchroProviders\CRMInfinity\ActionErrors";
                string pathFileError = Path.Combine(PathFinder.BasePathFinderInstance.GetStandardApplicationPath("ERP"), strActionErrorPath, action + ".xml");

                if (!File.Exists(pathFileError))
                {
                    result.IsOK = false;
                    result.ResultMessage = action + ".xml" + " : file not found in path " + strActionErrorPath;
                    return result;
                }

                XmlDocument xmlError = new XmlDocument();
                xmlError.Load(pathFileError);

                XmlNodeList xnList = xmlError.SelectNodes("/" + action + "/" + field);

                foreach (XmlNode xn in xnList)
                {
                    result.ResultMessage = xn["DESCR_ITA"].InnerText;
                }

                if (!string.IsNullOrEmpty(result.ResultMessage))
                    result.IsOK = true;
                else
                {
                    result.IsOK = false;
                    result.ResultMessage = field + " : field not found in file " + action + ".xml";
                }

                return result;
            }
            catch
            {
                result.IsOK = false;
                result.ResultMessage = "Generic Error";
                return result;
            }
           

        }
    }
}
