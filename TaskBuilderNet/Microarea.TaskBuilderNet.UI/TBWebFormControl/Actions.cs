using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using Microarea.TaskBuilderNet.Core.NameSolver;

namespace Microarea.TaskBuilderNet.UI.TBWebFormControl
{
	internal abstract class Action
	{
		//*** IMPORTANT! ****
		//this enum has to correspond to 'WebCommandType' one declared in TBGenlib\BaseDoc.h
		internal enum WebCommandType
		{
			WEB_UNDEFINED = -1,
			WEB_UNSUPPORTED = 0,
			WEB_NORMAL = 1,
			WEB_LINK = 2,
		};

		protected string fromIds;
		protected string fromValue;
		protected TBWebFormControl formControl;
		private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

		//--------------------------------------------------------------------------------------
		protected Action(TBWebFormControl formControl, string fromIds, string fromValue)
		{
			this.formControl = formControl;
			this.fromIds = formControl.GetServerId(fromIds);
			this.fromValue = fromValue;
		}

		//--------------------------------------------------------------------------------------
		public abstract byte[] Execute();
				
		//--------------------------------------------------------------------------------------
		public static Action ParseAction(TBWebControl control, string eventArgument)
		{
			List<string> args = null;
			try
			{
				DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(List<string>));
				XmlDictionaryReaderQuotas quotas = new XmlDictionaryReaderQuotas();
				quotas.MaxStringContentLength = int.MaxValue;
				using (XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(Encoding.UTF8.GetBytes(eventArgument), quotas))
				{
					args = json.ReadObject(reader, true) as List<string>;
				}
			}
			catch (Exception ex)
			{
				Debug.Fail(ex.ToString());
				return null;
			}

			switch (args[0])
			{
				case "CloseMessage":
					return new CloseMessageAction(control.FormControl, (TBMessageControl)control );
				case "MoveTo":
					return new MoveToAction(control.FormControl, control.InnerControl.ID, args[1], args[2]);
				case "Cmd":
					return new DoCommandAction(control.FormControl, control, args[1], args[2], args[3]);
				case "SelectIdx":
					return new SelectIndexAction(control.FormControl, control, args[1], args[2], args[3]);
				case "DblClickItem":
					return new DblClickItemAction(control.FormControl, control, args[1], args[2], args[3]);
				case "NewRow":
					return new NewRowAction(control.FormControl, control, args[1], args[2], args[3]);
				case "BodyPage":
					return new MoveBodyPageAction(control.FormControl, control, args[1], args[2], args[3]);
				case "Ping":
					return new PingAction(control.FormControl);
				case "HotLink":
					return new HotLinkAction(control.FormControl, (TBHotLink)control, args[1], args[2], args[3]);
				case "CloseForm":
					return new CloseAction(control.FormControl, control, args[1], args[2]);
				case "Resize":
					return new ResizeAction(control.FormControl, control, args[1], args[2]);
				case "SelectNode":
					return new SelectNodeAction(control.FormControl, control, args[1], args[2]);
				case "ToggleNode":
					return new ToggleNodeAction(control.FormControl, control, args[1], args[2]);
				case "ChangeTab":
					return new TabChangedAction(control.FormControl, (TBTabber)control, args[1], args[2], args[3]);
				case "RadarSelect":
					return new RadarSelectAction(control.FormControl, control, args[3]);
				case "RadarMoveRow":
					return new RadarMoveRowAction(control.FormControl, control, args[3]);
				case "ClickMenu":
					return new ClickMenuAction(control.FormControl, control, args[1], args[2]);
				case "DoKeyDown":
					return new DoKeyDownAction(control.FormControl, control, args[1], args[2], args[3], args[4], args[5], args[6], args[7]);
				case "Spin":
					return new SpinAction(control.FormControl, (TBSpinControl)control, args[1], args[2], args[3]);
				case "DoLink":
					return new DoLinkAction(control.FormControl, control, args[1], args[2], args[3], args[4]);
				case "DoHyperLink":
					return new DoHyperLinkAction(control.FormControl, control);
				case "SelectRow":
					return new SelectRowAction(control.FormControl, control);
				case "ToggleExpandNode":
					return new ToggleExpandNodeAction(control.FormControl, control);	
				case "ResizeColumn":
					return  new ResizeColumnAction(control.FormControl, control, args[1]);
				case "UploadFile":
					return new UploadFileAction(control.FormControl, control, args[4], args[5], args[6]);
				
				default:
					return null;
			}
		}


