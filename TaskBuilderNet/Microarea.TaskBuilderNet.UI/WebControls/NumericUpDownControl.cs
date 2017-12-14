using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microarea.TaskBuilderNet.Core.Generic;
//using Microarea.TaskBuilderNet.Core.CoreTypes;

namespace Microarea.TaskBuilderNet.UI.WebControls
{
	//====================================================================================
	[DefaultProperty("Value"),
	DesignerAttribute(typeof(NumericUpDownControl.NumericUpDownControlDesigner), typeof(IDesigner)),
	ToolboxData("<{0}:NumericUpDownControl runat=server></{0}:NumericUpDownControl>")]
	public class NumericUpDownControl : System.Web.UI.WebControls.WebControl, IPostBackDataHandler
	{
		private int		currentValue = 0;
		private string	upArrowImageURL = String.Empty;
		private string	downArrowImageURL = String.Empty;
		private int		maxValue = 100;
		private int		minValue = 0;
		private int		step = 1;
		private int		inputMaxCharLength = 4;
		private int		inputCharSize = 4;
		private int		pixelWidth = 82;
		private int		pixelHeight = 30;

		private const int InputWidthOffset = 4;
		private const int InputHeightOffset = 4;
		private const int UpDownImageCellWidth = 24;
		/// <summary>
		/// Event handler for change value event.
		/// </summary>
		public event System.EventHandler ValueChanged;

		/// <summary>
		/// Control constructor.
		/// </summary>
		public NumericUpDownControl()
		{
		}

		#region NumericUpDownControl public properties

		/// <summary>
		/// Property for setting and getting the value stored in the control.
		/// </summary>
		public int Value
		{
			get
			{
				object obj = ViewState["Value"];
				currentValue = (obj == null) ? 0 : (int)obj;
				return currentValue;
			}

			set
			{
				currentValue = (int)value;
				ViewState["Value"] = currentValue;
			}
		}

		public string Text
		{
			get
			{
				return this.Value.ToString();
			}

			set
			{
				try
				{
					int num = Convert.ToInt32(value);
					this.Value = num;
				}
				catch(Exception)
				{
				}
			}
		}
		
		/// <summary>
		/// Property for specifying the URL for up-arrow image.
		/// </summary>
		[Category("Text")]
		[Description("URL for up arrow image")]
		public string UpArrowImageURL
		{
			get
			{
				return upArrowImageURL;
			}
			set
			{
				upArrowImageURL = value;
			}
		}

		/// <summary>
		/// Property for specifying the URL for down arrow image.
		/// </summary>
		[Category("Text")]
		[Description("URL for down arrow image")]
		public string DownArrowImageURL
		{
			get
			{
				return downArrowImageURL;
			}
			set
			{
				downArrowImageURL = value;
			}
		}

		/// <summary>
		/// Property for specifying  MAX value for up-down control.
		/// </summary>
		[Category("Action")]
		[Description("Gets or sets the maximum value for control to display.")]
		public int MaxValue
		{
			get
			{
				return maxValue;
			}
			set
			{
				maxValue = value;
			}
		}

		/// <summary>
		/// Property for specifying the MIN value for up-down control.
		/// </summary>
		[Category("Action")]
		[Description("Gets or sets the minimum value for control to display.")]
		public int MinValue
		{
			get
			{
				return minValue;
			}
			set
			{
				minValue = value;
			}
		}

		/// <summary>
		/// Property for specifying increment for the up-down control.
		/// </summary>
		[Category("Action")]
		[Description("Gets or sets the increment value for up-down.")]
		public int Step
		{
			get
			{
				return step;
			}
			set
			{
				step = value;
			}
		}

