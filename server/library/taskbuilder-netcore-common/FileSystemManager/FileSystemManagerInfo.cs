using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microarea.Common.FileSystemManager
{
    public enum DriverType { FileSystem, WebService, Database };
    //=============================================================================
    public class FileSystemManagerInfo
    {
        private DriverType m_Driver;
        private bool m_bAutoDetectDriver;
        private bool m_bEnableCaching;
        private bool m_bEnablePerformanceCheck;

        private int m_nWebServiceDriverPort;
        private string m_sWebServiceDriverService;
        private string m_sWebServiceDriverNamespace;

        private string m_sFSServerName;
        private string m_sFSInstanceName;
        private string m_sFSStandardPath;
        private string m_sFSCustomPath;

        private string m_strStandardConnectionString;

        public string GetFSServerName() { return m_sFSServerName; }
        public string GetFSInstanceName() { return m_sFSInstanceName; }
        public string GetFSStandardPath() { return m_sFSStandardPath; }
        public string GetFSCustomPath() { return m_sFSCustomPath; }

        public string GetStandardConnectionString() { return m_strStandardConnectionString; }
        //----------------------------------------------------------------------------
        public FileSystemManagerInfo()
        { 
            m_Driver = DriverType.FileSystem;
            m_bAutoDetectDriver = true;

            m_bEnablePerformanceCheck = false;
            m_nWebServiceDriverPort = 80;
        }
        //----------------------------------------------------------------------------
        public DriverType GetDriver()
        {
            return m_Driver;
        }
        //----------------------------------------------------------------------------
        public bool IsCachingEnabled() 
        {
	        return m_bEnableCaching;
        }
        //----------------------------------------------------------------------------
        public bool IsPerformanceCheckEnabled()
        {
            return m_bEnablePerformanceCheck;
        }
        //----------------------------------------------------------------------------
        public int GetWebServiceDriverPort()
        {
            return m_nWebServiceDriverPort;
        }
        //----------------------------------------------------------------------------
        public string GetWebServiceDriverService()
        {
            return m_sWebServiceDriverService;
        }
        //----------------------------------------------------------------------------
        public string GetWebServiceDriverNamespace()
        {
            return m_sWebServiceDriverNamespace;
        }
        //----------------------------------------------------------------------------
        public bool IsAutoDetectDriver()
        {
            return m_bAutoDetectDriver;
        }
        //----------------------------------------------------------------------------
        public void SetDriver(DriverType aDriverType)
        {
            m_Driver = aDriverType;
        }
        //----------------------------------------------------------------------------
        public string GetFileName()
        {
            return @"C:\TBexplorer anna\FileSystemManager.config";// TODO LARA AfxGetPathFinder()->GetTBDllPath() + "\\" + FileSystemManagerStrings.szXmlFileName;
        }

        //----------------------------------------------------------------------------
        public bool LoadFile()
        {
            if (!File.Exists(GetFileName()))
                return false;
            // LARA
            //https://msdn.microsoft.com/it-it/library/t9bfea29.aspx
            //// FileSystemManager parsing with Sax
            //CFileSystemManagerContent aFileContent(this);
            //CXMLSaxReader aReader;

            //aReader.AttachContent(&aFileContent);
            //return aReader.ReadFile(GetFileName());
            using (XmlReader reader = XmlReader.Create(GetFileName()))
            {

                // Parse the XML document.  ReadString is used to 
                // read the text content of the elements.
                reader.Read();
                reader.ReadStartElement(FileSystemManagerStrings.szXmlRoot);
                reader.ReadStartElement(FileSystemManagerStrings.szXmlDriverTag);
                reader.ReadToFollowing(FileSystemManagerStrings.szXmlDBDriverTag);
                m_strStandardConnectionString = reader.GetAttribute(FileSystemManagerStrings.szXmlStandardConnectionString);
            }

            return true;
        }

        // FileSystemManager unparsing with Dom
        //----------------------------------------------------------------------------
        public bool SaveFile()
        {
            // LARA esempio msdn https://msdn.microsoft.com/it-it/library/cc189056(v=vs.95).aspx
            //    StringBuilder output = new StringBuilder();

            //    String xmlString =
            //            @"<?xml version='1.0'?>
            //<!-- This is a sample XML document -->
            //<Items>
            //  <Item>test with a child element <more/> stuff</Item>
            //</Items>";
            //    // Create an XmlReader
            //    using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
            //    {
            //        XmlWriterSettings ws = new XmlWriterSettings();
            //        ws.Indent = true;
            //        using (XmlWriter writer = XmlWriter.Create(output, ws))
            //        {

            //            // Parse the file and display each of the nodes.
            //            while (reader.Read())
            //            {
            //                switch (reader.NodeType)
            //                {
            //                    case XmlNodeType.Element:
            //                        writer.WriteStartElement(reader.Name);
            //                        break;
            //                    case XmlNodeType.Text:
            //                        writer.WriteString(reader.Value);
            //                        break;
            //                    case XmlNodeType.XmlDeclaration:
            //                    case XmlNodeType.ProcessingInstruction:
            //                        writer.WriteProcessingInstruction(reader.Name, reader.Value);
            //                        break;
            //                    case XmlNodeType.Comment:
            //                        writer.WriteComment(reader.Value);
            //                        break;
            //                    case XmlNodeType.EndElement:
            //                        writer.WriteFullEndElement();
            //                        break;
            //                }
            //            }

            //        }
            //    }
            //    OutputTextBlock.Text = output.ToString();

            //CXMLDocumentObject aDoc;

            //CXMLNode* pRoot = aDoc.CreateRoot(szXmlRoot);
            //if (!pRoot)
            //{
            //    ASSERT(FALSE);
            //    TRACE("Cannot create root tag of the FileSystemManager.config file");
            //    return FALSE;
            //}

            //// Driver
            //CXMLNode* pNewNode = pRoot->CreateNewChild(szXmlDriverTag);

            //string sTemp;
            //switch (m_Driver)
            //{
            //    case FileSystem: pNewNode->SetAttribute(szXmlValue, (LPCTSTR)szXmlIntZeroValue); break;
            //    case WebService: pNewNode->SetAttribute(szXmlValue, (LPCTSTR)szXmlIntOneValue); break;
            //    case Database: pNewNode->SetAttribute(szXmlValue, (LPCTSTR)szXmlIntTwoValue); break;
            //}

            //pNewNode->SetAttribute(szXmlAutodetect, (LPCTSTR)m_bAutoDetectDriver ? szXmlTrueValue : szXmlFalseValue);

            //// Caching
            //pNewNode = pRoot->CreateNewChild(szXmlCachingTag);
            //pNewNode->SetAttribute(szXmlEnabled, (LPCTSTR)m_bEnableCaching ? szXmlTrueValue : szXmlFalseValue);

            //// Performance Check
            //pNewNode = pRoot->CreateNewChild(szXmlPerformanceCheckTag);
            //pNewNode->SetAttribute(szXmlEnabled, (LPCTSTR)m_bEnablePerformanceCheck ? szXmlTrueValue : szXmlFalseValue);

            //const rsize_t nLen = 5;
            //TCHAR szBuffer[nLen];
            //_itot_s(m_nWebServiceDriverPort, szBuffer, nLen, 10);

            //// Web Service Driver
            //pNewNode = pRoot->CreateNewChild(szXmlWebServiceDriverTag);
            //pNewNode->SetAttribute(szXmlPort, (LPCTSTR)CString(szBuffer));
            //pNewNode->SetAttribute(szXmlService, (LPCTSTR)m_sWebServiceDriverService);
            //pNewNode->SetAttribute(szXmlNamespace, (LPCTSTR)m_sWebServiceDriverNamespace);

            //return aDoc.SaveXMLFile(GetFileName());
            return true;
        }
    }

}
