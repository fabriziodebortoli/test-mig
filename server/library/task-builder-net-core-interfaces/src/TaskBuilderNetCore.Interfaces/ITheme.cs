

namespace TaskBuilderNetCore.Interfaces
{
    public interface ITheme
    {
		int GetThemeElementColor(string themeElement);
		string GetStringThemeElement(string themeElement);
		//Font GetThemeElementFont(string themeElement);
		//Image GetThemeElementImage(string themeElement);
		bool GetBoolThemeElement(string themeElement);
		string Name { get; set; }
	}
}
