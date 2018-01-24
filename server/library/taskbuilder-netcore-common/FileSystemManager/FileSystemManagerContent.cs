using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Microarea.Common.FileSystemManager
{
     public class FileSystemManagerContent : XmlReader
    {
        private FileSystemManagerInfo m_pConfigInfo;
        //------------------------------------------------------------------------------
        public FileSystemManagerContent(FileSystemManagerInfo pConfigInfo)
        {
            m_pConfigInfo = pConfigInfo;
        }

        //------------------------------------------------------------------------------
        public string OnGetRootTag()
        {
	        return FileSystemManagerStrings.szXmlRoot;
        }

        //------------------------------------------------------------------------------
        public int ParseDriver(String sUri,  CXMLSaxContentAttributes& arAttributes)
    {
        if (m_pConfigInfo == null)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;

        CString sTmp = arAttributes.GetAttributeByName(szXmlValue);

        sTmp.Trim();
        sTmp = sTmp.MakeLower();
        if (sTmp.CompareNoCase(szXmlIntOneValue) == 0)
            m_pConfigInfo->m_Driver = CFileSystemManagerInfo::WebService;
        else if (sTmp.CompareNoCase(szXmlIntTwoValue) == 0)
            m_pConfigInfo->m_Driver = CFileSystemManagerInfo::Database;

        sTmp = arAttributes.GetAttributeByName(szXmlAutodetect);
        m_pConfigInfo->m_bAutoDetectDriver = sTmp.CompareNoCase(szXmlIntOneValue) == 0 ||
                                                sTmp.CompareNoCase(szXmlTrueValue) == 0;

        return CXMLSaxContent::OK;
    }

    //------------------------------------------------------------------------------
    public int ParseCaching(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
    {
        if (!m_pConfigInfo)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;

        CString sTmp = arAttributes.GetAttributeByName(szXmlEnabled);

        m_pConfigInfo->m_bEnableCaching = sTmp.CompareNoCase(szXmlIntOneValue) == 0 ||
                                            sTmp.CompareNoCase(szXmlTrueValue) == 0;

        return CXMLSaxContent::OK;
    }

    //------------------------------------------------------------------------------
    public int ParsePerformanceCheck(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
    {
        if (!m_pConfigInfo)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;

        CString sTmp = arAttributes.GetAttributeByName(szXmlEnabled);

        m_pConfigInfo->m_bEnablePerformanceCheck = sTmp.CompareNoCase(szXmlIntOneValue) == 0 ||
                                                    sTmp.CompareNoCase(szXmlTrueValue) == 0;

        return CXMLSaxContent::OK;
    }

    //------------------------------------------------------------------------------
    public int ParseWebServiceDriver(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
    {
        if (!m_pConfigInfo)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;

        CString sTmp = arAttributes.GetAttributeByName(szXmlPort);

        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_nWebServiceDriverPort = _tstoi(sTmp);

        sTmp = arAttributes.GetAttributeByName(szXmlService);
        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sWebServiceDriverService = sTmp.Trim();

        sTmp = arAttributes.GetAttributeByName(szXmlNamespace);
        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sWebServiceDriverNamespace = sTmp.Trim();

        return CXMLSaxContent::OK;
    }


    //------------------------------------------------------------------------------
    public int ParseFileSystemDriver(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
    {
        if (!m_pConfigInfo)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;

        CString sTmp = arAttributes.GetAttributeByName(szXmlInstance);

        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sFSInstanceName = sTmp;

        sTmp = arAttributes.GetAttributeByName(szXmlServer);
        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sFSServerName = sTmp.Trim();

        sTmp = arAttributes.GetAttributeByName(szXmlStandardPath);
        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sFSStandardPath = sTmp.Trim();

        sTmp = arAttributes.GetAttributeByName(szXmlCustomPath);
        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_sFSCustomPath = sTmp.Trim();

        return CXMLSaxContent::OK;
    }


    //----------------------------------------------------------------------------
    public int ParserDatabaseDriverKey(const CString& sUri, const CXMLSaxContentAttributes& arAttributes)
    {
        if (!m_pConfigInfo)
            return CXMLSaxContent::ABORT;

        if (!arAttributes.GetSize())
            return CXMLSaxContent::OK;
        CString sTmp = arAttributes.GetAttributeByName(szXmlStandardConnectionString);

        if (!sTmp.IsEmpty())
            m_pConfigInfo->m_strStandardConnectionString = sTmp;

        return CXMLSaxContent::OK;
    }
}
}
