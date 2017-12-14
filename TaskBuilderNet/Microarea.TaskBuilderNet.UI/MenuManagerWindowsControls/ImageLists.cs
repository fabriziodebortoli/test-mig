using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls
{
	public sealed class ImageLists
	{
		//-----------------------------------------------------------------------------
		public static ImageList FavoritesStructTreeImgList 
		{
			//questo codice prima era dentro il progetto Mago.Net, ma non volevo GetManifestResourceStream 
			//caricati da altri progetti.
			//Al momento sono così,  in futuro diventeranno png embedded come "Resources."
			get
			{
				ImageList favoritesStructTreeImgList = new ImageList();

				Stream imageStream = Assembly.GetAssembly(typeof(MenuMngWinCtrl)).GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.FavoritesImgSmall.bmp");
				if (imageStream != null)
				{
					System.Drawing.Image image = Image.FromStream(imageStream);
					if (image != null)
						favoritesStructTreeImgList.Images.Add(image);
				}

				imageStream = Assembly.GetAssembly(typeof(MenuMngWinCtrl)).GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultGroupImage.bmp");
				if (imageStream != null)
				{
					System.Drawing.Image image = Image.FromStream(imageStream);
					if (image != null)
						favoritesStructTreeImgList.Images.Add(image);
				}

				imageStream = Assembly.GetAssembly(typeof(MenuMngWinCtrl)).GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultMenuImgSmall.bmp");
				if (imageStream != null)
				{
					System.Drawing.Image image = Image.FromStream(imageStream);
					if (image != null)
						favoritesStructTreeImgList.Images.Add(image);
				}

				imageStream = Assembly.GetAssembly(typeof(MenuMngWinCtrl)).GetManifestResourceStream("Microarea.TaskBuilderNet.UI.MenuManagerWindowsControls.Bitmaps.DefaultCommandImgSmall.bmp");
				if (imageStream != null)
				{
					System.Drawing.Image image = Image.FromStream(imageStream);
					if (image != null)
						favoritesStructTreeImgList.Images.Add(image);
				}
				favoritesStructTreeImgList.TransparentColor = Color.Magenta;
				return favoritesStructTreeImgList;
			}
		}
	}
}
