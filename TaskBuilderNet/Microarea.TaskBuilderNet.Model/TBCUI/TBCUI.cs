using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Model.TBCUI
{
	//=========================================================================
	public class TBCUI : ITBCUI
	{
		ITBSecurityProvider security;
		IMAbstractFormDoc	document;
		IUIComponent		component;
		ITBCUIManager		uiManager;
		ITBCUI				parentCUI;
		ISite				site;
		
		public event EventHandler Disposed;

		NameSpaceObjectType nameSpaceType;

		public virtual IMAbstractFormDoc	Document	{ get { return document; } set { document = value; } }
		public	IUIComponent				Component	{ get { return component; } set { component = value; } }
		public	ITBCUIManager				UIManager	{ get { return uiManager; } set { uiManager = value; } }
		public	ISite						Site		{ get { return site; } set { site = value; } }
		public bool							IsEnabled	{ get { return Security.IsEnabled; } }
		public bool							IsVisible	{ get { return Security.IsVisible; } }
		public INameSpace					Namespace	{ get { return Security != null ? Security.Namespace : null; } }
		public string						Name		{ get { return Component.Name; } set { Security.Name = value; } }

		//---------------------------------------------------------------------
		public ITBSecurityProvider Security
		{
			get
			{
				if (security == null)
				{
					INameSpace parentNs = null;
					if (parentCUI != null)
					{
						parentNs = parentCUI.Namespace;
					}
					else if (Document != null)
					{
						parentNs = Document.Namespace;
					}

					if (parentNs != null)
					{
						security = new TBSecurityProvider(Document, nameSpaceType, parentNs, Name);
					}
				}

				return security;
			}
		}

		//---------------------------------------------------------------------
		public TBCUI(IUIComponent component, NameSpaceObjectType nameSpaceType)
		{
			this.component = component;
			this.nameSpaceType = nameSpaceType;
			this.component.ParentChanged += new EventHandler(ParentChanged);
		}

		//---------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			if (this.component != null)
			{
				this.component.ParentChanged -= new EventHandler(ParentChanged);
			}

			if (this.security != null)
			{
				security.Dispose();
				security = null;
			}

			Document = null;
			Component = null;
		}

		//---------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);

			if (Disposed != null)
				Disposed(this, EventArgs.Empty);

			GC.SuppressFinalize(this);
		}

		//---------------------------------------------------------------------
		public virtual void ParentChanged(object sender, EventArgs e)
		{
			IUIContainer theSender = sender as IUIContainer;
			IUIComponent parent = theSender == null || theSender.CUI.Document == null ? GetParent() : theSender;
			if (parent == null)
				return;

			// deve nascere alla prima istanza
			Document = parent.CUI.Document;
			UIManager = parent.CUI.UIManager;
			parentCUI = parent.CUI;

			IUIContainer uiContainer = Component as IUIContainer;
			if (uiContainer != null)
				RecursiveAttach(uiContainer.ChildControls);
		}

		//---------------------------------------------------------------------
		public virtual void SetModified()
		{
			if (Document != null)
				Document.Modified = true;
		}

		//---------------------------------------------------------------------
		public void RecursiveAttach(IList collection)
		{
			// TODO vedere come migliorare per uso di Control
			foreach (object c in collection)
			{
				IUIComponent uiComponent = c as IUIComponent;
				if (uiComponent != null)
				{
					((TBCUI)uiComponent.CUI).ParentChanged(null, EventArgs.Empty);
				}

				IUIContainer uiContainer = c as IUIContainer;
				if (uiContainer == null)
				{
					Control ctrl = c as Control;
					if (ctrl != null)
					{
						RecursiveAttach(ctrl.Controls);
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private IUIComponent GetParent()
		{
			Control me = Component as Control;
			Control parent = me == null ? null : me.Parent;

			while (parent != null)
			{
				IUIComponent component = parent as IUIComponent;
				if (component != null)
					return component;

				parent = parent.Parent;
			}
			return null;
		}
	}
}
