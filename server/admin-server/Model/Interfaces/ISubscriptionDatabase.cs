﻿namespace Microarea.AdminServer.Model.Interfaces
{
     //================================================================================
    public interface ISubscriptionDatabase : IModelObject
	{
		string SubscriptionKey { get; set; }
		string Name { get; set; }
        string InstanceKey { get; set; }
        string Description { get; set; }
		string DBServer { get; set; }
		string DBName { get; set; }
		string DBOwner { get; set; }
		string DBPassword { get; set; }
		bool UseDMS { get; }
		string DMSDBServer { get; set; }
		string DMSDBName { get; set; }
		string DMSDBOwner { get; set; }
		string DMSDBPassword { get; set; }
        bool Disabled { get; }
        string DatabaseCulture { get; }
        bool IsUnicode { get; }
		string Language { get; }
		string RegionalSettings { get; }
		string Provider { get; }
		bool Test { get; }
	}
}
