using System.Drawing;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;
using WeifenLuo.WinFormsUI.Docking;

namespace Microarea.MenuManager
{
	/// <summary>
	/// The skin used to display the document and tool strips and tabs.
	/// </summary>
	public class DockPaneStripThemeSkin : DockPaneStripSkin,  IDockPaneStripSkin
	{
		ITheme theme; 

		public DockPaneStripThemeSkin()
			: base()
		{
			theme = DefaultTheme.GetTheme();
		}

		public override void OverrideTheme()
		{
			Color tabBackColor = theme.GetThemeElementColor("DockPanelTabBackGroundColor");
			Color activeTabBackColor = theme.GetThemeElementColor("DockPanelActiveTabBackGroundColor");
			Color inactiveTabBackColor = theme.GetThemeElementColor("DockPanelInActiveTabBackGroundColor");

			Color activeTabForeColor = theme.GetThemeElementColor("DockPanelActiveTabForeGroundColor");
			Color inactiveTabForeColor = theme.GetThemeElementColor("DockPanelInActiveTabForeGroundColor");

			bool showBorder = theme.GetBoolThemeElement("DockPanelTabShowBorder");

			m_DocumentGradient.DockStripGradient.StartColor = Color.FromArgb(255, tabBackColor);
			m_DocumentGradient.DockStripGradient.EndColor = Color.FromArgb(255, tabBackColor);
			m_DocumentGradient.ActiveTabGradient.StartColor = Color.FromArgb(255, activeTabBackColor);
			m_DocumentGradient.ActiveTabGradient.EndColor = Color.FromArgb(255, activeTabBackColor);
			m_DocumentGradient.ActiveTabGradient.TextColor = Color.FromArgb(255, activeTabForeColor);
			m_DocumentGradient.ActiveTabGradient.ShowBorder = showBorder;

			m_DocumentGradient.InactiveTabGradient.StartColor = Color.FromArgb(255, inactiveTabBackColor);
			m_DocumentGradient.InactiveTabGradient.EndColor = Color.FromArgb(255, inactiveTabBackColor);
			m_DocumentGradient.InactiveTabGradient.TextColor = Color.FromArgb(255, inactiveTabForeColor);

			m_DocumentGradient.InactiveTabGradient.ShowBorder = showBorder;
		}
	}
}
