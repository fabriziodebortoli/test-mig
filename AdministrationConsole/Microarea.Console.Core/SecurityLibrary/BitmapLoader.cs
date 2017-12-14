using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace Microarea.Console.Core.SecurityLibrary
{
	/// <summary>
	/// Summary description for BitmapLoader.
	/// </summary>
	public class BitmapLoader
	{
		private ArrayList bitmaps = new ArrayList();

		//---------------------------------------------------------------------
		public BitmapLoader(string [] bitmapNames)
		{
			for(int i=0; i < bitmapNames.Length; i++)
			{
				AddBitmap(bitmapNames[i]);
			}
		}

		//---------------------------------------------------------------------
		public int AddBitmap(string bitmapName)
		{
			if (bitmapName == null || bitmapName == String.Empty)
				return -1;

			Assembly executingAssembly = Assembly.GetExecutingAssembly();

			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(GrantConstString.SecurityLibraryNamespace + ".img." + bitmapName);
			if (stream == null)
				return -1;

			Bitmap bitmap = ((System.Drawing.Bitmap)Image.FromStream(stream));
			bitmap.MakeTransparent(Color.White);

			return bitmaps.Add(bitmap);
		}

		//---------------------------------------------------------------------
		public Bitmap GetBitmap(int indexImg)
		{
			if (indexImg < 0 || indexImg > bitmaps.Count)
				return null;
			return (Bitmap) bitmaps[indexImg];
		}
	}
}
