using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Drawing;

using TaskBuilderNetCore.Interfaces;

using Microarea.Common.Generic;

//using Microarea.Common.Temp;

namespace Microarea.Common.NameSolver
{
	//=========================================================================
	/// <summary>
	/// UI theme for the application
	/// </summary>
	public class DefaultTheme : ITheme
	{
		static DefaultTheme theme;
		const string defaultTheme = "Default.theme";
		const string themeElementXPathQuery = "//ThemeElement[@name='{0}']";
		string name;
		public static event EventHandler ThemeChanged;

		XmlDocument xmlThemeDocument = null;

		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}


		//---------------------------------------------------------------------
		public static DefaultTheme GetTheme()
		{
			if (theme == null)
			{
				theme = new DefaultTheme();
			}
			return theme;
		}

		//---------------------------------------------------------------------
		public DefaultTheme() :
			this(defaultTheme)
		{
		}

		//---------------------------------------------------------------------
		public static void ReloadTheme(string themePath)
		{
			theme = new DefaultTheme(themePath);
			if (ThemeChanged != null)
				ThemeChanged(null, EventArgs.Empty);
		}

		//---------------------------------------------------------------------
		private DefaultTheme(string themeName)
		{
			this.name = Path.GetFileNameWithoutExtension(themeName);
			List<string> themes = PathFinder.PathFinderInstance.GetAvailableThemesFullNames();

			string themeFileName = string.Empty;
			foreach (string item in themes)
			{
				if (item.ContainsNoCase(themeName))
				{
					themeFileName = item;
					break;
				}
			}

			if (themeFileName.IsNullOrEmpty() || !PathFinder.PathFinderInstance.ExistFile(themeFileName))
				return;

            //xmlThemeDocument = new XmlDocument();
            //using (FileStream fs = File.Open(themeFileName, FileMode.Open, FileAccess.Read))
            //	xmlThemeDocument.Load(fs);
            xmlThemeDocument = new XmlDocument();
            xmlThemeDocument = PathFinder.PathFinderInstance.LoadXmlDocument(xmlThemeDocument, themeFileName);

        }

		//---------------------------------------------------------------------
		private Color GetColorFromValue(string value)
		{
			int colorRef = Int32.Parse(value);

			return Color.FromArgb(colorRef); //ColorTranslator.FromWin32(colorRef); TODO rsweb
		}

		//---------------------------------------------------------------------
		private Color GetColorFromHexValue(string value)
		{
			try
			{
				value = value.Replace("#", "");
				string r, g, b;
				r = value.Substring(0, 2);
				g = value.Substring(2, 2);
				b = value.Substring(4, 2);

				byte red, green, blue;

				red = byte.Parse(r, System.Globalization.NumberStyles.HexNumber);
				green = byte.Parse(g, System.Globalization.NumberStyles.HexNumber);
				blue = byte.Parse(b, System.Globalization.NumberStyles.HexNumber);

				return Color.FromArgb(255, red, green, blue);
			}
			catch (Exception)
			{
				return new Color();
			}
		}

		//---------------------------------------------------------------------
		public Color GetThemeElementColor(string themeElement)
		{
			if (xmlThemeDocument == null)
				return new Color();
			;
			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return new Color();
			;

			XmlAttribute rgbLongAttribute = node.Attributes["rgbLong"];
			if (rgbLongAttribute != null)
				return GetColorFromValue(rgbLongAttribute.Value);

			XmlAttribute rgbHexAttribute = node.Attributes["rgbHex"];
			if (rgbHexAttribute != null)
				return GetColorFromHexValue(rgbHexAttribute.Value);
			return new Color();
			;
		}

		//---------------------------------------------------------------------
		//public Font GetThemeElementFont(string themeElement)                      TODO rsweb
		//{
		//	string fontName = "Segoe UI";
		//	string fontCharset = "DEFAULT";
		//	int fontSize = 10;
		//	int fontSmallSize = 3;
		//	FontStyle style = FontStyle.Regular;
		//	bool isUnderline, isStriked, isItalic;

