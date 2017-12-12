using Microarea.Snap.Core;
using Microarea.Snap.Installer.Properties;
using Microarea.Snap.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Installer
{
    internal class Command : OutputWriter, ICommand
    {
        bool ensureProductFolder;
        IInversionOfControlFactory factory;

        protected IInversionOfControlFactory Factory
        {
            get
            {
                return factory;
            }
        }

        protected Command(System.IO.TextWriter textWriter, IInversionOfControlFactory factory, bool ensureProductFolder = true)
            : base(textWriter)
        {
            this.ensureProductFolder = ensureProductFolder;
            this.factory = factory;
        }

        public virtual void Execute()
        {
            this.PrintHeader();

            if (this.ensureProductFolder)
            {
                var productFolderService = factory.GetInstance<Services.IProductFolderService>();
                productFolderService.EnsureProductFolder();
                if (!productFolderService.IsProductFolderSet)
                {
                    throw new Services.ProductFolderException(Resources.ProductFolderNotSet);
                }
            }
        }

        protected virtual void PrintHeader()
        {
            var snapInstallerVersion = factory.GetInstance<ISnapInstallerVersionService>();

            TextWriter.Write(snapInstallerVersion.SnapInstallerProductName);
            TextWriter.Write(" version ");
            TextWriter.Write(snapInstallerVersion.SnapInstallerVersion);
            TextWriter.WriteLine("");
            TextWriter.Write(snapInstallerVersion.SnapInstallerCopyright);
            TextWriter.Write(" ");
            TextWriter.Write(snapInstallerVersion.SnapInstallerCompanyName);
            TextWriter.WriteLine("");
            TextWriter.WriteLine("");
        }
    }
}
