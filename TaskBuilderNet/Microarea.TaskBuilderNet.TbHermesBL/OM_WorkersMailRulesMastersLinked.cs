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
    
    public partial class OM_WorkersMailRulesMastersLinked
    {
        public int WorkersMailRulesId { get; set; }
        public int MasterLinkedSubId { get; set; }
        public string MasterCode { get; set; }
        public Nullable<int> OfficeFileId { get; set; }
        public System.DateTime TBCreated { get; set; }
        public System.DateTime TBModified { get; set; }
        public int TBCreatedID { get; set; }
        public int TBModifiedID { get; set; }
    
        public virtual OM_WorkersMailRules OM_WorkersMailRules { get; set; }
    }
}
