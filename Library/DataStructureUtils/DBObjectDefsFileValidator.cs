using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace Microarea.Library.DataStructureUtils
{
    public class DBObjectDefsFileValidator
    {
        public static Stream DBObjectsDefinitionsXSD { get { return Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Library.DataStructureUtils.XSD.DBObjectsDef.xsd"); } }

        //---------------------------------------------------------------------------
        public static bool IsValiDBObjectsDefFile(string path)
        {
            if (String.IsNullOrEmpty(path) || !File.Exists(path))
                return false;

            try
            {
                // Uso un apposito XSD per verificare la correttezza sintattica del file
                System.Xml.Schema.XmlSchema dbObjectsDefSchema = LoadDBObjectsDefSchema();

                if (dbObjectsDefSchema != null)
                {
                    XmlReaderSettings readerSettings = new XmlReaderSettings();
                    readerSettings.Schemas.Add(dbObjectsDefSchema);
                    readerSettings.ValidationType = ValidationType.Schema;
                    readerSettings.IgnoreComments = true;
                    readerSettings.CloseInput = true;
                    readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
                    readerSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                    readerSettings.ValidationEventHandler += new ValidationEventHandler(DBObjectsDefValidatingReader_Validation);

                    XmlReader xmlValidatingReader = XmlReader.Create(new XmlTextReader(path), readerSettings);
                    while (xmlValidatingReader.Read())
                    {
                    }
                    xmlValidatingReader.Close();

                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);

                return false;
            }
        }

        //---------------------------------------------------------------------------
        private static System.Xml.Schema.XmlSchema LoadDBObjectsDefSchema()
        {

            try
            {
                Stream schemaStream = DBObjectsDefinitionsXSD;

                if (schemaStream == null)
                    return null;

                return XmlSchema.Read(schemaStream, new ValidationEventHandler(DBObjectsDefSchema_Validation));
            }
            catch (Exception exception)
            {
                // There is a load or parse error in the XML file. In this case, the document remains empty.
                System.Diagnostics.Debug.Fail("Exception raised in DBObjectDefsFileValidator.LoadDBObjectsDefFileXSD: " + exception.Message);

                return null;
            }
        }

        //---------------------------------------------------------------------------
        private static void DBObjectsDefSchema_Validation(object sender, ValidationEventArgs args)
        {
            if (args == null)
                return;

            string errorMessage = String.Empty;
            if (args.Exception != null)
                errorMessage = String.Format(Strings.DBObjectsDefSchemaValidationErrorMessage, args.Exception.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString());
            else
                errorMessage = args.Message;

            throw new Exception(errorMessage);
        }

        //---------------------------------------------------------------------------
        private static void DBObjectsDefValidatingReader_Validation(object sender, ValidationEventArgs args)
        {
            if (args == null)
                return;

            XmlReader currentXmlReader = sender as XmlReader;
            if (currentXmlReader == null)
                return;

            string errorMessage = String.Empty;
            if (args.Exception != null)
                errorMessage = String.Format(Strings.DBObjectsDefValidationErrorMessage, currentXmlReader.BaseURI, args.Exception.Message, args.Exception.LineNumber.ToString(), args.Exception.LinePosition.ToString());
            else
                errorMessage = args.Message;

            throw new Exception(errorMessage);
        }
    }
}
