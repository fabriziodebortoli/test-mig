using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Services
{
    class SnapInstallerVersionService : ISnapInstallerVersionService
    {
        static Assembly execAsm = Assembly.GetExecutingAssembly();
        static string product = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(execAsm, typeof(AssemblyProductAttribute), false)).Product;
        static string version = execAsm.GetName().Version.ToString();
        static string copyright = ((AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(execAsm, typeof(AssemblyCopyrightAttribute), false)).Copyright;
        static string company = ((AssemblyCompanyAttribute)Attribute.GetCustomAttribute(execAsm, typeof(AssemblyCompanyAttribute), false)).Company;

        public string SnapInstallerVersion
        {
            get
            {
                return version;
            }
        }

        public string SnapInstallerProductName
        {
            get
            {
                return product;
            }
        }

        public string SnapInstallerCopyright
        {
            get
            {
                return copyright;
            }
        }

        public string SnapInstallerCompanyName
        {
            get
            {
                return company;
            }
        }
    }
}
