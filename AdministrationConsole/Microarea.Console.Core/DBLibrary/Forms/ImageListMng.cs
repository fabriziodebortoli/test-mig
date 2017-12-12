using System.Drawing;
using System.IO;
using System.Reflection;
using Microarea.Console.Core.PlugIns;
using Microarea.TaskBuilderNet.Data.DatabaseLayer;

namespace Microarea.Console.Core.DBLibrary.Forms
{
	/// <summary>
	/// Classe globale x la gestione delle Images nel DataManager
	/// </summary>
	//=========================================================================
	public partial class Images : System.ComponentModel.Component
	{
		// Properties
		//---------------------------------------------------------------------------
		public 	System.Windows.Forms.ImageList ImageList {	get { return imageList;} }

		# region Costruttori
		//---------------------------------------------------------------------------
		public Images(System.ComponentModel.IContainer container)
		{
			container.Add(this);
			InitializeComponent();
			AddBitmap();
		}

		//---------------------------------------------------------------------------
		public Images()
		{
			InitializeComponent();
			AddBitmap();			
		}
		# endregion

		/// <summary>
		/// inserisce nell'ImageList le varie bitmap (formato 16x16)
		/// </summary>
		//---------------------------------------------------------------------------
		private void AddBitmap()
		{
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultRed.png");
			AddImageToImageList(imageList, false, DatabaseLayerConsts.NamespacePlugInsImg + ".ResultGreen.png");
		}

		//---------------------------------------------------------------------------
		private int AddImageToImageList(System.Windows.Forms.ImageList imgList, bool fromExecutingAsm, string resourceName)
		{
			if (imgList == null || resourceName == null || resourceName.Length == 0)
				return -1;

			Assembly assembly = fromExecutingAsm 
				? Assembly.GetExecutingAssembly()
				: typeof(PlugIn).Assembly;

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

		#region Metodi statici
		//------------------------------------------------------------------
		// ImageList
		//------------------------------------------------------------------
		static public int GetUncheckedBitmapIndex()	{ return 0; }		
		static public int GetCheckedBitmapIndex()	{ return 1; }		
		#endregion

	}
}
