//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microarea.TaskBuilderNet.TbHermesBL
{
    using System;
    using System.Collections.Generic;
    
    public partial class OM_CommitmentsWorkers
    {
        public int CommitmentId { get; set; }
        public int WorkerId { get; set; }
        public string IsImminent { get; set; }
        public string HasReminder { get; set; }
        public string IsOwner { get; set; }
        public System.DateTime TBCreated { get; set; }
        public System.DateTime TBModified { get; set; }
        public int TBCreatedID { get; set; }
        public int TBModifiedID { get; set; }
        public int CalendarSubId { get; set; }
    }
}
