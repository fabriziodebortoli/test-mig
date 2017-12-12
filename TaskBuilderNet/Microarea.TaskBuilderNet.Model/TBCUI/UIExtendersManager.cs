using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Model.TBCUI
{
	//=========================================================================
	public abstract class UIExtendersManager : IUIExtendersManager
	{	
		int								lastWidth;
		int								spaceBetweenExtender;
		bool							dataBindingEventsWired;
		TBCUIControl					controller;
		List<UIControlBag>				extensionControls;
		IList<ITBUIExtenderProvider>	uiExtenders;

		//---------------------------------------------------------------------
		protected UIExtendersManager(TBCUIControl controller)
		{
			this.spaceBetweenExtender = 1;
			this.controller = controller;
			this.extensionControls = new List<UIControlBag>();
			this.uiExtenders = new List<ITBUIExtenderProvider>();
		}
		
		//---------------------------------------------------------------------
		public void  Dispose()
		{
 			if (this.controller != null)
			{
				this.UnwireEvents();
				this.UnwireDataBindingEvents();
				this.controller = null;
			}
			if (this.extensionControls != null && this.extensionControls.Count > 0)
			{
				this.extensionControls.Clear();
				this.extensionControls = null;
			}
			if (this.uiExtenders != null && this.uiExtenders.Count > 0)
			{
				foreach (ITBUIExtenderProvider provider in uiExtenders)
				{
					provider.Dispose();
				}
				this.uiExtenders.Clear();
				this.uiExtenders = null;
			}
		}

		//---------------------------------------------------------------------
		public virtual void ClearExtenders ()
		{
			if (this.uiExtenders == null || this.uiExtenders.Count == 0)
				return;

			this.UnwireEvents();
			this.UnwireDataBindingEvents();

			IUIControl extendee = this.Extendee;
			int  left = 0;
			int  right = 0;
			foreach (UIControlBag  bag in this.extensionControls)
			{
				switch (bag.Extender.Position)
				{
					case UIExtenderPosition.Left:
					{
						left += bag.ExtenderControl.Size.Width + spaceBetweenExtender;
						break;
					}
					case UIExtenderPosition.Right:
					{
						right += bag.ExtenderControl.Size.Width + spaceBetweenExtender;
						break;
					}
				}
				RemoveExtender(bag);
			}

			if (left != 0 || right != 0)
			{
				Size  size = extendee.Size;
				Point  location = extendee.Location;

				this.SetSizeAndLocation(new Size(size.Width + left + right, size.Height),new Point(location.X - left, location.Y));
			}
			this.extensionControls.Clear();
			this.uiExtenders.Clear();
		}

		//---------------------------------------------------------------------
		public virtual void AddExtender (ITBUIExtenderProvider extender)
		{
			EnsureUniqueProviderType(extender);
			if (!this.uiExtenders.Contains(extender))
			{
				this.uiExtenders.Add(extender);
				this.ExtendUI(extender);
			}
		}
		
		//---------------------------------------------------------------------
		public virtual void RemoveExtender (ITBUIExtenderProvider  extender)
		{
			IList<ITBUIExtenderProvider>	uiExtendersClone = new List<ITBUIExtenderProvider>(this.uiExtenders);
			ClearExtenders();

			uiExtendersClone.Remove(extender);
			foreach (IMenuItemGeneric item in extender.GetContextMenuItems())
			{
				controller.RemoveContextMenuItem(item);
			}

			foreach (ITBUIExtenderProvider current in uiExtendersClone)
			{
				AddExtender(current);
			}
		}
		
		//---------------------------------------------------------------------
		public virtual bool CanEnableExtendee ()
		{
			bool  enabled = true;
			foreach (ITBUIExtenderProvider  provider in this.uiExtenders)
			{
			   enabled &= provider.CanEnableExtendee();
			}
			return enabled;
		}

		//---------------------------------------------------------------------
		private IUIControl Extendee { get { return (this.controller != null) ? this.controller.Control : null; } }

		//---------------------------------------------------------------------
		private void  Data_ReadOnlyChanged		(object  sender, EasyBuilderEventArgs  e)
		{
			MDataObj obj = sender as MDataObj;
			if (obj != null)
			{
				bool readOnly = obj.ReadOnly;
				foreach (ITBUIExtenderProvider  provider in this.uiExtenders)
				{
					provider.OnDataReadOnlyChanged(readOnly);
				}
			}
		}

		//---------------------------------------------------------------------
		private void  Document_FormModeChanged	(object  sender, EventArgs  e) 
		{
			if (
				this.controller == null ||
				this.controller.Document == null
				)
			{
				return;
			}
			FormModeType formMode = this.controller.Document.FormMode;
			foreach (ITBUIExtenderProvider  provider in this.uiExtenders)
			{
				provider.OnFormModeChanged(formMode);
			}
		}
		
		//---------------------------------------------------------------------
		private void  Extendee_EnabledChanged	(object  sender, EventArgs  e)
		{
			IUIControl control = sender as IUIControl;
			if (control != null)
			{
				bool  enabled = control.Enabled;
				foreach (ITBUIExtenderProvider  provider in this.uiExtenders)
				{
					provider.OnControlEnabledChanged(enabled);
				}
			}
		}
		
		//---------------------------------------------------------------------
		private void  Extendee_LocationChanged	(object  sender, EventArgs  e)
		{
			this.LayoutExtenders();
		}

		//---------------------------------------------------------------------
		private void  Extendee_SizeChanged		(object  sender, EventArgs  e)
		{
			Size   size		= this.Extendee.Size;

			if (this.lastWidth != size.Width)
			{
				this.LayoutExtenders();
				this.lastWidth = size.Width;
			}
		}

		//---------------------------------------------------------------------
		private void  Extendee_VisibleChanged	(object  sender, EventArgs  e)
		{
		    if (this.extensionControls.Count != 0)
			{
				IUIControl control = sender as IUIControl;
				if (control == null)
				{
					Debug.Assert(false, "OOOPS!!!\'sender\' is not an IUIControl, is it right?");
				}
				else
				{
					foreach (UIControlBag  bag in this.extensionControls)
					{
						bag.ExtenderControl.Visible = control.Visible;
					}
				}
			}
		}

		//---------------------------------------------------------------------
		private void  Extendee_KeyDown			(object  sender, KeyEventArgs e)
		{
			foreach (ITBUIExtenderProvider  provider in this.uiExtenders)
			{
				provider.OnShortcutKey(e.KeyData);
			}
		}

		//---------------------------------------------------------------------
		private void  UnwireDataBindingEvents	()
		{
			 IMAbstractFormDoc  document = this.controller.Document;
			if (document != null)
			{
				document.FormModeChanged -= new EventHandler<EventArgs>(Document_FormModeChanged);
			}
			MDataObj obj2 = this.controller.DataBinding != null ? this.controller.DataBinding.Data as MDataObj : null;
			if (obj2 != null)
			{
				obj2.ReadOnlyChanged -= new EventHandler<EasyBuilderEventArgs>(Data_ReadOnlyChanged);
			}
			dataBindingEventsWired = false;
		}

		//---------------------------------------------------------------------
		private void  UnwireEvents				()
		{
			IUIControl  extendee = this.Extendee;
			if (extendee != null)
			{
				extendee.LocationChanged -= new EventHandler (Extendee_LocationChanged);
				extendee.SizeChanged -= new EventHandler (Extendee_SizeChanged);
				extendee.VisibleChanged -= new EventHandler (Extendee_VisibleChanged);
				extendee.EnabledChanged -= new EventHandler (Extendee_EnabledChanged);
				extendee.KeyDown -= new KeyEventHandler(Extendee_KeyDown);
			}
		}

		//---------------------------------------------------------------------
		private void  WireDataBindingEvents		()
		{
			IMAbstractFormDoc  document = this.controller.Document;
			if (document != null)
			{
				document.FormModeChanged += new EventHandler<EventArgs> (Document_FormModeChanged);
			}
			MDataObj obj2 = this.controller.DataBinding != null ? this.controller.DataBinding.Data as MDataObj : null;
			if (obj2 != null)
			{
				obj2.ReadOnlyChanged += new EventHandler<EasyBuilderEventArgs> (Data_ReadOnlyChanged);
			}
		}

		//---------------------------------------------------------------------
		private void  WireEvents()
		{
			 IUIControl  extendee = this.Extendee;
			if (extendee != null)
			{
				extendee.LocationChanged += new EventHandler (Extendee_LocationChanged);
				extendee.SizeChanged += new EventHandler (Extendee_SizeChanged);
				extendee.VisibleChanged += new EventHandler (Extendee_VisibleChanged);
				extendee.EnabledChanged += new EventHandler (Extendee_EnabledChanged);
				extendee.KeyDown += new KeyEventHandler(Extendee_KeyDown);
			}
		}
		
		//---------------------------------------------------------------------
		private UIExtenderPosition ExtendUI (ITBUIExtenderProvider  provider)
		{
			if (provider == null)
			{
				return UIExtenderPosition.None;
			}

			if (!dataBindingEventsWired)
			{
				WireDataBindingEvents();
				dataBindingEventsWired = true;
			}

			IUIControl  control = provider.CreateExtenderUIControl();
			foreach (IMenuItemGeneric item in provider.GetContextMenuItems())
			{
				controller.AddContextMenuItem(item);
			}
			if (control == null)
			{
				return UIExtenderPosition.None;
			}
    
			AddSibling(Extendee, control);
    
			UIControlBag  uiControlBag = new UIControlBag(control, provider);
			this.extensionControls.Add(uiControlBag);

			this.LayoutExtender(provider);

			return provider.Position;
		}

		
		//---------------------------------------------------------------------
		private void LayoutExtenders()
		{
			if (this.extensionControls.Count == 0)
				return;

			int  left = 0;
			int  right = 0;
			foreach (UIControlBag  bag in this.extensionControls)
			{
				switch (bag.Extender.Position)
				{
					case UIExtenderPosition.Left:
					{
						left += bag.ExtenderControl.Size.Width + spaceBetweenExtender;
						break;
					}
					case UIExtenderPosition.Right:
					{
						right += bag.ExtenderControl.Size.Width + spaceBetweenExtender;
						break;
					}
				}
			}

			left		= -left;
			right		= -right;
		
			foreach (UIControlBag  bag in extensionControls)
			{
				UpdateRightAndLeftExtender(bag, ref left, ref right);
				UpdateOverExtender(bag);
			}
		}

		//---------------------------------------------------------------------
		private void LayoutExtender(ITBUIExtenderProvider  provider)
		{
			 UIControlBag bag = GetBag(provider);
			if (bag == null)
			{
				Debug.Assert(false, "Chiamata LayoutExtender(provider) senza aver prima chiamato la ExtendUI(provider)");
				return;
			}

			switch (provider.Position)
			{
				case UIExtenderPosition.Over:
				{
					UpdateOverExtender(bag);
					break;
				}
				case UIExtenderPosition.Left:
				case UIExtenderPosition.Right:
				{
					int  left = 0;
					int  right = 0;
					UpdateRightAndLeftExtender(bag, ref left, ref right);
	
					if (left != 0 || right != 0)
					{
						IUIControl extendee = this.Extendee;
						Point  location = extendee.Location;
						Size  size = extendee.Size;

    					this.SetSizeAndLocation(new Size(size.Width - left - right, size.Height), new Point(location.X + left, location.Y));
					}
					break;
				}
			}
		}

		
		//---------------------------------------------------------------------
		private void  SetSizeAndLocation(Size  newSize, Point newLocation)			
		{
			IUIControl  extendee = this.Extendee;

			Size  minimumSize = extendee.MinimumSize;
			if (minimumSize != Size.Empty && (newSize.Width < minimumSize.Width || newSize.Height < minimumSize.Height))
			{
				Debug.Assert(false,"Attenzione!Sto impostando una Size minore della MinimumSize specificata per il control perche` gli sono stati appiccicati troppi extender, e` voluto?");
				extendee.MinimumSize = newSize;
			}

			this.UnwireEvents();
			extendee.Size = newSize;
			extendee.Location = newLocation;
			this.WireEvents();

			UpdateOverExtender();

			this.lastWidth = newSize.Width;
		}

		
		//---------------------------------------------------------------------
		private UIControlBag GetBag(ITBUIExtenderProvider provider)
		{
			foreach (UIControlBag  bag in this.extensionControls)
			{
				if (bag.Extender == provider)
					return bag;
			}
			return null;
		}

		//---------------------------------------------------------------------
		private void UpdateOverExtender()
		{
			foreach (UIControlBag  bag in this.extensionControls)
			{
				UpdateOverExtender(bag);
			}
		}

		//---------------------------------------------------------------------
		private void UpdateOverExtender(UIControlBag bag)
		{
			IUIControl  extendee = this.Extendee;
			if (bag.Extender.Position == UIExtenderPosition.Over)
			{
				IUIControl extensionControl= bag.ExtenderControl;
				extensionControl.Size		= extendee.Size;
				extensionControl.Location	= extendee.Location;
			}
		}

		//---------------------------------------------------------------------
		private void UpdateRightAndLeftExtender(UIControlBag bag, ref int left, ref int right)
		{
			if (bag == null || bag.Extender == null)
				return;

			UIExtenderPosition position = bag.Extender.Position;

			if (
				position != UIExtenderPosition.Right &&
				position != UIExtenderPosition.Left
				)
				return;

			IUIControl control = bag.ExtenderControl;
			IUIControl extendee = this.Extendee;

			Point  location = extendee.Location;
			Size  size = extendee.Size;

			switch (position)
			{
				case UIExtenderPosition.Left:
				{
					control.Location = new Point(location.X + left, location.Y);
					left = left + control.Size.Width + spaceBetweenExtender;
					break;
				}
				case UIExtenderPosition.Right:
				{
					right = right + control.Size.Width + spaceBetweenExtender;
					control.Location = new Point(location.X + size.Width - right, location.Y);
					break;
				}
			}
		}

		
		//---------------------------------------------------------------------
		[Conditional("DEBUG")]
		private void EnsureUniqueProviderType(ITBUIExtenderProvider  provider)
		{
			if (provider.Position == UIExtenderPosition.Over)
			{
				foreach (ITBUIExtenderProvider extenderProvider in this.uiExtenders)
				{
					if (extenderProvider.Position == UIExtenderPosition.Over)
					{
						System.Diagnostics.Debug.Assert(false, "E' gia` presente un provider con posizione Over");
					}
				}
			}
			else if (provider.Position != UIExtenderPosition.None)
			{
				Type newProviderType = provider.GetType();
				foreach (ITBUIExtenderProvider extenderProvider in this.uiExtenders)
				{
					if (extenderProvider.GetType() == newProviderType)
					{
						System.Diagnostics.Debug.Assert(false, "E' gia` presente un provider dello stesso tipo");
					}
				}
			}
		}
		
		//---------------------------------------------------------------------
		private void RemoveExtender(UIControlBag bag)
		{
			//Rimuovo i control che il provider ha attaccato al control da estendere
			RemoveSibling(this.Extendee, bag.ExtenderControl);
	
			//Distruggo i control che il provider ha attaccato al control da estendere
			bag.Extender.DestroyExtenderUIControl();
			bag.Extender.Dispose();
		}

		protected abstract void AddSibling		(IUIControl control, IUIControl newSibling);
		protected abstract void RemoveSibling	(IUIControl control, IUIControl sibling);
}

	//=========================================================================
	class UIControlBag
	{
		IUIControl control;
		ITBUIExtenderProvider extender;
	
		//---------------------------------------------------------------------
		public UIControlBag(IUIControl control, ITBUIExtenderProvider extender)
		{
			this.control = control;
			this.extender = extender;
		}

		//---------------------------------------------------------------------
		public IUIControl ExtenderControl
		{
			get 
			{
				return control;
			}
			set
			{
				control = value;
			}
		}

		//---------------------------------------------------------------------
		public ITBUIExtenderProvider Extender
		{
			get
			{
				return extender;
			}
			set
			{
				extender = value;
			}
		}
	};
}
