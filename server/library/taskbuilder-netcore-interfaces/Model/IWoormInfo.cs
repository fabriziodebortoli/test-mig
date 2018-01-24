using System;
using System.Collections.Generic;
using System.Text;

namespace TaskBuilderNetCore.Interfaces.Model
{
 	//=============================================================================
	public interface IWoormInfo
{
    string[] GetInputParamNames();

    object GetInputParamValue(string name);
}

public delegate IWoormInfo CreateWoormInfoWrapper(IntPtr woormInfoPtr);
}