		//--------------------------------------------------------------------------------------
		internal virtual TimeSpan GetTimeout()
		{
			return DefaultTimeout;
		}
	}

	//============================================================================================
	internal class RunDocumentAction : Action 
	{
		private static readonly TimeSpan RunDocumentTimeout = TimeSpan.FromSeconds(15);

		//--------------------------------------------------------------------------------------
		public RunDocumentAction(TBWebFormControl formControl)
			: base(formControl, "", "")
		{
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.RunDocument();
		}

		//--------------------------------------------------------------------------------------
		internal override TimeSpan GetTimeout()
		{
			return RunDocumentTimeout;
		}
	}

	//============================================================================================
	internal class AttachToDocumentAction : Action 
	{
		private int docId = 0;
		//--------------------------------------------------------------------------------------
		public AttachToDocumentAction(TBWebFormControl formControl, string docId)
			: base(formControl, "", "")
		{
			int.TryParse(docId, out this.docId);
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.AttachToDocument(docId);
		}


	}

	//============================================================================================
	internal class TabChangedAction : Action
	{
		TBTabber tabber;
		string tabId;
		
		//--------------------------------------------------------------------------------------
		public TabChangedAction(TBWebFormControl formControl, TBTabber tabber, string tabIndex, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			this.tabber = tabber;
			this.tabId = tabber.GetTabId(int.Parse(tabIndex));
			
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_ActivateTabPage(formControl.ProxyObjectId, fromIds, fromValue, tabber.InnerControl.ID, tabId);
		}

	}

	//============================================================================================
	internal class HotLinkAction : Action
	{
		TBHotLink hotLinkControl;
		bool lower;
		//--------------------------------------------------------------------------------------
		public HotLinkAction(TBWebFormControl formControl, TBHotLink hotLinkControl, string fromIds, string fromValue, string sLower)
			: base(formControl, fromIds, fromValue)
		{
			this.lower = sLower == "true";
			this.hotLinkControl = hotLinkControl;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoHotLink(formControl.ProxyObjectId, fromIds, fromValue, hotLinkControl.WindowId, lower);
		}

	}

	//============================================================================================
	internal class SpinAction : Action
	{
		TBSpinControl spinControl;
		bool lower;
		//--------------------------------------------------------------------------------------
		public SpinAction(TBWebFormControl formControl, TBSpinControl spinControl, string fromIds, string fromValue, string sLower)
			: base(formControl, fromIds, fromValue)
		{
			this.lower = sLower == "true";
			this.spinControl = spinControl;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoSpin(formControl.ProxyObjectId, fromIds, fromValue, spinControl.WindowId, lower);
		}

	}

	//============================================================================================
	internal class DoCommandAction : Action
	{
		private static readonly TimeSpan DoCommandTimeout = TimeSpan.FromSeconds(10);
		
		TBWebControl commandControl;
		int cmd;

		//--------------------------------------------------------------------------------------
		public DoCommandAction(TBWebFormControl formControl, TBWebControl buttonControl, string cmd, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			commandControl = buttonControl;
			if (!int.TryParse(cmd, out this.cmd))
				this.cmd = commandControl.CommandId;
		}

		//--------------------------------------------------------------------------------------
		internal override TimeSpan GetTimeout()
		{
			return DoCommandTimeout;
		}
		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			int res = 0;
			byte[] description = formControl.ActionService.WebProxyObj_DoCommand(formControl.ProxyObjectId, fromIds, fromValue, commandControl.CommandObject.WindowId, cmd, ref res);

			if (res == (int)WebCommandType.WEB_UNSUPPORTED)
				formControl.SetWarning(TBWebFormControlStrings.WebUnsupported);
			else if (res == (int)WebCommandType.WEB_LINK)
			{
				string url = formControl.ActionService.WebProxyObj_GetCommandLinkUrl(formControl.ProxyObjectId, commandControl.CommandObject.WindowId, cmd);
				//creo lo script javascript di inizializzazione del controllo
				commandControl.InitScriptBody = string.Format("window.open('{0}');", url);
				//aggiungo il controllo tra quelli che hanno lo script di init
				//(il caso specifico e' quello dell'help dal bottone della toolbar, registro qui il controllo per evitare che 
				//per ogni bottone della toolbar ci sia una chiamata inutile  alla RegisterInitControlScript)
				formControl.RegisterInitControlScript(commandControl);
				commandControl.Update();
			}
			return description;
		}


	}

	//============================================================================================
	internal class ClickMenuAction : Action
	{
		private static readonly TimeSpan ClickMenuTimeout = TimeSpan.FromSeconds(10);
		int itemID;
		string windowId;
		TBWebControl menuControl;

		//--------------------------------------------------------------------------------------
		public ClickMenuAction(TBWebFormControl formControl, TBWebControl menuControl, string windowId, string itemID)
			: base(formControl, "", "")
		{
			int.TryParse(itemID, out this.itemID);
			this.windowId = windowId;
			this.menuControl = menuControl;
		}

		//--------------------------------------------------------------------------------------
		internal override TimeSpan GetTimeout()
		{
			return ClickMenuTimeout;
		}
		
		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			int res = 0;
			byte[] description = formControl.ActionService.WebProxyObj_ClickContextMenu(formControl.ProxyObjectId, windowId, itemID, ref res);
			if (res == (int)WebCommandType.WEB_UNSUPPORTED)
				formControl.SetWarning(TBWebFormControlStrings.WebUnsupported);	
			else if (res == (int)WebCommandType.WEB_LINK)
			{
				string url = formControl.ActionService.WebProxyObj_GetCommandLinkUrl(formControl.ProxyObjectId,windowId,itemID);

				//creo lo script javascript di inizializzazione del controllo
				menuControl.InitScriptBody = string.Format("window.open('{0}');",url);
				//aggiungo il controllo tra quelli che hanno lo script di init
				//(il caso specifico e' quello dell'help dal bottone della toolbar, registro qui il controllo per evitare che 
				//per ogni bottone della toolbar ci sia una chiamata inutile  alla RegisterInitControlScript)
				formControl.RegisterInitControlScript(menuControl);
				menuControl.Update();
			}
			return description;
		}
	}

	//============================================================================================
	internal class CloseAction : Action
	{
		TBWebControl control;
		
		//--------------------------------------------------------------------------------------
		public CloseAction(TBWebFormControl formControl, TBWebControl control, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			this.control = control;
		}
		
		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoClose(formControl.ProxyObjectId, fromIds, fromValue, control.ID);
		}
	}

	//============================================================================================
	internal class ResizeAction : Action
	{
		TBWebControl control;
		int w;
		int h;

		//--------------------------------------------------------------------------------------
		public ResizeAction(TBWebFormControl formControl, TBWebControl control, string w, string h)
			: base(formControl, "", "")
		{
			int.TryParse(w, out this.w);
			int.TryParse(h, out this.h);
			this.control = control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			formControl.Invalidate();
			return formControl.ActionService.WebProxyObj_Resize(formControl.ProxyObjectId, control.WindowId, w, h);
		}
	}

	//============================================================================================
	internal class NewRowAction : Action
	{
		TBGridContainer bodyEditControl;
		int nCol = -1;
		//--------------------------------------------------------------------------------------
		public NewRowAction(TBWebFormControl formControl, TBWebControl bodyEditControl, string fromIds, string fromValue, string sCol)
			: base(formControl, fromIds, fromValue)
		{
			this.bodyEditControl = ((TBGridTable) bodyEditControl).TableContainer;
			int.TryParse(sCol, out nCol);
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_NewRow(formControl.ProxyObjectId, fromValue, fromIds, bodyEditControl.WindowId, nCol);
		}

	}

	//============================================================================================
	internal class MoveBodyPageAction : Action
	{
		TBGridContainer bodyEditControl;
		bool moveNext;

		//--------------------------------------------------------------------------------------
		public MoveBodyPageAction(TBWebFormControl formControl, TBWebControl bodyEditControl, string fromIds, string fromValue, string moveNext)
			: base(formControl, fromIds, fromValue)
		{
			this.bodyEditControl = ((TBGridTable)bodyEditControl).TableContainer;
			this.moveNext = bool.Parse(moveNext);
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			bodyEditControl.Update();
			return formControl.ActionService.WebProxyObj_DoMoveBodyPage(formControl.ProxyObjectId, moveNext, fromIds, fromValue, bodyEditControl.WindowId);
		}

	}

	//============================================================================================
	internal class MoveToAction : Action
	{
		string toIds;
	
		//--------------------------------------------------------------------------------------
		public MoveToAction(TBWebFormControl formControl, string toIds, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			this.toIds = formControl.GetServerId(toIds);
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_MoveTo(formControl.ProxyObjectId, fromIds, fromValue, toIds);
		}
		
	}

	//============================================================================================
	internal class SelectIndexAction : Action
	{
		TBWebControl control;
		int index;
		

		//--------------------------------------------------------------------------------------
		public SelectIndexAction(TBWebFormControl formControl, TBWebControl control, string index, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			int.TryParse(index, out this.index);
			this.control = control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_SelectIndex(formControl.ProxyObjectId, fromIds, fromValue, control.WindowId, index);
		}

	}

	//============================================================================================
	internal class DblClickItemAction : Action
	{
		TBWebControl control;
		int index;
		

		//--------------------------------------------------------------------------------------
		public DblClickItemAction(TBWebFormControl formControl, TBWebControl control, string index, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			int.TryParse(index, out this.index);
			this.control = control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DblClickItemAction(formControl.ProxyObjectId, fromIds, fromValue, control.WindowId, index);
		}

	}

	//============================================================================================
	internal class ToggleNodeAction : Action
	{
		TBTreeNode node;
		
		//--------------------------------------------------------------------------------------
		public ToggleNodeAction(TBWebFormControl formControl, TBWebControl control, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			node = (TBTreeNode)control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_ToggleNode(formControl.ProxyObjectId,fromValue,fromIds,node.TreeView.ID, node.WindowId);
		}
	}

	//============================================================================================
	internal class SelectNodeAction : Action
	{
		TBTreeNode node;
	
		//--------------------------------------------------------------------------------------
		public SelectNodeAction(TBWebFormControl formControl, TBWebControl control, string fromIds, string fromValue)
			: base(formControl, fromIds, fromValue)
		{
			node = (TBTreeNode)control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_SelectNode(formControl.ProxyObjectId, fromValue, fromIds, node.TreeView.ID, node.WindowId);
		}
	}

	//============================================================================================
	internal class DoKeyDownAction : Action
	{
		private static readonly TimeSpan DoKeyDownTimeout = TimeSpan.FromSeconds(10);
		TBWebControl control;
		int keyCode;
		bool shiftKey, ctrlKey, altKey;
		int selStart;
		int selEnd;
		string windowText;
		
		//--------------------------------------------------------------------------------------
		public DoKeyDownAction(
			TBWebFormControl formControl,
			TBWebControl control,
			string sKeyCode,
			string shiftKey,
			string ctrlKey,
			string altKey,
			string start,
			string end, 
			string text)
			: base(formControl, "", "")
		{
			this.control = control;
			int.TryParse(sKeyCode, out keyCode);
			bool.TryParse(shiftKey, out this.shiftKey);
			bool.TryParse(ctrlKey, out this.ctrlKey);
			bool.TryParse(altKey, out this.altKey);
			int.TryParse(start, out this.selStart);
			int.TryParse(end, out this.selEnd);
			this.windowText = text;
		}

		internal override TimeSpan GetTimeout()
		{
			return DoKeyDownTimeout;
		}
		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoKeyAction(formControl.ProxyObjectId, control.WindowId, keyCode, shiftKey, ctrlKey, altKey);
		}
	
	}


	///<summary>
	///Azione che selezione la riga con indice "sIndex", e quindi chiude il radar se nno e' "pinnato"
	/// </summary>
	//============================================================================================
	internal class RadarSelectAction : Action
	{
		TBWebControl control;
		int index;

		//--------------------------------------------------------------------------------------
		public RadarSelectAction(TBWebFormControl formControl, TBWebControl control, string sIndex)
			: base(formControl, "", "")
		{
			this.control = control;
			int.TryParse(sIndex, out index);

		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_SelectRadar(formControl.ProxyObjectId, control.WindowId, index);
		}
	}

	///<summary>
	///Azione che imposta come corrente (sposta visivamente la riga corrente ma non la seleziona chiudendo il radar)
	///la riga con indice "sIndex" 
	/// </summary>
	//============================================================================================
	internal class RadarMoveRowAction : Action
	{
		TBWebControl control;
		int index;

		//--------------------------------------------------------------------------------------
		public RadarMoveRowAction(TBWebFormControl formControl, TBWebControl control, string sIndex)
			: base(formControl, "", "")
		{
			this.control = control;
			int.TryParse(sIndex, out index);

		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_MoveRowRadar(formControl.ProxyObjectId, control.WindowId, index);
		}
	}

	//============================================================================================
	internal class DoLinkAction : Action
	{
		private static readonly TimeSpan DoLinkTimeout = TimeSpan.FromSeconds(15);
		TBWebControl control;
		int alias, row;

		//--------------------------------------------------------------------------------------
		public DoLinkAction(TBWebFormControl formControl, TBWebControl control, string fromIds, string fromValue, string sAlias, string sRow)
			: base(formControl, fromIds, fromValue)
		{
			this.control = control;
			int.TryParse(sAlias, out alias);
			int.TryParse(sRow, out row);
		}
		internal override TimeSpan GetTimeout()
		{
			return DoLinkTimeout;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoLink(formControl.ProxyObjectId, fromIds, fromValue, control.WindowId, alias, row);
		}
	}

	//============================================================================================
	internal class DoHyperLinkAction : Action
	{
		private static readonly TimeSpan DoHyperLinkTimeout = TimeSpan.FromSeconds(15);
		TBWebControl control;

		//--------------------------------------------------------------------------------------
		public DoHyperLinkAction(TBWebFormControl formControl, TBWebControl control)
			: base(formControl, "", "")
		{
			this.control = control;
		}
		internal override TimeSpan GetTimeout()
		{
			return DoHyperLinkTimeout;
		}
		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_DoHyperLinkAction(formControl.ProxyObjectId, control.WindowId);
		}
	}

	//============================================================================================
	internal class SelectRowAction : Action
	{
		TBWebControl control;

		//--------------------------------------------------------------------------------------
		public SelectRowAction(TBWebFormControl formControl, TBWebControl control)
			: base(formControl, "", "")
		{
			this.control = control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_SelectRowActionAction(formControl.ProxyObjectId, control.WindowId);
		}
	}

	//============================================================================================
	internal class ToggleExpandNodeAction : Action
	{
		TBWebControl control;

		//--------------------------------------------------------------------------------------
		public ToggleExpandNodeAction(TBWebFormControl formControl, TBWebControl control)
			: base(formControl, "", "")
		{
			this.control = control;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_BETreeToggleExpandNode(formControl.ProxyObjectId, control.WindowId);
		}
	}

	//============================================================================================
	internal class ResizeColumnAction : Action
	{
		TBWebControl control;
		int width;

		//--------------------------------------------------------------------------------------
		public ResizeColumnAction(TBWebFormControl formControl, TBWebControl control, string sWidth)
			: base(formControl, "", "")
		{
			this.control = control;
			int.TryParse(sWidth, out width);
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			formControl.Invalidate();
			return formControl.ActionService.WebProxyObj_ResizeColumnAction(formControl.ProxyObjectId, control.WindowId, width);
		}
	}


	//============================================================================================
	internal class UploadFileAction : Action
	{
		TBWebControl control;
		string fileFullPath;
	
		//--------------------------------------------------------------------------------------
		public UploadFileAction(TBWebFormControl formControl, TBWebControl control, string fileSize, string fileName, string fileContent)
			: base(formControl, "", "")
		{
			this.control = control;
			//remove mimetime from beginning of string, after the ,(comma) start the encoded 64 part
			string fileContentBase64 = fileContent.Substring(fileContent.IndexOf(',') + 1);
			byte[] stream = System.Convert.FromBase64String(fileContentBase64);

			try
			{
				string directoryPath = BasePathFinder.BasePathFinderInstance.GetWebProxyFilesPath();
				if (!Directory.Exists(directoryPath))
				{
					Directory.CreateDirectory(directoryPath);
				}
				fileFullPath = string.Format("{0}\\{1}", directoryPath, fileName);
				System.IO.FileStream fileStream =  new System.IO.FileStream(fileFullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				fileStream.Write(stream, 0, stream.Length);

				// close file stream
				fileStream.Close();
			}
			catch (Exception)
			{
			}

		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			int res = 0;
	
			byte[] description = formControl.ActionService.WebProxyObj_DoUploadFile(formControl.ProxyObjectId, fromIds, fromValue, control.ControlDescription.Id, control.CommandId, ref res, fileFullPath);

			if (res == (int)WebCommandType.WEB_UNSUPPORTED)
			{
				formControl.SetWarning(TBWebFormControlStrings.WebUnsupported);
			}
			return description;
		}
	}


	//============================================================================================
	class PingAction : Action
	{
		//--------------------------------------------------------------------------------------
		public PingAction(TBWebFormControl formControl)
			: base(formControl, "", "")
		{ 
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			return formControl.ActionService.WebProxyObj_GetThreadWindowsBinaryDescription(formControl.ProxyObjectId, formControl.ProxyObjectId);
		}
	}

	//============================================================================================
	class CloseMessageAction : Action
	{
		TBMessageControl messageControl;

		//--------------------------------------------------------------------------------------
		public CloseMessageAction(TBWebFormControl formControl, TBMessageControl messageControl)
			: base(formControl, "", "")
		{
			this.messageControl = messageControl;
		}

		//--------------------------------------------------------------------------------------
		public override byte[] Execute()
		{
			messageControl.DoClose();
			return formControl.ActionService.WebProxyObj_GetThreadWindowsBinaryDescription(formControl.ProxyObjectId, formControl.ProxyObjectId);
		}
	}
}
