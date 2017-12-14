using System.Drawing;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public interface IAnimationSize
	{
		//---------------------------------------------------------------------
		Size StartSize { get; set; }

		//---------------------------------------------------------------------
		Size EndSize { get; set; }

		//---------------------------------------------------------------------
		Size[] GetSizes();
	}
}