		//	Font defaultFont = new Font(fontName, fontSize, style);

		//	//	<ThemeElement name="FormFontFace" type="font" fontName="Segoe UI" fontCharset="DEFAULT" fontSize="10" fontSmallSize="3" isUnderline="1" isStriked="0" isItalic="0"/><!-- customizzato ofm-->

		//	if (xmlThemeDocument == null)
		//		return defaultFont;

		//	XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
		//	if (node == null)
		//		return defaultFont;


		//	XmlAttribute fontNameAttribute = node.Attributes["fontName"];
		//	if (fontNameAttribute != null)
		//		fontName = fontNameAttribute.Value;

		//	XmlAttribute fontCharsetAttribute = node.Attributes["fontCharset"];
		//	if (fontCharsetAttribute != null)
		//		fontCharset = fontCharsetAttribute.Value;

		//	XmlAttribute fontSizeAttribute = node.Attributes["fontSize"];
		//	if (fontSizeAttribute != null)
		//		int.TryParse(fontSizeAttribute.Value, out fontSize); ;

		//	XmlAttribute fontSmallSizeAttribute = node.Attributes["fontSmallSize"];
		//	if (fontSmallSizeAttribute != null)
		//		int.TryParse(fontSmallSizeAttribute.Value, out fontSmallSize); 

		//	XmlAttribute isUnderlineAttribute = node.Attributes["isUnderline"];
		//	if (isUnderlineAttribute != null)
		//	{
		//		bool.TryParse(isUnderlineAttribute.Value, out isUnderline);

		//		if (isUnderline)
		//				style |= FontStyle.Underline;
		//	}

		//	XmlAttribute isStrikedAttribute = node.Attributes["isStriked"];
		//	if (isStrikedAttribute != null)
		//	{
		//		bool.TryParse(isStrikedAttribute.Value, out isStriked);

		//		if (isStriked)
		//				style |= FontStyle.Strikeout;
		//	}

		//	XmlAttribute isItalicAttribute = node.Attributes["isItalic"];
		//	if (isItalicAttribute != null)
		//	{
		//		bool.TryParse(isItalicAttribute.Value, out isItalic);

		//		if (isItalic)
		//				style |= FontStyle.Strikeout;
		//	}

		//	return new Font(fontName, fontSize, style);
		//}

		//---------------------------------------------------------------------
		public bool GetBoolThemeElement(string themeElement)
		{
			if (xmlThemeDocument == null)
				return false;

			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return false;

			XmlAttribute valueAttribute = node.Attributes["value"];
			if (valueAttribute == null)
				return false;

			try
			{
				int res = int.Parse(valueAttribute.Value);
				return res == 1;
			}
			catch { }

			bool val = false;
			bool.TryParse(valueAttribute.Value, out val);
			return val;
		}

		//---------------------------------------------------------------------
		public string GetStringThemeElement(string themeElement)
		{
			if (xmlThemeDocument == null)
				return string.Empty;

			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return string.Empty;

			XmlAttribute valueAttribute = node.Attributes["value"];
			if (valueAttribute != null)
				return valueAttribute.Value.ToString();

			return string.Empty;
		}

		//---------------------------------------------------------------------
		public IImage GetThemeElementImage(string themeElement)                   //Image al posto di IImage TODO rsweb
		{
			if (xmlThemeDocument == null)
				return null;

			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return null;

			string ns = string.Empty;
			XmlAttribute imageNamespaceAttribute = node.Attributes["value"];
			if (imageNamespaceAttribute != null)
				ns = imageNamespaceAttribute.Value.ToString();

			if (string.IsNullOrEmpty(ns))
				return null;

			try
			{
				string imagePath = PathFinder.PathFinderInstance.GetImagePath(new NameSpace(ns));
				return null; //ImagesHelper.LoadImageWithoutLockFile(imagePath);  TODO rsweb
			}
			catch (Exception)
			{
				return null;
			}
		}

		int ITheme.GetThemeElementColor(string themeElement)
		{
			throw new NotImplementedException();
		}

	}
}
