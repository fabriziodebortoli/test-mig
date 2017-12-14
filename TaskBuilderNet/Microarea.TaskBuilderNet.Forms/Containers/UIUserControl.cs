using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;


namespace Microarea.TaskBuilderNet.Forms
{
	//================================================================================================================
	/// <summary>
	///   Tb Window Form 
	/// </summary>
    [ToolboxItem(false)] 
    [FormType(typeof(UIForm))]
	public partial class UIUserControl : UserControl, IUIUserControl
	{
        TBCUI cui;

		[Browsable(false)]
		virtual public ITBCUI CUI { get { return cui; } }

		//-------------------------------------------------------------------------
		[Browsable(false)]
		public virtual NameSpaceObjectType NameSpaceObjectType
		{
			get { return NameSpaceObjectType.Form; }
		}


        //-------------------------------------------------------------------------
        public System.Collections.IList ChildControls
        {
            get { return Controls; }
        }

 
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IMAbstractFormDoc Document { get { return CUI.Document; } }

		/// <summary>
		///   Constructor
		/// </summary>
        //-------------------------------------------------------------------------
        public UIUserControl()
		{
            cui = new TBCUI(this, NameSpaceObjectType); 
            InitializeComponent();
            AutoScroll = true;
		}

        //-------------------------------------------------------------------------
        virtual public void OnCreateDocumentPartsUI()
        {
        }

        //-------------------------------------------------------------------------
        virtual public void CreateComponents()
        {
            
        }

        //-------------------------------------------------------------------------
        virtual public void BindControls(MSqlRecord record)
        {

        }

        //-------------------------------------------------------------------------
        virtual public void BindControls(MDBTObject dbt)
        {

        }

		//-------------------------------------------------------------------------
		protected UIForm CreateForm<T>(out T ctrl) where T : UIUserControl 
		{
			UIForm parentForm =(UIForm)FindForm();
            Type formType = typeof(T) ;
        	object[] customAttributes = formType.GetCustomAttributes(typeof(FormTypeAttribute), true);

			if (customAttributes == null || customAttributes.Length == 0)
			{
				formType = typeof(UIForm);
			}
			else
                formType = ((FormTypeAttribute)customAttributes[0]).FormType;

            UIForm form = Activator.CreateInstance(formType) as UIForm;
			form.Owner = parentForm;
			
			ctrl = Activator.CreateInstance<T>();
			ctrl.CUI.Document = Document;
			ctrl.CUI.UIManager = (ITBCUIManager) parentForm.CUI;
			ctrl.BindingContext = BindingContext;
			form.OwnsDocument = false;
			form.Show(ctrl, Document, IntPtr.Zero);

			return form;
		}

        //-------------------------------------------------------------------------
        public CurrencyManager GetCurrencyManagerOf(IDataManager dataManager)
        {
            object dataSource = dataManager.BindableDataSource;
            return BindingContext[dataSource] as CurrencyManager;
        }

		//-------------------------------------------------------------------------
        public virtual void CreateExtenders()
		{
		}
	}
}
