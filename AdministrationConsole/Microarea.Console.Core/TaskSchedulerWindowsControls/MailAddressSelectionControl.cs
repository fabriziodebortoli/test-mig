using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.TaskschedulerEngine.TaskSchedulerObjects;

namespace Microarea.Console.Core.TaskSchedulerWindowsControls
{
	/// <summary>
	/// Summary description for MailAddressSelectionControl.
	/// </summary>
	public partial class MailAddressSelectionControl : System.Windows.Forms.UserControl
	{
        private bool validateRecipients = true;

		#region MailAddressesSelectionButton class

		//===============================================================================
		public class MailAddressesSelectionButton : Control
		{
			public delegate void RecipientsListChangedEventHandler (object sender, string recipientsList);
			public event RecipientsListChangedEventHandler RecipientsListChanged;
			public event System.EventHandler BeforeRecipientsListChange;
			
			private IContainer components = new Container();
		
			private string		currentRecipientsList = String.Empty;
			private Image		image = null;
			private ToolTip		toolTip = null;
			private bool		mouseOver = false;
			private bool		mouseCapture = false;

			//---------------------------------------------------------------------------
			public MailAddressesSelectionButton()
			{
				Initialize();
			}

			//---------------------------------------------------------------------------
			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (components != null)
						components.Dispose();
				}
				base.Dispose(disposing);
			}

