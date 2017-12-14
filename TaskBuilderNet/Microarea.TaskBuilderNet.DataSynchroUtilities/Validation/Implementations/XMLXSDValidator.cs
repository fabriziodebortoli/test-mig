using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Interfaces;
using Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations;
using System;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Collections.Generic;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using System.Diagnostics;

namespace Microarea.TaskBuilderNet.DataSynchroUtilities.Validation.Implementations
{
        
    public class ValidationHandler
    {
        public ValidatorResult IAFResult = new ValidatorResult();

        private TranslateResult resultErrorTranslate = new TranslateResult();

        public string ActionName { get; set; }

        public int NoCurrentRow { get; set; }
        public void HandleValidationError(object sender, ValidationEventArgs ve)
        {
            if (ve.Severity == XmlSeverityType.Error || ve.Severity == XmlSeverityType.Warning)
            {
                XmlReader xmlReader = (sender as XmlReader);
                ITranslate TranslateError = Factory.Instance.CreateTranslateError();
                resultErrorTranslate = TranslateError.Translate(ActionName, xmlReader.Name);
                string message = resultErrorTranslate.ResultMessage;
                IAFResult.AddXSDErrorElement(xmlReader.Name, xmlReader.Value, message, NoCurrentRow);
            }
        }
        
    }
    
    //=========================================================================
    public class XMLXSDValidator : IXMLValidator
    {
        public ValidationHandler ValidationHandler { get; protected set; }

        public XMLXSDValidator()
        {
            ValidationHandler = new ValidationHandler();
        }

        public ValidatorResult Validate(ValidateXMLInfo CreateXMLInfo, string actionName)
        {            
            bool bValidationOk = true;
            try
            {
                XmlReaderSettings XSDValidator = new XmlReaderSettings();
                XSDValidator.ValidationEventHandler += new ValidationEventHandler(ValidationHandler.HandleValidationError);
                ValidationHandler.ActionName = actionName;
                XmlSchemaSet schemas = new XmlSchemaSet();
                XElement xsdSchema = null;
                
                foreach (var item in CreateXMLInfo.XSDInfo)
                {
                    xsdSchema = XElement.Parse(item.XsdContenct);
                    schemas.Add(null, xsdSchema.CreateReader());
                }

                XSDValidator.Schemas = schemas;
                XSDValidator.ValidationType = ValidationType.Schema;

                XmlReader Reader;
                using (StringReader sr = new StringReader(CreateXMLInfo.XMLString))
                {
                    using (Reader = XmlReader.Create(sr, XSDValidator))
                    {
                        while(!Reader.EOF)
                        {
                            try
                            {
                                ValidationHandler.NoCurrentRow++;
                                Reader.Read();
                            }
                            catch
                            {
                                throw;
                            }
                            finally
                            {
                                if (!(Reader.NodeType == XmlNodeType.Element && Reader.Name.StartsWith("Add_")))
                                    ValidationHandler.NoCurrentRow--;
                            }
                        }
                        
                    }
                }
            }
            catch (XmlSchemaValidationException e)
            {
                bValidationOk = false;
                Debug.WriteLine(e.Message);
            }

            bValidationOk = ValidationHandler.IAFResult.CountIAFErrorList() > 0 ? false : true;
            ValidationHandler.IAFResult.IsOK = bValidationOk;
            return ValidationHandler.IAFResult;
        }
    }
}
