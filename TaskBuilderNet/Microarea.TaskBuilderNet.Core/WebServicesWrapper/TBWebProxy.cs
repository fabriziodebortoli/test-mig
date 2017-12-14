using System;
using Microarea.TaskBuilderNet.Core.SoapCall;
using Microarea.TaskBuilderNet.Core.TbGesInterface;

namespace Microarea.TaskBuilderNet.Core.WebServicesWrapper
{
	//================================================================================
	public class TBWebProxy : IDisposable
	{
		TbGesClient tbGes;
		Microarea.TaskBuilderNet.Core.TbGesInterface.TBHeaderInfo tbGesHeader;

		//--------------------------------------------------------------------------------
		public bool Available
		{
			get
			{
				return TbLoaderClientInterface.IsAvailable<TbGes>(tbGes);
			}
		}

		//--------------------------------------------------------------------------------
		public TBWebProxy(TbGesClient tbGes, Microarea.TaskBuilderNet.Core.TbGesInterface.TBHeaderInfo tbGesHeader)
		{
			this.tbGes = tbGes;
			this.tbGesHeader = tbGesHeader;
		}

		//--------------------------------------------------------------------------------
		public int WebProxyObjCreate(ref int threadId)
		{
			return tbGes.CreateWebProxy(ref tbGesHeader, ref threadId);
		}
		
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObjCreateForThread(int threadId, ref int handle)
		{
			byte[] description = null;
			handle = tbGes.CreateWebProxyForThread(ref tbGesHeader, threadId, ref description);
			return description;
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_ActivateTabPage(int handle, string fromId, string fromValue,string windowId, string tabId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_ActivateTabPageAction(ref tbGesHeader, handle, fromId, fromValue, windowId, tabId, ref description);
			return description;
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_Resize(int handle, string windowId, int w, int h)
		{
			byte[] description = null;
			tbGes.WebProxyObj_ResizeAction(ref tbGesHeader, handle, windowId, w, h, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_MoveTo(int handle, string fromId, string fromValue, string toId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_MoveToAction(ref tbGesHeader, handle, fromId, fromValue, toId, ref description);
			return description;


		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_SelectRadar(int handle, string windowId, int index)
		{
			byte[] description = null;
			tbGes.WebProxyObj_SelectRadarAction(ref tbGesHeader, handle, windowId, index, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_MoveRowRadar(int handle, string windowId, int index)
		{
			byte[] description = null;
			tbGes.WebProxyObj_MoveRowRadarAction(ref tbGesHeader, handle, windowId, index, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public string WebProxyObj_GetCommandLinkUrl(int handle, string windowId, int commandId)
		{
			return tbGes.WebProxyObj_GetCommandLinkUrl(ref tbGesHeader, handle, windowId, commandId);
		}

		//-----------------------------------------------------------------------
		public bool ExistDocument(int handle)
		{
			return tbGes.ExistDocument(ref tbGesHeader, handle);
		}

	
		//-----------------------------------------------------------------------
		public virtual bool IsLoginValid()
		{
			try { return tbGes.IsLoginValid(ref tbGesHeader);} catch { return false; }
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoCommand(int handle, string fromId, string fromValue, string windowId, int commandId, ref int commandType)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoCommandAction(ref tbGesHeader, handle, fromId, fromValue, windowId, commandId, ref commandType, ref description);
			return description;
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoUploadFile(int handle, string fromId, string fromValue, string windowId, int commandId, ref int commandType, string fileFullPath)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoUploadFileAction(ref tbGesHeader, handle, fromId, fromValue, windowId, commandId, ref commandType, fileFullPath, ref description);
			return description;
		}

		//--------------------------------------------------------------------------------
		public void WebProxyObj_OpenDropdownBtn(int handle, string windowId, int commandId)
		{
			tbGes.WebProxyObj_OpenDropdownBtn(ref tbGesHeader, handle, windowId, commandId);

		}

		//--------------------------------------------------------------------------------
		public void WebProxyObj_GetThreadContextMessages(int handle, bool clearMessages, ref string[] messages, ref int[] types)
		{
			tbGes.WebProxyObj_GetThreadContextMessages(ref tbGesHeader, handle, clearMessages, ref messages, ref types);
		}
		//--------------------------------------------------------------------------------
		public void GetLoginContextMessages(bool clearMessages, ref string[] messages, ref int[] types)
		{
			tbGes.GetLoginContextMessages(ref tbGesHeader, clearMessages, ref messages, ref types);
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_NewRow(int handle, string fromValue, string fromId, string windowId, int nCol)
		{
			byte[] description = null;
			tbGes.WebProxyObj_NewRowAction(ref tbGesHeader, handle, fromId, fromValue, windowId, nCol, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoHotLink(int handle, string fromId, string fromValue, string windowId, bool lower)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoHotLinkAction(ref tbGesHeader, handle, fromId, fromValue, windowId, lower, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoSpin(int handle, string fromId, string fromValue, string windowId, bool lower)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoSpinAction(ref tbGesHeader, handle, fromId, fromValue, windowId, lower, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public void WebProxyObj_DoContextMenu(int handle, string windowId)
		{
			tbGes.WebProxyObj_DoContextMenu(ref tbGesHeader, handle, windowId);
		}
		//--------------------------------------------------------------------------------
		public bool WebProxyObj_GetContextMenu(int handle, ref byte[] menuDescription)
		{
			return tbGes.WebProxyObj_GetContextMenu(ref tbGesHeader, handle, ref menuDescription);
		}

		//--------------------------------------------------------------------------------
		public bool WebProxyObj_GetTreeViewAdvContextMenu(int handle,ref byte[] menuDescription, string handleTree)
		{
			return tbGes.WebProxyObj_GetTreeViewAdvContextMenu(ref tbGesHeader,handle,ref menuDescription, handleTree);
		}
		

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_ClickContextMenu(int handle, string windowId, int commandId, ref int commandType)
		{
			byte[] description = null;
			tbGes.WebProxyObj_ClickContextMenuAction(ref tbGesHeader, handle, windowId, commandId, ref commandType, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoClose(int handle, string fromId, string fromValue, string windowId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoCloseAction(ref tbGesHeader, handle, fromId, fromValue, windowId, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_GetThreadWindowsBinaryDescription(int ProxyObjectId, int MainDocId)
		{
			byte[] description = new byte[0];
			try
			{ 
				tbGes.WebProxyObj_GetThreadWindowsBinaryDescription(ref tbGesHeader, ProxyObjectId, ref description);
			}
			catch
			{
				description = new byte[0];
			}

			return description;
		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_SelectIndex(int handle, string fromId, string fromValue, string windowId, int index)
		{
			byte[] description = null;
			tbGes.WebProxyObj_SelectIndexAction(ref tbGesHeader, handle, fromId, fromValue, windowId, index, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DblClickItemAction(int handle, string fromId, string fromValue, string windowId, int index)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DblClickItemAction(ref tbGesHeader, handle, fromId, fromValue, windowId, index, ref description);
			return description;

		}
		

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_ToggleNode(int handle, string fromId, string fromValue, string treeID, string sItem)
		{
			byte[] description = null;
			tbGes.WebProxyObj_ToggleNodeAction(ref tbGesHeader, handle, fromId, fromValue, treeID, sItem, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_SelectNode(int handle, string fromId, string fromValue, string treeID, string sItem)
		{
			byte[] description = null;
			tbGes.WebProxyObj_SelectNodeAction(ref tbGesHeader, handle, fromId, fromValue, treeID, sItem, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public void WebProxyObj_DoKey(int handle, string windowId, int keyCode, bool shift, bool ctrl, bool alt, bool keyIsChar, ref int selStart, ref int selEnd, ref string windowText)
		{
			tbGes.WebProxyObj_DoKey(ref tbGesHeader, handle, windowId, keyCode, shift, ctrl, alt, keyIsChar, ref selStart, ref selEnd, ref windowText);
		}
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoKeyAction(int handle, string windowId, int keyCode, bool shift, bool ctrl, bool alt)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoKeyAction(ref tbGesHeader, handle, windowId, keyCode, shift, ctrl, alt, ref description);
			return description;

		}
		//--------------------------------------------------------------------------------
		public string[] WebProxyObj_GetComboItems(int handle, string windowId, out int selectedIndex)
		{
			selectedIndex = -1;
			return tbGes.WebProxyObj_GetComboItems(ref tbGesHeader, handle, windowId, ref selectedIndex);
		}
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoLink(int handle, string fromId, string fromValue, string windowId, int alias, int row)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoLinkAction(ref tbGesHeader, handle, fromId, fromValue, windowId, alias, row, ref description);
			return description;

		}
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoHyperLinkAction(int handle, string windowId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoHyperLinkAction(ref tbGesHeader, handle, windowId, ref description);
			return description;

		}
		//--------------------------------------------------------------------------------
		public string WebProxyObj_SetDate(int handle, string windowId, int day, int month, int year)
		{
			return tbGes.WebProxyObj_SetDate(ref tbGesHeader, handle, windowId, day, month, year);
		}
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_SelectRowActionAction(int handle, string windowId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_SelectRowAction(ref tbGesHeader, handle, windowId, ref description);
			return description;

		}

		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_BETreeToggleExpandNode(int handle, string windowId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_BETreeToggleExpandNodeAction(ref tbGesHeader, handle, windowId, ref description);
			return description;

		}
		
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_ResizeColumnAction(int handle, string windowId, int width)
		{
			byte[] description = null;
			tbGes.WebProxyObj_ResizeColumnAction(ref tbGesHeader, handle, windowId, width, ref description);
			return description;

		}
		
		//--------------------------------------------------------------------------------
		public byte[] WebProxyObj_DoMoveBodyPage(int handle, bool next, string fromWindow, string fromValue, string windowId)
		{
			byte[] description = null;
			tbGes.WebProxyObj_DoMoveBodyPageAction(ref tbGesHeader, handle, next, fromWindow, fromValue, windowId, ref description);
			return description;

		}
		//-----------------------------------------------------------------------
		public byte[] RunDocument(int handle, string command, ref int docHandle)
		{
			byte[] description = null;
			tbGes.WebProxyObj_RunDocumentAction(ref tbGesHeader, handle, command, ref docHandle, ref description);
			return description;

		}
		//-----------------------------------------------------------------------
		public byte[] RunFunction(int handle, string command)
		{
			byte[] description = null;
			tbGes.WebProxyObj_RunFunctionAction(ref tbGesHeader, handle, command, ref description);
			return description;

		}
		//-----------------------------------------------------------------------
		public byte[] RunReport(int handle, string command, ref int docHandle)
		{
			byte[] description = null;
			tbGes.WebProxyObj_RunReportAction(ref tbGesHeader, handle, command, ref docHandle, ref description);
			return description;

		}

		//-----------------------------------------------------------------------
		public void SetUserInteractionMode(int handle, int mode)
		{
			tbGes.WebProxyObj_SetUserInteractionMode(ref tbGesHeader, handle, mode);
		}

		//-----------------------------------------------------------------------
		public void DestroyDocument(int id)
		{
			tbGes.DestroyDocument(ref tbGesHeader, id);

		}

		//--------------------------------------------------------------------------------
		public void SetTimeout(TimeSpan timeSpan)
		{
			WCFSoapClient.SetTimeoutToBinding(tbGes.Endpoint.Binding, timeSpan);
		}

		//--------------------------------------------------------------------------------
		public void Dispose()
		{
			try
			{
				if (tbGes != null)
				{
					tbGes.Close();
					tbGes = null;
				}
			}
			catch
			{
			}
		}


		
	}
}
