using System;
using System.Linq;
using System.Collections.Generic;
using Microarea.Snap.Core;
using Microarea.Snap.Services;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microarea.Snap.Installer
{

    internal class OutputWriter
    {
        TextWriter textWriter;

        protected OutputWriter(TextWriter textWriter)
        {
            this.textWriter = textWriter;
        }

        internal TextWriter TextWriter
        {
            get
            {
                return textWriter;
            }
        }
    }

}