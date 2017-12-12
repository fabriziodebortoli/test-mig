using System;
namespace WeifenLuo.WinFormsUI.Docking
{
	public interface IDockPaneStripSkin
	{
		DockPaneStripGradient DocumentGradient { get; set; }
		DockPaneStripToolWindowGradient ToolWindowGradient { get; set; }
		void OverrideTheme();
	}
}
