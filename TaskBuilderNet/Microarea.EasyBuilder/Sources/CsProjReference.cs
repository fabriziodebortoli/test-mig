using System;
using System.Linq;
using System.Collections.Generic;
namespace Microarea.EasyBuilder
{
    //=========================================================================
    /// <summary>
    /// This class maps a csproj reference entry
    /// </summary>
    public class CsProjReference
    {
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Name of the reference
        /// </summary>
        public string ReferenceName { get; set; }
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Path of the reference
        /// </summary>
        public string ReferenceDllPath { get; set; }
    }
}