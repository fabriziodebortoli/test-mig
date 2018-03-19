using Microarea.Common.NameSolver;
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

        private bool m_bEnablePerformanceCheck;

        private int m_nWebServiceDriverPort;
        private string m_sWebServiceDriverService = string.Empty;
        private string m_sWebServiceDriverNamespace = string.Empty;

        private string m_sFSServerName = string.Empty;
        private string m_sFSInstanceName = string.Empty;
        private string m_sFSStandardPath = string.Empty;
        private string m_sFSCustomPath = string.Empty;

        private string m_strStandardConnectionString;
        private string customConnectionString;
        private string companyName;
        private string userName;

        public string GetCompanyName() { return companyName; }
        public string GetUserName() { return userName; }
        public string GetFSServerName() { return m_sFSServerName; }
        public string GetFSInstanceName() { return m_sFSInstanceName; }
        public string GetFSStandardPath() { return m_sFSStandardPath; }
        public string GetFSCustomPath() { return m_sFSCustomPath; }

        public string GetStandardConnectionString() { return m_strStandardConnectionString; }
        public string GetCustomConnectionString() { return customConnectionString; }

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
        public void SetDriver(string aDriverType)
        {
            int value = Convert.ToInt32(aDriverType);
            m_Driver = ((DriverType)value);
        }

        //----------------------------------------------------------------------------
        public void SetDriver(DriverType aDriverType)
        {
            int value = Convert.ToInt32(aDriverType);
            m_Driver = ((DriverType)value);
        }
        //----------------------------------------------------------------------------
        public string GetFileName()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileSystemManager.config");
        }

        //----------------------------------------------------------------------------
        public bool LoadFile()
        {
            if (!File.Exists(GetFileName()))
                return false;

            using (XmlReader reader = XmlReader.Create(GetFileName()))
            {
                reader.Read();
                reader.ReadStartElement(FileSystemManagerStrings.szXmlRoot);//< FileSystemManager >
                reader.ReadToFollowing(FileSystemManagerStrings.szXmlDriverTag);//<Driver value="0" autodetect="true"/>
                SetDriver(reader.GetAttribute(FileSystemManagerStrings.szXmlValue));

                if (GetDriver() == DriverType.Database)
                {
                    reader.ReadToFollowing(FileSystemManagerStrings.szXmlDBDriverTag);//  <DatabaseDriver 
                    m_strStandardConnectionString = reader.GetAttribute(FileSystemManagerStrings.szXmlStandardConnectionString);//standardconnectionstring
                    customConnectionString = reader.GetAttribute(FileSystemManagerStrings.testCustomConnectionString);
                }
                reader.ReadToFollowing(FileSystemManagerStrings.szXmCompanyNameTag);
                companyName = reader.GetAttribute(FileSystemManagerStrings.szXmlName);
                reader.ReadToFollowing(FileSystemManagerStrings.szXmCompanyNameTag);
                userName = reader.GetAttribute(FileSystemManagerStrings.szXmlUserNameTag);
            }

            return true;
        }
    }
}
