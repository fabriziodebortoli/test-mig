
namespace Microarea.TaskBuilderNet.UI.WinControls
{
	//=========================================================================
	public interface IAnimationPath
	{
		//---------------------------------------------------------------------
		System.Drawing.Point StartPoint { get; set; }

		//---------------------------------------------------------------------
		System.Drawing.Point EndPoint { get; set; }

		//---------------------------------------------------------------------
		int AnimationSteps { get; }

		//---------------------------------------------------------------------
		int MillSecsBetweenPoints { get; }

		//---------------------------------------------------------------------
		System.Drawing.Point[] GetPathPoints();
	}
}
