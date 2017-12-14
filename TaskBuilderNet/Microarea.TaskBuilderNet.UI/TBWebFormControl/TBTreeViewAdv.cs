using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	///<summary>
	///Classe che renderizza in HTML il TreeViewAdv della libreria AGA.Controls scirtta in c#
	///Eredita dalla classe TBTreeView (che renderizza il CTreeCtrl MFC)
	/// </summary>
	//=========================================================================
	class TBTreeViewAdv : TBTreeView
	{
	}

	///<summary>
	///Classe che renderizza in HTML il TreeNodeAdv della libreria AGA.Controls scirtta in c#
	///Eredita dalla classe TBTreeNode (che renderizza il Nodo del CTreeCtrl MFC)
	/// </summary>
	//=========================================================================
	class TBTreeNodeAdv : TBTreeNode
	{
		// metodo che crea l'immagine e la salva su file system (per referenziarla poi dall'HTML)
		//dato lo stream di byte delll'immagine
		//--------------------------------------------------------------------------------------
		protected static void CreateImage(ImageBuffer bitmap)
		{
			try
			{
				CreateDocumentImage(bitmap,GetImageName(bitmap.Id,true),GetImageName(bitmap.Id,false));
			}
			catch
			{
			}
		}

		// imposta gli attributi del nodo, in particolare l'icona del nodo
		//--------------------------------------------------------------------------------------
		public override void SetControlAttributes()
		{
			base.SetControlAttributes();
			
			//Immagine associata al nodo
			ImageBuffer bitmap = ((WndImageDescription)ControlDescription).Image;
			if (bitmap != null && !bitmap.IsEmpty)
			{
				CreateImage(bitmap);
				icon.ImageUrl = ImagesHelper.GetImageUrl(bitmap.Id);

				//sposto l'etichetta del nodo per fare spazio all'icona del nodo
				using (Bitmap b = bitmap.CreateBitmap())
				{
					toggle.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X).ToString();
					icon.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + 20).ToString();
					label.Style[HtmlTextWriterStyle.Left] = Unit.Pixel(X + b.Width + 25).ToString();
				}
			}

			label.Attributes.Add("onContextMenu", string.Format("onTreeAdvContextMenu(event, '{0}', '{1}');return false;", TreeView.WindowId/*treeid*/, InnerControl.ClientID /*nodekey*/));
		}
	}
}
