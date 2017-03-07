using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.StringLoader;
using Microarea.Common.Applications;
using Microarea.Common.Lexan;

using Microarea.RSWeb.WoormEngine;
using Microarea.RSWeb.WoormViewer;
using Microarea.RSWeb.WoormWebControl;
using Microarea.RSWeb.Models;
using Microarea.RSWeb.Objects;

namespace Microarea.RSWeb.Objects
{
    //=========================================================================
    [Serializable]
    //[KnownType(typeof(Layout))]
    public class ReportData : ISerializable
    {
        public Layout reportObjects;
        public short paperLength;
        public short paperWidth;

        const string REPORT_OBJECTS = "reportObjects";
        const string PAPER_LENGTH = "paperLength";
        const string PAPER_WIDTH = "paperWidth";

        //--------------------------------------------------------------------------
        public ReportData()
        {
        }

        //--------------------------------------------------------------------------
        public ReportData(SerializationInfo info, StreamingContext context)
        {
             //object[] arReportObjects = info.GetValue<object[]>(REPORT_OBJECTS);
            //if (arReportObjects != null)
            //{
            //    reportObjects = new Layout();
            //    foreach (object obj in arReportObjects)
            //        reportObjects.Add((BaseObj)obj);
            //}

            paperLength = info.GetInt16(PAPER_LENGTH);
            paperWidth = info.GetInt16(PAPER_WIDTH);
        }

        //--------------------------------------------------------------------------
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue(REPORT_OBJECTS, reportObjects);
            info.AddValue(PAPER_LENGTH, paperLength);
            info.AddValue(PAPER_WIDTH, paperWidth);
        }
    }

}
