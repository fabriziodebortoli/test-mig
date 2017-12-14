using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Model.TBCUI
{
	//=========================================================================
	public class TBCUIManager : TBCUI, ITBCUIManager
	{
		List<IUIDocumentPart> uiParts;
		IntPtr hostingHandle;

		//public TBCUIManager	()
		//{
		//    uiParts = new List<IUIDocumentPart>();

		//}
		
		//---------------------------------------------------------------------
		public TBCUIManager(IUIForm component)
			:
			base(component, NameSpaceObjectType.Form)
		{
			uiParts = new List<IUIDocumentPart>();
			UIManager = this;
		}

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			// serve a sganciare gli eventi 
			Document = null;

			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------
		public IUIToolbar PrimaryToolbar
		{
			get
			{
				IUIForm frame = (IUIForm)Component;
				return frame.PrimaryToolbar;
			}

		}
		
		//---------------------------------------------------------------------
		public IUIToolbar AuxiliaryToolbar
		{
			get
			{
				IUIForm frame = (IUIForm)Component;
				return frame.AuxiliaryToolbar;
			}
		}

		//---------------------------------------------------------------------
		public System.IntPtr HostingHandle { get { return hostingHandle; } set { hostingHandle = value; } }

		//---------------------------------------------------------------------
		public PrimaryToolbarStyle PrimaryToolBarStyle
		{
			set
			{
				IUIForm form = (IUIForm)Component;
				form.PrimaryToolbarStyle = value;
			}
		}

		//---------------------------------------------------------------------
		public AuxiliaryToolbarStyle AuxiliaryToolBarStyle
		{
			set
			{
				IUIForm form = (IUIForm)Component;
				form.AuxiliaryToolbarStyle = value;
			}
		}

		// document parts
		//---------------------------------------------------------------------
		public IUIDocumentPart InitDocumentPartUI(IDocumentPart part, System.String uiTypeName, IUIUserControl parent)
		{
			if (String.IsNullOrWhiteSpace(uiTypeName))
			{
				return null;
			}

			Type t = Type.GetType(uiTypeName);
			if (t == null)
			{
				Debug.Assert(false, "InitDocumentPart without a IUIDocumentPart object!!");
				return null;
			}

			IUIDocumentPart partUI = Activator.CreateInstance(t) as IUIDocumentPart;
			if (partUI == null)
			{
				Debug.Assert(false, "InitDocumentPart without a IUIDocumentPart object!!");
				return null;
			}

			((TBCUI)partUI.CUI).ParentChanged(parent, EventArgs.Empty);
			partUI.DocumentPart = part;
			partUI.MainUI = parent;
			uiParts.Add(partUI);

			return partUI;
		}

		//---------------------------------------------------------------------
		public virtual IUIDocumentPart GetDocumentPartUI
			(
				IDocumentPart part,
				System.String name,
				IUIUserControl parent
			)
		{
			IUIDocumentPart uiPart = GetDocumentPartUI(part);
			if (uiPart != null)
				return uiPart;

			uiPart = InitDocumentPartUI(part, CUtility.GetViewManagedTypeName(Document.TbHandle, part.Namespace), parent);

			if (uiPart == null)
				return uiPart;

			if (!String.IsNullOrEmpty(name))
			{
				uiPart.Name = name;
				// qui preferisco che si schianti piuttosto che non agganciare l'oggetto
				uiPart.CUI.Name = name;
			}
			uiPart.OnAdded();

			return uiPart;
		}

		//---------------------------------------------------------------------
		public void CallCreateComponents()
		{
			foreach (IUIDocumentPart uiPart in uiParts)
			{
				uiPart.CreateComponents();
			}
		}

		//---------------------------------------------------------------------
		public virtual void AddContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			IUIForm form = (IUIForm)Component;
			form.AddContextMenuItem(ownerControl, item);
		}

		//---------------------------------------------------------------------
		public virtual void RemoveContextMenuItem(IUIComponent ownerControl, IMenuItemGeneric item)
		{
			IUIForm form = (IUIForm)Component;
			form.RemoveContextMenuItem(ownerControl, item);
		}

		//---------------------------------------------------------------------
		IUIDocumentPart GetDocumentPartUI(IDocumentPart part)
		{
			foreach (IUIDocumentPart uiPart in uiParts)
			{
				if (uiPart.DocumentPart == part)
					return uiPart;
			}
			return null;
		}
		
	}
}
