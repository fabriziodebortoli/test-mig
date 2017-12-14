using System;
using System.Drawing;
using System.Xml;

namespace Microarea.TaskBuilderNet.Interfaces
{
    public interface ITheme
    {
		Color GetThemeElementColor(string themeElement);
		string GetStringThemeElement(string themeElement);
		Font GetThemeElementFont(string themeElement);
		Image GetThemeElementImage(string themeElement);
		bool GetBoolThemeElement(string themeElement);
		string Name { get; set; }
	}
}
