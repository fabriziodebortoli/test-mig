﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MZP_CompanyEntities : DbContext
    {
        public MZP_CompanyEntities()
            : base("name=MZP_CompanyEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<OM_MailMessagePop3> OM_MailMessagePop3 { get; set; }
        public DbSet<OM_MailMessageAddresses> OM_MailMessageAddresses { get; set; }
        public DbSet<OM_MailMessageAttachments> OM_MailMessageAttachments { get; set; }
        public DbSet<OM_MailMessageHTML> OM_MailMessageHTML { get; set; }
        public DbSet<OM_MailMessageLinks> OM_MailMessageLinks { get; set; }
        public DbSet<OM_MailMessageWorkers> OM_MailMessageWorkers { get; set; }
        public DbSet<OM_WorkersMailRulesFilters> OM_WorkersMailRulesFilters { get; set; }
        public DbSet<OM_WorkersMailRulesMastersLinked> OM_WorkersMailRulesMastersLinked { get; set; }
        public DbSet<OM_WorkersMailRulesWorkerShared> OM_WorkersMailRulesWorkerShared { get; set; }
        public DbSet<OM_MailMessages> OM_MailMessages { get; set; }
        public DbSet<OM_WorkerAccounts> OM_WorkerAccounts { get; set; }
        public DbSet<OM_WorkersMailRules> OM_WorkersMailRules { get; set; }
        public DbSet<OM_Workers> OM_Workers { get; set; }
        public DbSet<OM_MailFooter> OM_MailFooter { get; set; }
        public DbSet<OM_WorkersPreferencesDetails> OM_WorkersPreferencesDetails { get; set; }
        public DbSet<OM_WorkersAlerts> OM_WorkersAlerts { get; set; }
        public DbSet<OM_GoogleAccounts> OM_GoogleAccounts { get; set; }
        public DbSet<OM_GoogleCalendars> OM_GoogleCalendars { get; set; }
        public DbSet<OM_GoogleCalendarsEvents> OM_GoogleCalendarsEvents { get; set; }
        public DbSet<OM_GoogleEvents> OM_GoogleEvents { get; set; }
        public DbSet<OM_CommitmentsWorkers> OM_CommitmentsWorkers { get; set; }
        public DbSet<OM_WorkersCalendars> OM_WorkersCalendars { get; set; }
        public DbSet<OM_MastersContacts> OM_MastersContacts { get; set; }
        public DbSet<OM_Facilities> OM_Facilities { get; set; }
        public DbSet<OM_FacilitiesDetails> OM_FacilitiesDetails { get; set; }
        public DbSet<OM_Commitments> OM_Commitments { get; set; }
        public DbSet<OM_ModulesOFM> OM_ModulesOFM { get; set; }
    }
}