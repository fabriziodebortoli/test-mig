using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;

using Microarea.Library.TBWizardProjects;
using Microarea.Library.SqlScriptUtility;

namespace Microarea.Library.DataStructureUtils
{
    public class DBObjectDefsHelper
    {
        XmlDocument dbObjectDefsDocument = null;
       
        //---------------------------------------------------------------------------
        public DBObjectDefsHelper(string aFileName)
        {
            if (!String.IsNullOrEmpty(aFileName) && File.Exists(aFileName))
            {
                dbObjectDefsDocument = new XmlDocument();
                dbObjectDefsDocument.Load(aFileName);
            }
        }

        //---------------------------------------------------------------------------
        public IList<WizardTableInfo> ParseFileTables()
        {
            if (dbObjectDefsDocument == null)
                return null;
            try
            {
                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseTablesInfoNode(dbObjectDefsDocument, false);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public static IList<WizardTableInfo> ParseFileTables(string aFileName)
        {
            if (String.IsNullOrEmpty(aFileName) || !File.Exists(aFileName))
                return null;

            XmlDocument dbObjectDefsDocument = new XmlDocument();

            try
            {
                dbObjectDefsDocument.Load(aFileName);

                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseTablesInfoNode(dbObjectDefsDocument, false);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public Microarea.Library.SqlScriptUtility.SqlViewList ParseViewsInfoNode()
        {
            if (dbObjectDefsDocument == null)
                return null;
            try
            {
                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseViewsInfoNode(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public static Microarea.Library.SqlScriptUtility.SqlViewList ParseViewsInfoNode(string aFileName)
        {
            if (String.IsNullOrEmpty(aFileName) || !File.Exists(aFileName))
                return null;

            XmlDocument dbObjectDefsDocument = new XmlDocument();

            try
            {
                dbObjectDefsDocument.Load(aFileName);

                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseViewsInfoNode(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public Microarea.Library.SqlScriptUtility.SqlProcedureList ParseProceduresInfoNode()
        {
            if (dbObjectDefsDocument == null)
                return null;
            try
            {
                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseProceduresInfoNode(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public static Microarea.Library.SqlScriptUtility.SqlProcedureList ParseProceduresInfoNode(string aFileName)
        {
            if (String.IsNullOrEmpty(aFileName) || !File.Exists(aFileName))
                return null;

            XmlDocument dbObjectDefsDocument = new XmlDocument();

            try
            {
                dbObjectDefsDocument.Load(aFileName);

                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseProceduresInfoNode(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }

        //---------------------------------------------------------------------------
        public IList<WizardExtraAddedColumnsInfo> ParseExtraAddedColumnsInfo()
        {
            if (dbObjectDefsDocument == null)
                return null;
            try
            {
                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseExtraAddedColumnsInfo(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }
        
        //---------------------------------------------------------------------------
        public static IList<WizardExtraAddedColumnsInfo> ParseExtraAddedColumnsInfo(string aFileName)
        {
            if (String.IsNullOrEmpty(aFileName) || !File.Exists(aFileName))
                return null;

            XmlDocument dbObjectDefsDocument = new XmlDocument();

            try
            {
                dbObjectDefsDocument.Load(aFileName);

                DBObjectsProjectParser parser = new DBObjectsProjectParser();
                if (parser == null)
                    return null;

                return parser.ParseExtraAddedColumnsInfo(dbObjectDefsDocument);
            }
            catch
            {
                return null;
            }
        }
    }
}
