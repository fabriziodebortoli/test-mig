using System;
using System.Linq;
using System.Collections.Generic;
namespace Microarea.EasyBuilder
{
    //=========================================================================
    /// <summary>
    /// This class maps a csproj source file entry
    /// </summary>
    public class CsProjSourceFile
    {
        //--------------------------------------------------------------------------------
        /// <summary>
        /// The file path is relative to the project file path
        /// </summary>
        public string RelativeFilePath { get; set; }
    }
}