﻿using System;

namespace Microarea.AdminServer.Model.Interfaces
{
	//================================================================================
	public interface IInstance 
	{
        string InstanceKey { get; set; }
        string Description { get; set; }
		bool Disabled { get; set; }
        string Origin { get; set; }
        string Tags { get; set; }
        bool UnderMaintenance { get; set; }
        DateTime PendingDate { get; set; }
        int VerificationCode { get; set; }
        int Ticks { get; set; }
		string SecurityValue { get; set; }
	}
}
