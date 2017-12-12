using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.EasyBuilder.ComponentModel
{
	//================================================================================
	class TBBaseSite : ITBSite, IDisposable
    {
        string name;
        IComponent component;
        IContainer container;

        public string Name { get { return name; } set { name = value; } }
        public IComponent Component { get { return component; } set { component = value; } }
        public IContainer Container { get { return container; } set { container = value; } }

        [ThreadStatic]
        static Hashtable dictionary;

        //--------------------------------------------------------------------------------
        public static Hashtable Dictionary
        {
            get
            {
                if (dictionary == null)
                    dictionary = new Hashtable();
                return dictionary;
            }
        }
        
		//--------------------------------------------------------------------------------
        public TBBaseSite
            (
            IComponent component,
            IContainer container,
            string name
            )
        {
            this.component = component;
            this.container = container;
            this.name = name;
        }

        //--------------------------------------------------------------------------------
        protected TBBaseSite(TBBaseSite tbSite)
		{
            if (tbSite == null)
                throw new ArgumentNullException("tbsite");
			this.component = tbSite.component;
			this.container = tbSite.container;
			this.name = tbSite.name;
		}


        #region ITBSite Members

        //--------------------------------------------------------------------------------
        public virtual ITBSite CloneChild(IComponent component, string name)
        {
            TBBaseSite site = new TBBaseSite(component, Container, name);

            if (this.Component is IContainer)
                site.Container = this.Component as IContainer;
            return site;
        }

        #endregion

        #region ISite Members
        //--------------------------------------------------------------------------------
        public bool DesignMode
        {
            get { return true; }
        }

        #endregion

        #region IServiceProvider Members

        //--------------------------------------------------------------------------------
        public virtual object GetService(Type serviceType)
        {
            return null;
        }

        #endregion

        #region IDictionaryService Members

        //--------------------------------------------------------------------------------
        public object GetKey(object value)
        {
            foreach (DictionaryEntry entry in Dictionary)
            {
                if (entry.Value == value)
                    return entry.Key;
            }

            return null;
        }

        //--------------------------------------------------------------------------------
        public object GetValue(object key)
        {
            return Dictionary[key];
        }

        //--------------------------------------------------------------------------------
        public void SetValue(object key, object value)
        {
            Dictionary[key] = value;
        }

        #endregion

        #region ICloneable Members

        //--------------------------------------------------------------------------------
        public virtual object Clone()
        {
            return new TBBaseSite(this);
        }

        #endregion
        #region IDisposable Members

        //--------------------------------------------------------------------------------
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        //--------------------------------------------------------------------------------
        protected virtual void Dispose(bool disposing)
        {
           
        }

        #endregion
    }

	//================================================================================
    class TBSite : TBBaseSite
	{
        Editor editor;

		IEventBindingService evtBndSvc;
		AmbientProperties ambientPropertiesSvc;
		
        public Editor Editor { get { return editor; } }
		public bool RunAsEasyStudioDesigner { get { return editor != null && editor is FormEditor && ((FormEditor)editor).RunAsEasyStudioDesigner; } }
		//--------------------------------------------------------------------------------
		public TBSite
			(
			IComponent component,
			IContainer container,
            Editor editor,
			string name
            )
            : base(component, container, name)
		{
			
			this.editor = editor;
            if (editor != null)
				evtBndSvc = new TBEventBindingService(editor.Sources, this);
			ambientPropertiesSvc = new AmbientProperties();
		}

		//--------------------------------------------------------------------------------
		protected TBSite(TBSite tbSite): base (tbSite as TBBaseSite)
		{
			this.editor = tbSite.editor;
            if (editor != null)
			    evtBndSvc = new TBEventBindingService(editor.Sources, this);
			ambientPropertiesSvc = new AmbientProperties();
		}

		//--------------------------------------------------------------------------------
		public override object GetService(Type serviceType)
		{
			if (serviceType == typeof(IEventBindingService))
				return evtBndSvc;

			if (serviceType == typeof(IDictionaryService))
				return this;

			if (serviceType == typeof(AmbientProperties))
				return ambientPropertiesSvc;

            if (editor != null)
                return editor.GetService(serviceType);

			return base.GetService(serviceType);
		}

		/// <summary>
		/// DocumentController è il componente radice del modello ad oggetti.
		/// Qui viene popolato tutto il modello a partire dal controller
		/// </summary>
		//-----------------------------------------------------------------------------
		internal static void AdjustSites(FormEditor formEditor)
		{
			if (formEditor.Controller == null)
				return;
			
			AdjustComponents(formEditor.Controller, formEditor);

			// aggiusta anche i siti dei referenced component perchè deve decodificare il drag&drop sul codeeditor
			if (formEditor.ComponentDeclarator != null)
			{
				foreach (ReferenceableComponent refComponent in formEditor.ComponentDeclarator.GetReferenceableComponents(typeof(BusinessObject)))
					refComponent.Site = new TBSite(refComponent, formEditor.Controller, formEditor, refComponent.DragDropClassName);
			}
		}

		//-----------------------------------------------------------------------------
		private static void AdjustView(FormEditor formEditor)
		{
			AdjustComponents(formEditor.Controller.View, formEditor);
		}

		//-----------------------------------------------------------------------------
		private static void AdjustComponents(IContainer container, FormEditor formEditor)
		{

			foreach (IComponent cmp in container.Components)
			{
                EasyBuilderComponent ebComponent = cmp as EasyBuilderComponent;
                if (ebComponent == null)
					continue;
                string siteName = ebComponent is IDocumentDataManager ? EasyBuilderSerializer.DocumentPropertyName : ebComponent.SerializedName;
                ebComponent.Site = new TBSite(ebComponent, container, formEditor, siteName);
                IContainer cnt = ebComponent as IContainer;
				if (cnt != null)
					AdjustComponents(cnt, formEditor);
				else
				{

					//recItem.Site = new TBSite(recItem, aMSqlRec, formEditor, recItem.SerializedName);
					//aMDataObj.Site = new TBSite(aMDataObj, aMSqlRec, formEditor, recItem.SerializedName);

					MSqlRecordItem recItem = ebComponent as MSqlRecordItem;

					//MSqlRecord sqlRec = container as MSqlRecord;
					//sqlRec->GetData()
					
					if (recItem != null)
					{
                        recItem.Site = new TBSite(recItem, container, formEditor, recItem.SerializedName);
                        MDataObj dataObj = recItem.DataObj as MDataObj;
						dataObj.Site = new TBSite(ebComponent, container, formEditor, recItem.SerializedName);
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		public override ITBSite CloneChild(IComponent component, string name)
		{
            TBSite site = new TBSite(component, Container, editor, name);
		
			if (this.Component is IContainer)
				site.Container = this.Component as IContainer;		
			return site;
		}

		//--------------------------------------------------------------------------------
		public EasyBuilderComponent GetEasyBuilderComponent()
		{
			if (Component is EasyBuilderComponent)
                return Component as EasyBuilderComponent;

			if (Container is IComponent)
				return ((IComponent)Container).Site == null ? null : ((TBSite)((IComponent)Container).Site).GetEasyBuilderComponent();

			return null;
		}

		

		#region ICloneable Members

		//--------------------------------------------------------------------------------
		public override object Clone()
		{
			return new TBSite(this);
		}

		#endregion
        #region IDisposable Members

        //--------------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
			base.Dispose(disposing);
        }

        #endregion
	}
}
