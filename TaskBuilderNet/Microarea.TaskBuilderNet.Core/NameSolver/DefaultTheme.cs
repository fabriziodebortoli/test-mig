using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.IO;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Core.NameSolver
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
			List<string> themes =  BasePathFinder.BasePathFinderInstance.GetAvailableThemesFullNames();

			string themeFileName = string.Empty;
			foreach (string item in themes)
			{
				if (item.ContainsNoCase(themeName))
				{
					themeFileName = item;
					break;
				}
			}

			if (themeFileName.IsNullOrEmpty() || !File.Exists(themeFileName))
				return;

			xmlThemeDocument = new XmlDocument();
			xmlThemeDocument.Load(themeFileName);
		}

		//---------------------------------------------------------------------
		private Color GetColorFromValue(string value)
		{
			int colorRef = Int32.Parse(value);
			
			return ColorTranslator.FromWin32(colorRef);
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

				int red, green, blue;

				red = int.Parse(r, System.Globalization.NumberStyles.HexNumber);
				green = int.Parse(g, System.Globalization.NumberStyles.HexNumber);
				blue = int.Parse(b, System.Globalization.NumberStyles.HexNumber);

				return Color.FromArgb(255, red, green, blue);
			}
			catch (Exception)
			{
				return Color.Empty;
			}
		}

		//---------------------------------------------------------------------
		public Color GetThemeElementColor(string themeElement)
        {
            if (xmlThemeDocument == null)
				return Color.Empty;
			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return Color.Empty;

			XmlAttribute rgbLongAttribute = node.Attributes["rgbLong"];
			if (rgbLongAttribute != null)
				return GetColorFromValue(rgbLongAttribute.Value);

			XmlAttribute rgbHexAttribute = node.Attributes["rgbHex"];
			if (rgbHexAttribute != null)
				return GetColorFromHexValue(rgbHexAttribute.Value);
			return Color.Empty;
		}

		//---------------------------------------------------------------------
		public Font GetThemeElementFont(string themeElement)
		{
			string fontName = "Segoe UI";
			string fontCharset = "DEFAULT";
			int fontSize = 10;
			int fontSmallSize = 3;
			FontStyle style = FontStyle.Regular;
			bool isUnderline, isStriked, isItalic;

			Font defaultFont = new Font(fontName, fontSize, style);

			//	<ThemeElement name="FormFontFace" type="font" fontName="Segoe UI" fontCharset="DEFAULT" fontSize="10" fontSmallSize="3" isUnderline="1" isStriked="0" isItalic="0"/><!-- customizzato ofm-->
		
			if (xmlThemeDocument == null)
				return defaultFont;

			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return defaultFont;

			
			XmlAttribute fontNameAttribute = node.Attributes["fontName"];
			if (fontNameAttribute != null)
				fontName = fontNameAttribute.Value;

			XmlAttribute fontCharsetAttribute = node.Attributes["fontCharset"];
			if (fontCharsetAttribute != null)
				fontCharset = fontCharsetAttribute.Value;

			XmlAttribute fontSizeAttribute = node.Attributes["fontSize"];
			if (fontSizeAttribute != null)
				int.TryParse(fontSizeAttribute.Value, out fontSize); ;

			XmlAttribute fontSmallSizeAttribute = node.Attributes["fontSmallSize"];
			if (fontSmallSizeAttribute != null)
				int.TryParse(fontSmallSizeAttribute.Value, out fontSmallSize); 

			XmlAttribute isUnderlineAttribute = node.Attributes["isUnderline"];
			if (isUnderlineAttribute != null)
			{
				bool.TryParse(isUnderlineAttribute.Value, out isUnderline);

				if (isUnderline)
 					style |= FontStyle.Underline;
			}

			XmlAttribute isStrikedAttribute = node.Attributes["isStriked"];
			if (isStrikedAttribute != null)
			{
				bool.TryParse(isStrikedAttribute.Value, out isStriked);

				if (isStriked)
 					style |= FontStyle.Strikeout;
			}

			XmlAttribute isItalicAttribute = node.Attributes["isItalic"];
			if (isItalicAttribute != null)
			{
				bool.TryParse(isItalicAttribute.Value, out isItalic);

				if (isItalic)
 					style |= FontStyle.Strikeout;
			}

			return new Font(fontName, fontSize, style);
		}

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
		public Image GetThemeElementImage(string themeElement)
		{
			if (xmlThemeDocument == null)
				return null;

			XmlNode node = xmlThemeDocument.SelectSingleNode(string.Format(themeElementXPathQuery, themeElement));
			if (node == null)
				return null	;

			string ns = string.Empty;
			XmlAttribute imageNamespaceAttribute = node.Attributes["value"];
			if (imageNamespaceAttribute != null)
				ns = imageNamespaceAttribute.Value.ToString();

			if (string.IsNullOrEmpty(ns))
				return null;

			try
			{
				string imagePath = BasePathFinder.BasePathFinderInstance.GetImagePath(new NameSpace(ns));
				return ImagesHelper.LoadImageWithoutLockFile(imagePath);
			}
			catch (Exception)
			{
				return null;
			}
		}
	}
}
