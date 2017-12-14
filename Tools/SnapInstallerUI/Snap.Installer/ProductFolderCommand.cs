using Microarea.Snap.Core;
using Microarea.Snap.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microarea.Snap.Services;

namespace Microarea.Snap.Installer
{
    internal class ProductFolderCommand : Command
    {
        string productFolder;
        bool productFolderFound;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public ProductFolderCommand(IEnumerable<string> args, TextWriter textWriter, IInversionOfControlFactory factory)
            : base(textWriter, factory)
        {
            try
            {
                if (args.Count() > 0)
                {
                    this.productFolder = args.ElementAt(0);
                    productFolderFound = true;
                }
            }
            catch (Exception exc)
            {
                TextWriter.WriteLine("Error during 'ProductFolder' command: " + exc.ToString());
            }
        }

        public override void Execute()
        {
            var productFolderService = Factory.GetInstance<IProductFolderService>();
            if (!productFolderFound)
            {
                base.Execute();
                TextWriter.WriteLine(productFolderService.ProductInstanceFolder);
            }
            else
            {
                this.PrintHeader();

                productFolderService.ProductInstanceFolder = productFolder;

                TextWriter.WriteLine("Product folder successfully set to " + productFolder);
            }
        }
    }
}
