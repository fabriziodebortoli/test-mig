using System;
using System.Drawing;

using Microarea.Common.Applications;
using Microarea.Common.Generic;
using Microarea.RSWeb.WoormViewer;
using Microarea.Common.Temp;
using TaskBuilderNetCore.Interfaces;
using System.Net;
using Microarea.Common.NameSolver;
using System.IO;

namespace Microarea.RSWeb.WoormWebControl
{
    internal enum BoxType { Text, Cell, TableTitle, ColumnTitle, SubTotal, Total }	

	//================================================================================
	public static class Helper
	{
		public const string FileNameParam				= "Filename";
		public const string NameSpaceParam				= "Namespace";
		public const string PrintParam					= "Print";

		//--------------------------------------------------------------------------------
		public static string FormatParametersForRequest(string xmlDomParameters)
		{
			return WebUtility.UrlEncode(xmlDomParameters);
		}

		//--------------------------------------------------------------------------------
		public static string UnformatParametersFromRequest(string xmlFormattedParameters)
		{
			return WebUtility.UrlDecode(xmlFormattedParameters);
		}

		//------------------------------------------------------------------------------
		//public static string GetConnectionKey(string reportNamespace, string reportParameters)
		//{
		//	return "__connectionKey:" + reportNamespace + reportParameters;
		//}
		
		// se il fontStyle selezionato ha un colore settato allora uso lui come colore
		// solo se l'elemento ha il suo colore di default e se il font è nero (default)
		//------------------------------------------------------------------------------
		internal static Color TrueColor(this WoormDocument woorm, BoxType boxType, Color elementColor, string fontStyleName)
		{
			FontElement fe = woorm.GetFontElement(fontStyleName);
			if (fe == null) return elementColor;

			if (elementColor == BoxTypeDefaultColor(boxType) && fe.Color != Color.FromArgb(255,255,255,255))
				return fe.Color;

			return elementColor;
		}

		//------------------------------------------------------------------------------
		internal static Color BoxTypeDefaultColor(BoxType boxType)
		{
			return (boxType == BoxType.TableTitle) ? Color.FromArgb(255, 0, 0, 0) : Color.FromArgb(255, 255, 255, 255);
		}

		///<summary>
		///Metodo che dato un rettangolo, calcola il rettangolo che rappresenta l'area interna disponibile
		///tenedo conto della presenza di bordi, e del Pen che viene usato per disegnarli
		/// </summary>
		//------------------------------------------------------------------------------
		public static Rectangle InflateForPosition(this Rectangle rect,Borders borders,BorderPen pen)
		{
			return InflateForPosition(rect, borders, pen, pen);
		}

		///<summary>
		///Metodo che dato un rettangolo, calcola il rettangolo che rappresenta l'area interna disponibile
		///tenedo conto della presenza di bordi, e dei Pen che vengono usati per disegnarli (un pen per i bordi top-left-right, 
		///e uno per il bordo inferiore: usato perche il separatore di riga(che coincide con il bordo inferiore della cella) puo essere diverso 
		///dagli altri bordi)
		/// </summary>
		//------------------------------------------------------------------------------
		public static Rectangle InflateForPosition(this Rectangle rect,Borders borders,BorderPen pen, BorderPen bottomPen)
		{
			int left = borders.Left ? pen.Width : 0;
			int top = borders.Top ? pen.Width : 0;
			int bottom = borders.Bottom ? bottomPen.Width : 0;
			int right = borders.Right ? pen.Width : 0;

			int x = rect.X;
			int y = rect.Y;

			int width = rect.Width - left - right;
			int height = rect.Height - top - bottom;

			// controllo che il rettangolo abbia dimensioni reali
			width = width < 0 ? 0 : width;
			height = height < 0 ? 0 : height;

			Rectangle inflated = new Rectangle(x, y, width, height);
			return inflated;
		}

