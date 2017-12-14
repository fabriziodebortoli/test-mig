using System;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.Validation;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms
{
    //================================================================================================================
    class TBWinDiagnosticExtender : TBWinExtender
    {
        ErrorProvider errorProvider;

		Control ExtendedControl { get { return base.Extendee as Control; } }

        //----------------------------------------------------------------------------
        public TBWinDiagnosticExtender(TBCUIControl tbCuiControl)
			: base (tbCuiControl)
        {
			Position = UIExtenderPosition.None;
		}

        //----------------------------------------------------------------------------
		public override IUIControl CreateExtenderUIControl()
        {
			if (Controller == null)
				throw new InvalidOperationException("No controller set for this extender");

			if (Controller.DataBinding == null)
                throw new InvalidOperationException("extendedObject does not have a databinding");

			IDataObj dataObj = Data as IDataObj;
            if (dataObj == null)
                throw new ArgumentException("extendedObject has an invalid databinding");

			ITBValidationConsumer validationConsumer = Controller.Document as ITBValidationConsumer;

            ITBValidationContext context = validationConsumer == null ? null : validationConsumer.ValidationManager.GetContext(dataObj);
            //Il contesto di validazione e` null quando al data obj non e` associata alcuna validazione.
            if (context != null)
            {
                //Se c'e` un contesto di validazione allora creo l' error provider.
                errorProvider = new ErrorProvider();
                errorProvider.BlinkStyle = ErrorBlinkStyle.AlwaysBlink;
                errorProvider.SetIconAlignment(this.ExtendedControl, ErrorIconAlignment.MiddleRight);
				errorProvider.SetIconPadding(this.ExtendedControl, -20);

                context.ValidationStarted += new ValidationStartedDelegate(DataObj_ValidationStarted);
                context.ValidationEnded += new ValidationEndedDelegate(DataObj_ValidationEnded);
            }

			return null;
         }

		//-------------------------------------------------------------------------
		public override void DestroyExtenderUIControl()
		{
			if (errorProvider != null)
				errorProvider.SetError(this.ExtendedControl, string.Empty);
		}

		//----------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (disposing)
			{
				IDataObj dataObj = Data as IDataObj;
				if (dataObj != null)
				{
					ITBValidationConsumer validationConsumer = Controller.Document as ITBValidationConsumer;

					ITBValidationContext context = validationConsumer == null ? null : validationConsumer.ValidationManager.GetContext(dataObj);
					if (context != null)
					{
						context.ValidationStarted -= new ValidationStartedDelegate(DataObj_ValidationStarted);
						context.ValidationEnded -= new ValidationEndedDelegate(DataObj_ValidationEnded);
					}
				}

				if (errorProvider != null)
				{
					errorProvider.Dispose();
					errorProvider = null;
				}
			}
		}

		//----------------------------------------------------------------------------
		public override void OnFormModeChanged(FormModeType newFormMode)
		{
			base.OnFormModeChanged(newFormMode);
			if (newFormMode == FormModeType.Browse)
			{
				DestroyExtenderUIControl();
			}
		}

        //----------------------------------------------------------------------------
        void DataObj_ValidationStarted(object sender, IValidationEventArgs e)
        {
            e.Info.Message = string.Empty;

			if (errorProvider != null && this.ExtendedControl != null)
				errorProvider.SetError(this.ExtendedControl, String.Empty);
        }
        
        //----------------------------------------------------------------------------
        void DataObj_ValidationEnded(object sender, IValidationEventArgs e)
        {
			if (e.Info.Validated)
            {
                e.Info.Message = string.Empty;
            }

            if (errorProvider != null && this.ExtendedControl != null)
				errorProvider.SetError(this.ExtendedControl, e.Info.Message);
        }
	}
}
