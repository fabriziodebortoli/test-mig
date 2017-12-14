using System;
using Microarea.TaskBuilderNet.Interfaces.Model;

//=============================================================================
public class HotLinkReattachEventArgs : EventArgs
{
	IMHotLink oldValue;
	IMHotLink newValue;

	public HotLinkReattachEventArgs(IMHotLink oldValue, IMHotLink newValue)
	{
		this.oldValue = oldValue; this.newValue = newValue;
	}

	public IMHotLink OldValue { get { return oldValue; } }
	public IMHotLink NewValue { get { return newValue; } }
}
