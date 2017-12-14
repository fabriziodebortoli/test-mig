using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// TBButtonExtenderProvider
	/// </summary>
	public class TBWinStatusButtonExtender : TBWinButtonExtender
	{
		ImageList imageList = new ImageList();
		MDataObj statusDataObj;

		//---------------------------------------------------------------------
		protected MDataObj StatusDataObj
		{
			get { return statusDataObj; }
			set { statusDataObj = value; }
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Create a new instance of TBButtonExtenderProvider
		/// </summary>
		/// <param name="controller">the controller</param>
		/// <param name="dataObjForButtonState">databinding related to the status of the extender,
		/// it causes for example the change of the button image (autonumbering)</param>
		/// <param name="menuItems">menu items</param>
		/// <param name="shortcutKey">shortcut key associated to the button click</param>
		public TBWinStatusButtonExtender(
			ITBCUI controller,
			IDataObj dataObjForButtonState = null,
			IList<IMenuItemGeneric> menuItems = null,
			Keys shortcutKey = Keys.None
			)
			: base(controller, null, menuItems, shortcutKey)
		{
			this.statusDataObj = dataObjForButtonState as MDataObj;
		}

		//---------------------------------------------------------------------
		void DataObj_ValueChanged(object sender, EasyBuilderEventArgs e)
		{
			MDataObj mDataObj = sender as MDataObj;
			if (mDataObj == null)
			{
				return;
			}

			Button.BackgroundImage = imageList.Images[mDataObj.Value.ToString()];
		}

		//---------------------------------------------------------------------
		public void AddImage(string imageKey, Image image)
		{
			imageList.Images.Add(imageKey, image);
		}

		//---------------------------------------------------------------------
		public override void OnFormModeChanged(FormModeType newFormMode)
		{
			base.OnFormModeChanged(newFormMode);

			this.Button.Enabled = (newFormMode == FormModeType.New || newFormMode == FormModeType.Edit);
		}

		//---------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
		{
			IUIControl button = base.CreateExtenderUIControl();

			if (StatusDataObj != null)
			{
				this.StatusDataObj.ValueChanged += new EventHandler<EasyBuilderEventArgs>(DataObj_ValueChanged);
				DataObj_ValueChanged(this.StatusDataObj, EasyBuilderEventArgs.Empty);
			}

			return button;
		}

		//---------------------------------------------------------------------
		public override void DestroyExtenderUIControl()
		{
			base.DestroyExtenderUIControl();

			if (this.StatusDataObj != null)
			{
				this.StatusDataObj.ValueChanged -= new EventHandler<EasyBuilderEventArgs>(DataObj_ValueChanged);
			}
		}

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (this.imageList != null)
			{
				this.imageList.Dispose();
				this.imageList = null;
			}
			base.Dispose(disposing);
		}

		//---------------------------------------------------------------------
		protected override void OnButtonClicked(TBWinButtonExtenderEventArgs args)
		{
			args.StateDataObj = this.StatusDataObj;
			base.OnButtonClicked(args);
		}
	}
}
