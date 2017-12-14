using System.Drawing;
using System.IO;
using System.Reflection;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;
using Microarea.TaskBuilderNet.Data.DataManagerEngine;

namespace Microarea.Console.Core.DataManager.Common
{
	/// <summary>
	/// Classe globale x la gestione delle Images nel DataManager
	/// </summary>
	//=========================================================================
	public partial class Images : System.ComponentModel.Component
	{
		//--------------------------------------------------------------------------------
		public System.Windows.Forms.ImageList ImageList { get { return imageList; } }
		//--------------------------------------------------------------------------------
		public System.Windows.Forms.ImageList SmallPictureImageList { get { return smallPictureImageList; } }
		//--------------------------------------------------------------------------------
		public System.Windows.Forms.ImageList LargePictureImageList { get { return largePictureImageList; } }

		//--------------------------------------------------------------------------------
		public Images(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			AddBitmap();
			AddSmallBitmap();
			AddLargeBitmap();
		}

		//---------------------------------------------------------------------------
		public Images()
		{
			InitializeComponent();
			AddBitmap();
			AddSmallBitmap();
			AddLargeBitmap();
		}

		#region Add bitmap methods
		/// <summary>
		/// inserisce nell'ImageList le varie bitmap (formato 16x16)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddBitmap()
		{
			// prima considero la bitmap dell'application
			// attenzione che se cambio l'ordine devo cambiare anche l'indice nella bitmap nel relativo metodo statico
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".Application.bmp");
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".Module.bmp");
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".Table.bmp");
			AddImageToImageList(imageList, true, DataManagerConsts.NamespaceDataManagerImg + ".Xml.bmp"); //fromExecutingAssembly
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".Default.bmp");
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultRed.png");
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".Key.bmp");
			AddImageToImageList(imageList, true, DataManagerConsts.NamespaceDataManagerImg + ".Column.bmp"); //fromExecutingAssembly
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultGreen.png");
			AddImageToImageList(imageList, true, DataManagerConsts.NamespaceDataManagerImg + ".XmlYellow.bmp"); //fromExecutingAssembly
			AddImageToImageList(imageList, true, DataManagerConsts.NamespaceDataManagerImg + ".SelectedColumn.bmp"); //fromExecutingAssembly
		}

		/// <summary>
		/// inserisce nella SmallImageList le varie bitmap (formato 47x47)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddSmallBitmap()
		{
			//prima considero la bitmap dell'application
			AddImageToImageList(smallPictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".DefaultDataSmall.bmp");
			AddImageToImageList(smallPictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".ExportDataSmall.bmp");
			AddImageToImageList(smallPictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".ImportDataSmall.bmp");
			AddImageToImageList(smallPictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".SampleDataSmall.bmp");
		}

		/// <summary>
		/// inserisce nella LargeImageList le varie bitmap (formato 255x255 (anche se sono più grandi))
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddLargeBitmap()
		{
			AddImageToImageList(largePictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".DefaultData.bmp");
			AddImageToImageList(largePictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".ExportData.bmp");
			AddImageToImageList(largePictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".ImportData.bmp");
			AddImageToImageList(largePictureImageList, true, DataManagerConsts.NamespaceDataManagerImg + ".SampleData.bmp");
		}

		/// <summary>
		/// Carica la bitmap selezionata dall'assemblyName e dal resourceName
		/// </summary>
		//---------------------------------------------------------------------------
		private int AddImageToImageList(System.Windows.Forms.ImageList imgList, bool fromExecutingAsm, string resourceName)
		{
			if (imgList == null || resourceName.Length == 0)
				return -1;

			Assembly assembly = fromExecutingAsm
				? Assembly.GetExecutingAssembly()
				: typeof(PlugIn).Assembly; ;

			if (assembly == null)
				return -1;

			Stream imageStream = assembly.GetManifestResourceStream(resourceName);
			if (imageStream == null)
				return -1;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return -1;

			imgList.Images.Add(image, Color.Magenta);

			return imgList.Images.Count - 1;
		}
		#endregion

		#region Metodi statici
		//------------------------------------------------------------------
		// ImageList
		//------------------------------------------------------------------
		static public int GetApplicationBitmapIndex() { return 0; }
		//--------------------------------------------------------------------------------
		static public int GetModuleBitmapIndex() { return 1; }
		//--------------------------------------------------------------------------------
		static public int GetTableBitmapIndex() { return 2; }
		//--------------------------------------------------------------------------------
		static public int GetXmlFileBitmapIndex() { return 3; }
		//--------------------------------------------------------------------------------
		static public int GetFolderBitmapIndex() { return 4; }
		//--------------------------------------------------------------------------------
		static public int GetUncheckedBitmapIndex() { return 5; }
		//--------------------------------------------------------------------------------
		static public int GetKeyBitmapIndex() { return 6; }
		//--------------------------------------------------------------------------------
		static public int GetColumnBitmapIndex() { return 7; }
		//--------------------------------------------------------------------------------
		static public int GetCheckedBitmapIndex() { return 8; }
		//--------------------------------------------------------------------------------
		static public int GetYellowXmlFileBitmapIndex() { return 9; }
		//--------------------------------------------------------------------------------
		static public int GetSelectedColumnBitmapIndex() { return 10; }

		//------------------------------------------------------------------
		// SmallImageList
		//------------------------------------------------------------------
		static public int GetDefaultBmpSmallIndex() { return 0; }
		//--------------------------------------------------------------------------------
		static public int GetExportBmpSmallIndex() { return 1; }
		//--------------------------------------------------------------------------------
		static public int GetImportBmpSmallIndex() { return 2; }
		//--------------------------------------------------------------------------------
		static public int GetSampleBmpSmallIndex() { return 3; }

		//------------------------------------------------------------------
		// LargeImageList
		//------------------------------------------------------------------
		static public int GetDefaultBmpLargeIndex() { return 0; }
		//--------------------------------------------------------------------------------
		static public int GetExportBmpLargeIndex() { return 1; }
		//--------------------------------------------------------------------------------
		static public int GetImportBmpLargeIndex() { return 2; }
		//--------------------------------------------------------------------------------
		static public int GetSampleBmpLargeIndex() { return 3; }
		#endregion
	}
}