		/// <summary>
		/// Property for specifying the maximum number of characters that the user can enter in the text input control.
		/// </summary>
		[Category("Action")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Gets or sets the maximum number of characters that the user can enter in the text input control.")]
		public int InputMaxCharLength
		{
			get
			{
				return inputMaxCharLength;
			}
			set
			{
				inputMaxCharLength = value;
			}
		}
		/// <summary>
		/// Property for specifying the size of the text input control, in characters.
		/// </summary>
		[Category("Action")]
		[RefreshProperties(RefreshProperties.All)]
		[Description("Gets or sets the size of the text input control, in characters.")]
		public int InputCharSize
		{
			get
			{
				return inputCharSize;
			}
			set
			{
				inputCharSize = value;
			}
		}

		/// <summary>
		/// Property for specifying the width for the up-down control (in pixels).
		/// </summary>
		[Category("Action")]
		[Description("Gets or sets the width of the control(in pixels).")]
		public int PixelWidth
		{
			get
			{
				return pixelWidth;
			}
			set
			{
				pixelWidth = value;
				base.Width = new Unit(value, UnitType.Pixel);
			}
		}

		[Browsable(false)]
		[Description("Gets the width of the control.")]
		public new Unit Width
		{
			get
			{
				return base.Width;
			}
		}

		[Browsable(false)]
		[Description("Gets the width of the area available for the text input control, in pixels.")]
		public int InputPixelWidth
		{
			get
			{
				return pixelWidth - InputWidthOffset - UpDownImageCellWidth;
			}
		}

		[Browsable(false)]
		public int UpDownArrowsCellWidth
		{
			get
			{
				return UpDownImageCellWidth;
			}
		}

		/// <summary>
		/// Property for specifying the height for the up-down control (in pixels).
		/// </summary>
		[Category("Action")]
		[Description("Gets or sets the height of the control (in pixels).")]
		public int PixelHeight
		{
			get
			{
				return pixelHeight;
			}
			set
			{
				pixelHeight = value;
				base.Height = new Unit(value, UnitType.Pixel);
			}
		}

		[Browsable(false)]
		public new Unit Height
		{
			get
			{
				return base.Height;
			}
		}

		[Browsable(false)]
		public int InputPixelHeight
		{
			get
			{
				return pixelHeight - InputHeightOffset;
			}
		}

		#endregion

		#region NumericUpDownControl private properties

		/// <summary>
		/// This property returns the unique name that is assigned to the hidden
		/// field for storing control's value.
		/// </summary>
		private string HiddenID
		{
			get
			{
				return "__" + ClientID + "_State";
			}
		}

		/// <summary>
		/// This field returns the name of the variable that saves the reference to
		/// hidden field DHTML object storing the value of control.
		/// </summary>
		private string HiddenFieldVar
		{
			get
			{
				return "ob_" + HiddenID;
			}
		}

		/// <summary>
		/// This property returns the unique name that is assigned to the text
		/// control.
		/// </summary>
		private string InputCtlName
		{
			get
			{
				return "__" + this.ClientID + "_Input";
			}
		}

		/// <summary>
		/// This property returns the name of the variable that holds the reference to 
		/// the  text control DHTML object.
		/// </summary>
		private string InputCtlVarName
		{
			get
			{
				return "ob" + this.ClientID;
			}
		}

		/// <summary>
		/// This property returns the name of the function that is used to parse the
		/// text value of control into "int"
		/// </summary>
		private string InputCtlValParseFunctionName
		{
			get
			{
				return "getControlValue_" + this.ClientID;
			}
		}

		private string InputCtlValOnKeyPressFunctionName
		{
			get
			{
				return "onKeyPress_" + this.ClientID;
			}
		}

		private string InputCtlValOnKeyUpFunctionName
		{
			get
			{
				return "onKeyUp_" + this.ClientID;
			}
		}
		
		private string InputCtlValOnChangeFunctionName
		{
			get
			{
				return "OnChange_" + this.ClientID;
			}
		}

		/// <summary>
		/// This property returns the name of the function that handles when user clicks 
		/// on the up arrow image.
		/// </summary>
		private string UpArrowClickFunction
		{
			get
			{
				return "onUpArrowClick_" + this.ClientID;
			}
		}

		/// <summary>
		/// This property returns the name of the function that handles when user clicks 
		/// on the down arrow image.
		/// </summary>
		private string DownArrowClickFunction
		{
			get
			{
				return "onDownArrowClick_" + this.ClientID;
			}
		}

		/// <summary>
		/// This property returns the name of the variable for storing maximum value for the
		/// control, in client side script.
		/// </summary>
		private string MaxValVar
		{
			get
			{
				return this.ClientID + "_MaxVal";
			}
		}

		/// <summary>
		/// This property returns the name of the variable for storing minimum value for the
		/// control, in client side script.
		/// </summary>
		private string MinValVar
		{
			get
			{
				return this.ClientID + "_MinVal";
			}
		}

		/// <summary>
		/// The proprty return variable name for storing increment value for the control in
		/// client side script.
		/// </summary>
		private string StepVar
		{
			get
			{
				return this.ClientID + "_StepVal";
			}
		}

		#endregion

		#region NumericUpDownControl protected overridden methods

		/// <summary>
		/// This event is raised by framework to give chance to the controls to any intialization.
		/// At this stage, control should not make any assumption about existence of any other
		/// control on the container page. In this event, control should perfom any initialization
		/// that is not dependant on other controls or state.
		/// This control, registers itself with the page letting it know that it wants to be
		/// notified of PostBack event.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			if (Page != null)
			{
				if (downArrowImageURL.Length == 0)
					downArrowImageURL = ImagesHelper.CreateImageAndGetUrl("DownArrow.gif", Helper.DefaultReferringType);

				if (upArrowImageURL.Length == 0)
					upArrowImageURL = ImagesHelper.CreateImageAndGetUrl("UpArrow.gif", Helper.DefaultReferringType);

				Page.RegisterRequiresPostBack(this);
			}
		}

