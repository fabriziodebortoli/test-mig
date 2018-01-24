﻿using System.Collections;
using System.Collections.Generic;

namespace TaskBuilderNetCore.Interfaces
{
	//=========================================================================
	public interface IDocumentInfo
	{
		int AddViewMode(IViewMode aViewMode);
		string Classhierarchy { get; }
		string DefaultSecurityRoles { get; }
		IViewMode GetDefaultViewMode();
		string InterfaceClass { get; }
		bool IsSecurityhidden { get; set; }
		string Name { get; }
		INameSpace NameSpace { get; }
		string Title { get; }
		string Description { get; }
		IList ViewModes { get; }
        bool IsDynamic { get; set; }
        bool IsBatch { get; set; }
		bool IsFinder { get; set; }
		bool IsDataEntry { get; set; }
		bool IsSchedulable { get; set; }
        List<IDocumentInfoComponent> Components { get; }
	}

    //=========================================================================
    public interface IDocumentInfoComponent
    {
        INameSpace NameSpace { get; }
        string Activation { get; set; }
    }
}
