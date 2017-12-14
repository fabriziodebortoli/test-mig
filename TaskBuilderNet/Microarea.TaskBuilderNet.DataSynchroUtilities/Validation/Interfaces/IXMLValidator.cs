using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces
{
    public interface IValidateInfo
    {
        string XMLString { get; set; }
        List<IBusinessObjectXSDInfo> XSDInfo { get; set; }
    }
    
    public class ValidateXMLInfo : IValidateInfo
    {
        public ValidateXMLInfo() 
        {
            XSDInfo = new List<IBusinessObjectXSDInfo>();
        }

        public string XMLString { get; set; }
        public List<IBusinessObjectXSDInfo> XSDInfo { get; set; }
    }
    
    public class ValidatorResult
    {

        public bool IsOK { get; set; }

        public IList<IAFError> IAFErrorList = new List<IAFError>();
        
        public int CountIAFErrorList()
        {
            return IAFErrorList.Count();
        }

        public void AddXSDErrorElement(string tag, string value, string msgError, int noRow)
        {
            IAFError element = new IAFError(msgError, false, true);
            element.SetXSDDataError(tag, value, msgError, noRow);
            IAFErrorList.Add(element);
        }
    }

    public class IAFError
    {
        public IAFError()
        {
            MessageError = string.Empty;
            IsFKError = false;
            IsXSDError = false;
            IAF_Tag = string.Empty;
            IAF_Value = string.Empty;
            NoRowInXML = 0;
        }
        public IAFError(string msgError, bool bIsFKError, bool bIsXSDError)
        {
            MessageError    = msgError;
            IsFKError       = bIsFKError;
            IsXSDError      = bIsXSDError;
            IAF_Tag         = string.Empty;
            IAF_Value       = string.Empty;
            NoRowInXML      = 0;
        }

        public void SetXSDDataError(string tag, string value, string msgError, int noRow)
        {
            IAF_Tag         = tag;
            IAF_Value       = value;
            MessageError    = msgError;
            NoRowInXML      = noRow;
        }

        public string IAF_Tag { get; set; }
        public string IAF_Value { get; set;  }
        public string MessageError { get; set; }
        public int NoRowInXML { get; set; }
        public bool IsXSDError { get; set; }
        public bool IsFKError { get; set; }
    }

    //=========================================================================
    public interface IXMLValidator
    {
        ValidatorResult Validate(ValidateXMLInfo ValidateInfo, string actionName);
    }
    
}
