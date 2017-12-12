using System.Drawing;
using System.IO;
using System.Reflection;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Plugin.ApplicationDBAdmin.Forms
{
	/// <summary>
	/// Classe globale x la gestione delle Images nell'ApplicationDBAdmin
	/// </summary>
	//=========================================================================
	public partial class ImagesListManager : System.ComponentModel.Component
	{
		//---------------------------------------------------------------------------
		public 	System.Windows.Forms.ImageList ImageList { get { return imageList;} }

		//---------------------------------------------------------------------------
		public ImagesListManager(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			AddBitmap();
		}

		//---------------------------------------------------------------------------
		public ImagesListManager()
		{
			InitializeComponent();
			AddBitmap();			
		}

		/// <summary>
		/// inserisce nell'ImageList le varie bitmap (formato 16x16)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddBitmap()
		{
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Application.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Module.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Table.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".View.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".StoredProc.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Default.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultGreen.png");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultRed.png");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".RedFlag.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".GreenFlag.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".TableUnchecked.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".ViewUnchecked.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".StoredProcUnchecked.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".DummyState.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".MagoNet16.png");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".EasyAttachment16.png");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Warning.bmp");
			AddImageToImageList(imageList, DatabaseLayerConsts.NamespacePlugInsImg + ".Information.bmp");
		}

		//---------------------------------------------------------------------------
		private int AddImageToImageList(System.Windows.Forms.ImageList imgList, string resourceName)
		{
			if (imgList == null || resourceName.Length == 0)
				return -1;

			Assembly assembly = typeof(PlugIn).Assembly;

			if (assembly == null)
				return -1;
			
			Stream imageStream = assembly.GetManifestResourceStream(resourceName);
			if (imageStream == null)
				return -1;

			Image image = Image.FromStream(imageStream);
			if (image == null)
				return -1;
				
			imgList.Images.Add(image, Color.Magenta);
				
			return imgList.Images.Count-1;
		}	

		//------------------------------------------------------------------
		// ImageList
		//------------------------------------------------------------------
		static public int GetApplicationBitmapIndex			() { return 0; }
		static public int GetModuleBitmapIndex				() { return 1; }
		static public int GetTableBitmapIndex				() { return 2; }
		static public int GetViewBitmapIndex				() { return 3; }
		static public int GetProcedureBitmapIndex			() { return 4; }
		static public int GetDefaultBitmapIndex				() { return 5; }				
		static public int GetCheckedBitmapIndex				() { return 6; }		
		static public int GetUncheckedBitmapIndex			() { return 7; }		
		static public int GetRedFlagBitmapIndex				() { return 8; }		
		static public int GetGreenFlagBitmapIndex			() { return 9; }		
		static public int GetTableUncheckedBitmapIndex		() { return 10;}		
		static public int GetViewUncheckedBitmapIndex		() { return 11;}		
		static public int GetProcedureUncheckedBitmapIndex	() { return 12;}
		static public int GetDummyStateBitmapIndex			() { return 13;}
		static public int GetMagoNet16BitmapIndex			() { return 14;}
		static public int GetEasyAttachment16BitmapIndex	() { return 15;}
		static public int GetWarningBitmapIndex				() { return 16;}
		static public int GetInformationBitmapIndex			() { return 17;}
	}
}