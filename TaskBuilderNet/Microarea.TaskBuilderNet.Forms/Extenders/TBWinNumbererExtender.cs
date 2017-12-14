using System;
using System.Windows.Forms;
using Microarea.Framework.TBApplicationWrapper;
using Microarea.TaskBuilderNet.Forms.Properties;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms
{
	//=========================================================================
	/// <summary>
	/// UINumbererExtenderProvider
	/// </summary>
	public class TBWinNumbererExtender : TBWinStatusButtonExtender
	{
		NumbererProvider numbererProvider;
		bool useFormatter = true;

		//---------------------------------------------------------------------
		public bool UseFormatter
		{
			get { return useFormatter; }
			set { useFormatter = value; }
		}

		//---------------------------------------------------------------------
		public MDataBool ManualNumbering { get { return this.StatusDataObj as MDataBool; } }

		//---------------------------------------------------------------------
		public TBWinNumbererExtender(
			ITBCUI controller
			)
			: base(controller)
		{
			this.numbererProvider = new NumbererProvider();

			AddImage("false", Resources.Lightning);
			AddImage("true", Resources.Pencil);

			(Extendee as Control).Leave += new EventHandler(TBWinNumbererExtender_Leave);
		}

		//---------------------------------------------------------------------
		void TBWinNumbererExtender_Leave(object sender, EventArgs e)
		{
			Extendee.Text = this.numbererProvider.ApplyMask(Extendee.Text);
		}

		//---------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
		{
			this.numbererProvider.Initialize(Controller.Document, Data, useFormatter);
			this.StatusDataObj = this.numbererProvider.StateDataObj as MDataObj;

			return base.CreateExtenderUIControl();
		}

		//---------------------------------------------------------------------
		public override bool CanEnableExtendee()
		{
			FormModeType formMode = this.Controller.Document.FormMode;

			return this.ManualNumbering.Value && (formMode == FormModeType.New || formMode == FormModeType.Edit);
		}

		//---------------------------------------------------------------------
		protected override void OnButtonClicked(TBWinButtonExtenderEventArgs args)
		{
			base.OnButtonClicked(args);

			this.ManualNumbering.Value = !this.ManualNumbering.Value;

			this.Extendee.Enabled = this.ManualNumbering.Value;
			if (this.ManualNumbering.Value)
			{
				this.Data.Clear();
			}
			else
			{
				this.numbererProvider.ReadNumber();
			}
		}

		//---------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (this.numbererProvider != null)
			{
				this.numbererProvider.Dispose();
				this.numbererProvider = null;
			}
			Control ctrl = Extendee as Control;
			if (ctrl != null)
			{
				ctrl.Leave -= new EventHandler(TBWinNumbererExtender_Leave);
			}

			base.Dispose(disposing);
		}
	}
}