		/// <summary>
		/// This event is raised by framework just before the control is to be renders.
		/// Control takes this oppurtunity to register the hidden field that will be used
		/// to convey its value when postback happens.
		/// </summary>
		/// <param name="args"></param>
		protected override void OnPreRender(EventArgs args)
		{
			base.OnPreRender(args);
			if (Page != null)
			{
				Page.ClientScript.RegisterHiddenField(HiddenID, Value.ToString());
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// This is the function that does the rendering of the control. 
		/// </summary>
		/// <param name="output"></param>
		//-----------------------------------------------------------------------------
		protected override void RenderContents(HtmlTextWriter output)
		{
			// Start rendering the control.
			StringBuilder strRender = new StringBuilder();
	
			strRender.Append("\n<table name=\"");		
			strRender.Append(this.UniqueID + "\"");
			strRender.Append(" id =\"" + this.UniqueID + "\"");
			strRender.Append(" height=\"" + this.PixelHeight.ToString() + "px\" width=\"" + this.PixelWidth.ToString() + "px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">");
			strRender.Append("\n<tr>");

			strRender.Append("\n<td rowspan=\"2\" align=\"left\" valign=\"middle\" width=\"" + this.InputPixelWidth.ToString(CultureInfo.InvariantCulture) + "px\" height=\"" + this.PixelHeight.ToString(CultureInfo.InvariantCulture) + "px\">");
			strRender.Append("\n<input type=\"text\"");
			strRender.Append(" style=\"COLOR:" + this.ForeColor.Name + ";FONT-FAMILY:" + this.Font.Name + ";FONT-SIZE:" + this.Font.Size.ToString(CultureInfo.InvariantCulture) + ";height:" + this.InputPixelHeight.ToString(CultureInfo.InvariantCulture) + "px;\"");
			strRender.Append(" value=\"");
			strRender.Append(this.Value);
			strRender.Append("\"");
			strRender.Append(" id=\"" + this.InputCtlName + "\"");
			strRender.Append(" name=\"" + this.InputCtlName + "\"");
			strRender.Append(" maxlength=\"" + this.InputMaxCharLength + "\" size=\"" + this.InputCharSize.ToString(CultureInfo.InvariantCulture) + "\"");
			strRender.Append(" onkeypress=\"javascript:" + this.InputCtlValOnKeyPressFunctionName + "();\"");
			strRender.Append(" onkeyup=\"javascript:" + this.InputCtlValOnKeyUpFunctionName + "();\"");
			strRender.Append(" onchange=\"javascript:" + this.InputCtlValOnChangeFunctionName + "();\"");
			strRender.Append(">");
			strRender.Append("\n</td>");
																																																								 
			strRender.Append("\n<td align=\"left\" valign=\"bottom\" width=\"" + this.UpDownArrowsCellWidth.ToString(CultureInfo.InvariantCulture) + "px\" height=\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">");
			strRender.Append("\n<p><font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"1\">");
			strRender.Append("<img width=\"19\" height=\"11\" border=\"0\" title=\"" + String.Format(WebControlsStrings.UpDownIncrementValue, this.Step) + "\" src=\"");
			strRender.Append(this.upArrowImageURL);
			strRender.Append("\"");
			strRender.Append(" onclick=\"javascript:" + this.UpArrowClickFunction + "();\"");
			strRender.Append(">");
			strRender.Append("\n</font></p>");
			strRender.Append("\n</td>");
	
			strRender.Append("\n</tr>");
			
			strRender.Append("\n<tr>");
			
			strRender.Append("\n<td align=\"left\" valign=\"top\" width=\"" + this.UpDownArrowsCellWidth.ToString(CultureInfo.InvariantCulture) + "px\" height=\"12px\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">");
			strRender.Append("\n<font face=\"Verdana, Arial, Helvetica, sans-serif\" size=\"1\">");
			strRender.Append("<img width=\"19\" height=\"11\" border=\"0\" title=\"" + String.Format(WebControlsStrings.UpDownDecrementValue, this.Step) + "\" src=\"");
			strRender.Append(this.downArrowImageURL);
			strRender.Append("\"");
			strRender.Append(" onclick=\"javascript:" + this.DownArrowClickFunction + "();\"");
			strRender.Append(">");
			strRender.Append("\n</font>");
			strRender.Append("\n</td>");
	
			strRender.Append("\n</tr>");

			strRender.Append("\n</table>");

			RenderClientSideScript(strRender);

			output.Write(strRender.ToString());
		}
		
		#endregion

		#region NumericUpDownControl private methods

		/// <summary>
		/// This function renders the client side script for the control. This script
		/// emits the functions handling event for click on up-down images. In response
		/// to the click event, the functions change the values in the control's text
		/// box accordingly.
		/// </summary>
		/// <param name="strRender"></param>
		private void RenderClientSideScript(StringBuilder strRender)
		{
			string postback = "";
			if (Page != null)
				postback = Page.ClientScript.GetPostBackEventReference(this, null) + ";";
			
			strRender.Append("\n");
			strRender.Append(
				"<script language=\"javascript\">" +
				"\n	var " + this.InputCtlVarName  + "= document.getElementById(\"" + this.InputCtlName + "\");" +
				"\n	var " + this.HiddenFieldVar + "= document.getElementById(\"" + this.HiddenID + "\");" +
				"\n var " + this.MaxValVar + " = " + this.MaxValue + ";" +
				"\n var " + this.MinValVar + " = " + this.MinValue + ";" +
				"\n var " + this.StepVar + " = " + this.Step + ";" +
				"\nfunction " + this.UpArrowClickFunction + "()" +
				"\n{" +
				"\n\tvar currValue = " + this.InputCtlValParseFunctionName + "();" +
				"\n\tif ((currValue + " + this.StepVar + ") <= " + this.MaxValVar + ")" +
				"\n\t{" +
				"\n\t\tcurrValue = currValue + " + this.StepVar + ";" +
				"\n\t\t" + this.InputCtlVarName + ".value = currValue;" +
				"\n\t\t" + this.HiddenFieldVar + ".value = currValue;" +
				"\n\t\t" + postback +
				"\n\t}" +
				"\n}" +
				"\nfunction "+ this.DownArrowClickFunction + "()" +
				"\n{" +
				"\n\tvar currValue = " + this.InputCtlValParseFunctionName + "();" +
				"\n\tif ((currValue - " + this.StepVar + ") >= " + this.MinValVar + ")" +
				"\n\t{" +
				"\n\t\tcurrValue = currValue - " + this.StepVar + ";" +
				"\n\t\t" + this.InputCtlVarName + ".value = currValue;" +
				"\n\t\t" + this.HiddenFieldVar + ".value = currValue;" +
				"\n\t\t" + postback +
				"\n\t}" +
				"\n}" +
				"\nfunction " + this.InputCtlValParseFunctionName + "()" +
				"\n{" +
				"\n\tvar iVal = String(" + this.InputCtlVarName + ".value);" +
				"\n\tif (iVal == \"undefined\" || iVal == \"-\")" + 
				"\n\t\treturn NumericUpDownControl1_MinVal;" +
				"\n\tnum = parseInt(iVal, 10);" +
				"\n\tif (num == NaN)" +
				"\n\t\treturn " + this.MinValVar + ";" +
				"\n\telse" +
				"\n\t\treturn num;" +
				"\n}" +
				"\nfunction " + this.InputCtlValOnKeyPressFunctionName + "()" +
				"\n{" +
				"\n\tvar strChar = String.fromCharCode(window.event.keyCode);" +
				"\n\tif (strChar < \"0\" || strChar > \"9\")" +
				"\n\t{" +
				"\n\t\tif (strChar != \"-\")" +
				"\n\t\t\twindow.event.keyCode = null;" +
				"\n\t\twindow.event.cancelBubble = true;" +
				"\n\t}" +
				"\n}" +
				"\nfunction " + this.InputCtlValOnChangeFunctionName + "()" +
				"\n{" +
				"\n\tvar currValue = " + this.InputCtlValParseFunctionName + "();" +
				"\n\tif (currValue == NaN)" + 
				"\n\t\tcurrValue = " + this.MinValVar + ";" +
				"\n\tif (currValue < " + this.MinValVar + " || currValue > "  + this.MaxValVar + ")" +
				"\n\t{" +
				"\n\t\tif (" + this.HiddenFieldVar + " == undefined || " + this.HiddenFieldVar + ".value == NaN) " + 
				"\n\t\t" + this.HiddenFieldVar + ".value = " + this.MinValVar + ";" +
				"\n\t\t" + this.InputCtlVarName + ".value = " + this.HiddenFieldVar + ".value;" +
				"\n\t\talert('" + WebControlsStrings.OutOfRangeValueAlertMsg + "');" +
				"\n\t}" +
				"\n\telse" +
				"\n\t\t" + this.HiddenFieldVar + ".value = currValue;" +
				"\n\t" + postback +
				"\n}" +
				"\nfunction " + this.InputCtlValOnKeyUpFunctionName + "()" +
				"\n{" +
				"\n	var currValue = document.getElementById(\"" + this.InputCtlName + "\").value;" +
				"\n\tif (currValue != NaN && currValue > " + this.MinValVar + " && currValue < " + this.MaxValVar + ")" +
				"\n\t{" +
				"\n\t\t" + this.InputCtlVarName + ".value = currValue;" +
				"\n\t\t" + this.HiddenFieldVar + ".value = currValue;" +
				"\n\t\t" + postback +
				"\n\t}" +
				"\n}" +
				"\n</script>");
		}

		#endregion

		/// <summary>
		/// Framework will call this method when postback event is fired on the containing
		/// page. This gives a chance to the control to see if the data was changed for it
		/// or not. Based on this check, the control can decide to fire its own event, letting
		/// any other control know about its new state.
		/// </summary>
		/// <param name="postDataKey">Key identifying this control</param>
		/// <param name="postCollection">Collection containing the posted data</param>
		/// <returns></returns>
		public virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			int presentValue = this.Value;
			string postedString = postCollection[HiddenID];
			int postedValue = (postedString != null && postedString != String.Empty) ? (int)Convert.ToInt32(postCollection[HiddenID]) : 0;

			// If two values don't match, then update the value.
			if (presentValue != postedValue)
			{
				this.Value = postedValue;
				// Indicate the state of this control has changed.
				return true;
			}

			// If there was no change in the data, then indicate that there is
			// no change in state by returning false.
			return false;
		}

		/// <summary>
		/// This method gets called if server returns "true" from LoadPostData event indicating that
		/// control's state has changed. The control this oppurtunity to raise it own events.
		/// </summary>
		public virtual void RaisePostDataChangedEvent()
		{
			OnValueChanged(EventArgs.Empty);
		}

		/// <summary>
		/// Event handler for the changed value of the control.
		/// </summary>
		/// <param name="args"></param>
		private void OnValueChanged(EventArgs args)
		{
			// If the value has been changed, raise the event.
			if (ValueChanged != null)
				ValueChanged(this, args);
		}
		
		//====================================================================================
		public class NumericUpDownControlDesigner : System.Web.UI.Design.ControlDesigner 
		{
			//--------------------------------------------------------------------------------
			public override string GetDesignTimeHtml() 
			{
				// Component is the control instance, defined in the base
				// designer
				NumericUpDownControl ctl = (NumericUpDownControl)Component;

				StringWriter sw = new StringWriter();
				HtmlTextWriter tw = new HtmlTextWriter(sw);

				ctl.RenderControl(tw);

				return sw.ToString();
			}
		}
	}
}