			//---------------------------------------------------------------------------
			public string RecipientsList 
			{
				get { return currentRecipientsList; }
				set 
				{	
					currentRecipientsList = String.Empty;
					if (value == null || value == String.Empty)
						return;

					try
					{
						using (WTESimpleMAPIWrapper simpleMAPI = new WTESimpleMAPIWrapper())
						{
							if (simpleMAPI.Logon(this.Handle, true))
							{
								string[] recipients = value.Split(';');

								if (recipients != null && recipients.Length > 0)
								{
									foreach (string recipientName in recipients)
									{
										if (recipientName == null || recipientName == String.Empty)
											continue;

										WTEMapiRecipDesc resolvedRecipient;
										if (simpleMAPI.ResolveName(recipientName, out resolvedRecipient))
										{
											if (currentRecipientsList.Length > 0)
												currentRecipientsList += ';';
											currentRecipientsList += resolvedRecipient.address;
											Marshal.FreeCoTaskMem(resolvedRecipient.entryID);
										}
									}
								}
								simpleMAPI.Logoff();
							}
						}
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
					currentRecipientsList = value;		
				}
			}

			#region MailAddressesSelectionButton protected overridden methods

			//---------------------------------------------------------------------------
			protected override void OnEnabledChanged(EventArgs e)
			{
				base.OnEnabledChanged(e);
				if (Enabled == false)
				{
					mouseOver = false;
					mouseCapture = false;
				}
				Invalidate();
			}
		
			//---------------------------------------------------------------------------
			protected override void OnMouseDown(MouseEventArgs e)
			{
				base.OnMouseDown(e);

				if (e.Button != MouseButtons.Left)
					return;

				if (mouseCapture == false || mouseOver == false)
				{
					mouseCapture = true;
					mouseOver = true;

					//Redraw to show button state
					Invalidate();
				}
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseUp(MouseEventArgs e)
			{
				base.OnMouseUp(e);

				if (e.Button != MouseButtons.Left)
					return;

				if (mouseOver == true || mouseCapture == true)
				{
					mouseOver = false;
					mouseCapture = false;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseUp(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseMove(MouseEventArgs e)
			{
				base.OnMouseMove(e);

				// Is mouse point inside our client rectangle
				bool over = this.ClientRectangle.Contains(new Point(e.X, e.Y));

				// If entering the button area or leaving the button area...
				if (over != mouseOver)
				{
					// Update state
					mouseOver = over;

					// Redraw to show button state
					Invalidate();
				}
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseEnter(EventArgs e)
			{
				// Update state to reflect mouse over the button area
				if (!mouseOver)
				{
					mouseOver = true;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseEnter(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnMouseLeave(EventArgs e)
			{
				// Update state to reflect mouse not over the button area
				if (mouseOver)
				{
					mouseOver = false;

					// Redraw to show button state
					Invalidate();
				}

				base.OnMouseLeave(e);
			}

			//---------------------------------------------------------------------------
			protected override void OnPaint(PaintEventArgs e)
			{
				base.OnPaint(e);
			
				DrawImage(e.Graphics);
				DrawBorder(e.Graphics);
			}

			//---------------------------------------------------------------------------
			protected override void OnClick(EventArgs e)
			{
				base.OnClick(e);

				if (BeforeRecipientsListChange != null)
					BeforeRecipientsListChange(this, System.EventArgs.Empty);
				try
				{
					using (WTESimpleMAPIWrapper simpleMAPI = new WTESimpleMAPIWrapper())
					{
						if (simpleMAPI.Logon(this.Handle, true))
						{
							WTEMapiRecipDesc[] oldRecipients = null;

							if (currentRecipientsList != null && currentRecipientsList != String.Empty)
							{
								string[] recipients = currentRecipientsList.Split(';');

								if (recipients != null && recipients.Length > 0)
								{
									int i = 0;
									oldRecipients = new WTEMapiRecipDesc[recipients.Length];
									foreach (string recipientName in recipients)
									{
										if (recipientName == null || recipientName == String.Empty)
											continue;

                                        WTEMapiRecipDesc resolvedRecipient;
										if (simpleMAPI.ResolveName(recipientName, out resolvedRecipient))
											oldRecipients[i++] = resolvedRecipient;
									}
								}
							}

                            WTEMapiRecipDesc[] newRecipients;
							bool ok = simpleMAPI.SelectAddresses("", oldRecipients, out newRecipients);

							if (oldRecipients != null)
							{
								foreach (WTEMapiRecipDesc recipientDesc in oldRecipients)
								{
									if (recipientDesc != null)
										Marshal.FreeCoTaskMem(recipientDesc.entryID);
								}
							}

							if (ok)
							{
								string recipientsList = String.Empty;
								foreach (WTEMapiRecipDesc recipient in newRecipients)
								{
									if (recipientsList.Length > 0)
										recipientsList += ';';
									recipientsList += recipient.name;
								}

								currentRecipientsList = recipientsList;

								if (RecipientsListChanged != null)
									RecipientsListChanged(this, recipientsList);
							}

							simpleMAPI.Logoff();
						}
						else
						{
							MessageBox.Show(TaskSchedulerWindowsControlsStrings.MAPILogonFailureErrMsg);
							this.Enabled = false;
						}
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
			}
			
			#endregion

			#region MailAddressesSelectionButton private methods

			//---------------------------------------------------------------------------
			private void Initialize()
			{
				Stream bitmapStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.Console.Core.TaskSchedulerWindowsControls.Bitmaps.MailAddressBook.bmp");
				if (bitmapStream != null)
				{
					System.Drawing.Bitmap bitmap = new Bitmap(bitmapStream);
					if (bitmap != null)
						image = bitmap;
				}

				// Prevent drawing flicker by blitting from memory in WM_PAINT
				SetStyle
					(
					ControlStyles.ResizeRedraw |
					ControlStyles.UserPaint |
					ControlStyles.AllPaintingInWmPaint |
					ControlStyles.DoubleBuffer,
					true
					);

				// Prevent base class from trying to generate double click events and
				// so testing clicks against the double click time and rectangle. Getting
				// rid of this allows the user to press then release button very quickly.
				SetStyle(ControlStyles.StandardDoubleClick, false);

				// Should not be allowed to select this control
				SetStyle(ControlStyles.Selectable, false);
	
				toolTip = new ToolTip(this.components);
				toolTip.SetToolTip(this, TaskSchedulerWindowsControlsStrings.MailAddressesSelectionButtonToolTipText);
			}

			//---------------------------------------------------------------------------
			private void DrawImage(Graphics g)
			{
				if (image == null)
					return;

				Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

				if (Enabled)
				{
					// Three points provided are upper-left, upper-right and 
					// lower-left of the destination parallelogram. 
					Point[] pts = new Point[3];
					pts[0].X = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
					pts[0].Y = (Enabled && mouseOver && mouseCapture) ? 1 : 0;
					pts[1].X = pts[0].X + image.Width;
					pts[1].Y = pts[0].Y;
					pts[2].X = pts[0].X;
					pts[2].Y = pts[1].Y + image.Height;

					g.DrawImage(image, pts, rect, GraphicsUnit.Pixel);
				}
				else
					ControlPaint.DrawImageDisabled(g, image, 0, 0, this.BackColor);
			}	

			//---------------------------------------------------------------------------
			private void DrawBorder(Graphics g)
			{
				if (!Enabled || !mouseOver)
					return;

				System.Windows.Forms.Border3DStyle borderStyle  = mouseCapture ? System.Windows.Forms.Border3DStyle.SunkenOuter : System.Windows.Forms.Border3DStyle.RaisedOuter;

				ControlPaint.DrawBorder3D(g, this.ClientRectangle, borderStyle);
			}

			#endregion
		}

		#endregion
		
		public MailAddressSelectionControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

		}

		#region MailAddressSelectionControl public properties
		
		//---------------------------------------------------------------------------
		[Localizable(true)]
		public new string Text 
		{
			get { return AddressTextBox.Text; }
			set 
            {
                AddressTextBox.Text = value;
                CheckValidAddress();
            }
		}
		
		#endregion

        //--------------------------------------------------------------------------------------------------------------------------------
        private void CheckValidAddress()
        {
            AddressTextBox.Text = AddressTextBox.Text.Trim();
            string trimmedRecipientName = String.Empty;
            if (validateRecipients && AddressTextBox.Text.Length > 0)
            {
				try
				{
					string[] recipients = AddressTextBox.Text.Split(';');

					bool recipientsOK = true;
					if (recipients != null && recipients.Length > 0)
					{
                        WTESimpleMAPIWrapper simpleMAPI = null;
						foreach (string recipientName in recipients)
						{
							trimmedRecipientName = recipientName.Trim();
							if (trimmedRecipientName == null || trimmedRecipientName == String.Empty)
								continue;

							// The following code uses the static Regex.IsMatch method to verify 
							// that a string is in valid e-mail format. 
							if (!System.Text.RegularExpressions.Regex.IsMatch(trimmedRecipientName, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$"))
							{
								if (simpleMAPI == null)
									simpleMAPI = new WTESimpleMAPIWrapper();
								if (!simpleMAPI.IsLoggedOn)
									simpleMAPI.Logon(this.Handle, true);

								WTEMapiRecipDesc resolvedRecipient;
								if
									(
									simpleMAPI != null &&
									simpleMAPI.IsLoggedOn &&
									simpleMAPI.ResolveName(trimmedRecipientName, out resolvedRecipient)
									)
								{
									Marshal.FreeCoTaskMem(resolvedRecipient.entryID);
								}
								else
								{
									recipientsOK = false;
									break;
								}
							}
						}
						if (simpleMAPI != null && simpleMAPI.IsLoggedOn)
							simpleMAPI.Logoff();

					}

					if (!recipientsOK)
					{
						MessageBox.Show(String.Format(TaskSchedulerWindowsControlsStrings.InvalidEmailAddressErrMsg, trimmedRecipientName));
						AddressTextBox.Text = String.Empty;
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show(ex.Message);
				}
            }
            validateRecipients = false;
        }

        #region MailAddressSelectionControl event handlers

        //--------------------------------------------------------------------------------------------------------------------------------
        private void AddressTextBox_TextChanged(object sender, System.EventArgs e)
        {
            validateRecipients = true;
        }

        //--------------------------------------------------------------------------------------------------------------------------------
        private void AddressTextBox_Validated(object sender, System.EventArgs e)
        {
            CheckValidAddress();
        }
        
        //---------------------------------------------------------------------------
		private void AddressBookButton_BeforeRecipientsListChange(object sender, System.EventArgs e)
		{
			AddressBookButton.RecipientsList = AddressTextBox.Text;
		}

        //--------------------------------------------------------------------------------------------------------------------------------
		private void AddressBookButton_RecipientsListChanged(object sender, string recipientsList)
		{
			AddressTextBox.Text = AddressBookButton.RecipientsList;
		}
	
		#endregion

	}
}