		///<summary>
		///Metodo che dato un rettangolo, calcola il rettangolo che rappresenta l'area interna disponibile
		///tenedo conto della presenza di bordi, e del Pen che viene usato per disegnarli
		/// </summary>
		//------------------------------------------------------------------------------
		public static Rectangle Inflate(Rectangle rect, Borders borders, BorderPen pen)
		{
			return Inflate(rect, borders, pen, pen);
		}

		///<summary>
		///Metodo che dato un rettangolo, calcola il rettangolo che rappresenta l'area interna disponibile
		///tenedo conto della presenza di bordi, e dei Pen che vengono usati per disegnarli (un pen per i bordi top-left-right, 
		///e uno per il bordo inferiore: usato perche il separatore di riga(che coincide con il bordo inferiore della cella) puo essere diverso 
		///dagli altri bordi)
		/// </summary>
		//------------------------------------------------------------------------------
		public static Rectangle Inflate(Rectangle rect, Borders borders, BorderPen pen, BorderPen bottomPen)
		{
			int left = borders.Left ? pen.Width : 0;
			int top = borders.Top ? pen.Width : 0;
			int bottom = borders.Bottom ? bottomPen.Width : 0;
			int right = borders.Right ? pen.Width : 0;

			int x = rect.X + left;
			int y = rect.Y + top;

			int width = rect.Width - left - right;
			int height = rect.Height - top - bottom;

			// controllo che il rettangolo abbia dimensioni reali
			width = width < 0 ? 0 : width;
			height = height < 0 ? 0 : height;

			Rectangle inflated = new Rectangle(x,y,width,height);
			return inflated;
		}

		//------------------------------------------------------------------------------
		internal static string ReadTextFile(this WoormDocument woorm, string textFilename)
		{
			StreamReader inputFile = null;
			string text = "";
            string filename = PathFinder.PathFinderInstance.ExistFile(textFilename) ? 
                textFilename : 
                woorm.GetFilename(textFilename, NameSpaceObjectType.Text);

			if (PathFinder.PathFinderInstance.ExistFile(filename))
			{
				try
				{
                    using (Stream fs = PathFinder.PathFinderInstance.GetStream(filename, true))//TODO LARA
                    {
                        inputFile = new StreamReader(fs, System.Text.Encoding.GetEncoding(0));
                        string buffer;

                        do
                        {
                            buffer = inputFile.ReadLine();
                            if (buffer != null) text += buffer + "<br/>";
                        }
                        while (buffer != null);
                    }
				}
				catch (IOException e)
				{
					return e.ToString();
				}
				finally
				{
					if (inputFile != null)
						inputFile.Dispose();
				}
				return text;
			}

			//analogia con woorm c++.Se non c'e' il file ritorna stringa vuota cosi da non "sporcare" il report
			return string.Empty;
		}
		//------------------------------------------------------------------------------
		internal static string GetFilename(this WoormDocument woorm, string filenameOrNamespace, NameSpaceObjectType type)
		{
			NameSpace ns = new NameSpace(filenameOrNamespace, type);
			string filename = woorm.ReportSession.PathFinder.GetFilename(ns, string.Empty);
			if (filename == string.Empty) filename = filenameOrNamespace;

			return filename;
		}

		///<summary>
		///Restituisce true se la stringa e' su piu linee e se lo stile di allineamento non e' singleLine
		///</summary>
		//------------------------------------------------------------------------------
		internal static bool IsMultilineString(string text, AlignType align)
		{
			return ((align & AlignType.DT_SINGLELINE) == 0)
					&& 
					(text.IndexOf(Chars.CR) != -1 || text.IndexOf(Chars.LF) != -1);
		}

		///<summary>
		///Metodo usato per splittare i testi multiline
		///</summary>
		//------------------------------------------------------------------------------
		internal static string[] SplitMultilineString(string text)
		{
			char[] separator = new char[]{Chars.LF,Chars.CR};
			return text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
		}

		/// <summary>
		/// Caratteri usati per lo split della stringa.
		/// </summary>
		/// ================================================================================
		internal static class Chars
		{
			public const char CR           = '\x0D';
			public const char LF           = '\x0A';
		}
	}
}
