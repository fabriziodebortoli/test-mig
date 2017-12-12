using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.StringLoader;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms.Containers
{
	//================================================================================================================
	public partial class UIDynamicView : UIUserControl
	{
		public enum Direction { Vertical, Horizontal }
		IDataManager dataManager;
		IRecord recordPrototype;
		private const int verticalSpace = 8;
		private const int horizontalSpace = 20;

		enum MoveTo { First, Last, Next, Prev };
        //---------------------------------------------------------------------
		List<CompositeControl> dynamicControls = new List<CompositeControl>();
		public Direction ResizeDirection { get; set; }
		//---------------------------------------------------------------------
		public void CreateAutomaticControls(IDataManager dataManager)
		{
			this.dataManager = dataManager;
			recordPrototype = dataManager.Record;
			CreateControls();

            CUI.UIManager.PrimaryToolbar.OnUpdateStatus();
		}

        //---------------------------------------------------------------------
        void PrimaryToolbar_ToolbarCreated(object theSender, EventArgs e)
        {
            UIToolbar toolbar = theSender as UIToolbar;
            UIToolbarButton rowFirst = toolbar.GetItem("FirstRow") as UIToolbarButton;
            rowFirst.Click += (sender, args) => { PerformRowChange(MoveTo.First); };
            toolbar.ChangeEnableCondition(rowFirst, () => 
                                                {
                                                    if (dataManager == null)
                                                        return false;

                                                    CurrencyManager currencyManager = GetCurrencyManagerOf(dataManager); 
                                                    return currencyManager.Position > 0; 
                                                });
            
            UIToolbarButton rowLast = toolbar.GetItem("LastRow") as UIToolbarButton;
            rowLast.Click += (sender, args) => { PerformRowChange(MoveTo.Last); };
            toolbar.ChangeEnableCondition(rowLast, () => 
                                                { 
                                                    if (dataManager == null)
                                                        return false;

                                                    CurrencyManager currencyManager = GetCurrencyManagerOf(dataManager); 
                                                    return currencyManager.Position < currencyManager.Count - 1; 
                                                });

            UIToolbarButton rowPrev = toolbar.GetItem("PrevRow") as UIToolbarButton;
            rowPrev.Click += (sender, args) => { PerformRowChange(MoveTo.Prev); };
            toolbar.ChangeEnableCondition(rowPrev, () => 
                                                {
                                                    if (dataManager == null)
                                                        return false;

                                                    CurrencyManager currencyManager = GetCurrencyManagerOf(dataManager); 
                                                    return currencyManager.Position > 0; 
                                                });

            UIToolbarButton rowNext = toolbar.GetItem("NextRow") as UIToolbarButton;
            rowNext.Click += (sender, args) => { PerformRowChange(MoveTo.Next); };
            toolbar.ChangeEnableCondition(rowNext, () => 
                                                {
                                                    if (dataManager == null)
                                                        return false;

                                                    CurrencyManager currencyManager = GetCurrencyManagerOf(dataManager); 
                                                    return currencyManager.Position < (currencyManager.Count -1); 
                                                });
        }

		//---------------------------------------------------------------------
		public override void CreateComponents()
		{
            CUI.UIManager.PrimaryToolbar.ToolbarCreated += new EventHandler(PrimaryToolbar_ToolbarCreated);
            CUI.UIManager.PrimaryToolBarStyle = PrimaryToolbarStyle.RowView;

			ResizeDirection = Direction.Horizontal;
			base.CreateComponents();
        }
	
        //---------------------------------------------------------------------
        private void PerformRowChange(MoveTo to)
        {
            CurrencyManager currencyManager = GetCurrencyManagerOf(dataManager);
            switch (to)
            {
                case MoveTo.First: currencyManager.Position = 0;  break;
                case MoveTo.Last: currencyManager.Position = currencyManager.Count - 1; break;
                case MoveTo.Prev: currencyManager.Position--; break;
                case MoveTo.Next: currencyManager.Position++; break;
                default:
                    break;
            }
            UIToolbar toolbar = CUI.UIManager.PrimaryToolbar as UIToolbar;
            toolbar.OnUpdateStatus();
        }

        //---------------------------------------------------------------------
		private void CreateControls()
		{
			Controls.Clear();

			for (int i = 0; i < recordPrototype.Fields.Count; i++)
			{
				IRecordField field = (IRecordField)recordPrototype.Fields[i];

				if (field == null || field.Name == "TBCreated" || field.Name == "TBCreatedID" || field.Name == "TBModified" || field.Name == "TBModifiedID")
					continue;
					
				string colLocalized = DatabaseLocalizer.GetLocalizedDescription(field.Name, recordPrototype.Name);
				colLocalized = InsertSpaces(colLocalized);
				IUIControl control = TBWFCUIControl.CreateControl(field.DataObj.GetType());
				if (control == null)
					continue;
				control.Name = field.Name;

				UILabel label = null;
				UICheckBox checkbox = control as UICheckBox;
				if (checkbox != null)
				{
					checkbox.Text = colLocalized;
				}
				else
				{
					label = new UILabel();
					Controls.Add(label);
					label.Text = colLocalized;
					label.Name = colLocalized;
				}
				dynamicControls.Add(new CompositeControl(control, label));
				Controls.Add((Control)control);
				control.AddTbBinding(dataManager, field.Name);


			}
			ResizeControl();
		}

		//---------------------------------------------------------------------
		private void ResizeControl()
		{
			if (ResizeDirection == UIDynamicView.Direction.Horizontal)
				ResizeControlHorizontal();
			else
				ResizeControlVertical();
		}

		//---------------------------------------------------------------------
		private static string InsertSpaces(string colLocalized)
		{
			StringBuilder sb = new StringBuilder();
			char prev = ' ';
			for (int i = 0; i < colLocalized.Length; i++)
			{
				char ch = colLocalized[i];

				if (Char.IsUpper(ch) && !Char.IsWhiteSpace(prev))
					sb.Append(' ');
				sb.Append(ch);
				prev = ch;
			}
			return sb.ToString();
		}

		//---------------------------------------------------------------------
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
			ResizeControl();
		}

		//---------------------------------------------------------------------
		private void ResizeControlVertical()
		{
			int x = 10, y = 10;
			int width = 0;
			for (int i = 0; i < dynamicControls.Count; i++)
			{
				CompositeControl ctrl = dynamicControls[i];
				UILabel label = ctrl.Label;

				IUIControl control = ctrl.Control;
				int newY = y + control.Size.Height;
				if (label != null)
					newY += label.Size.Height;
				if (newY > Height)
				{
					y = 10;
					x += width + horizontalSpace;
				}

				if (label != null)
				{
					label.Location = new Point(x, y);
					y += label.Size.Height;
				}
				
				control.Location = new Point(x, y);
				y += control.Size.Height + verticalSpace;
				width = Math.Max(width, control.Size.Width);
				
			}

		}

		//---------------------------------------------------------------------
		private void ResizeControlHorizontal()
		{
			int x = 10, y = 10;
			int currentBottom = 0;
			for (int i = 0; i < dynamicControls.Count; i++)
			{
				CompositeControl ctrl = dynamicControls[i];
				UILabel label = ctrl.Label;

				Control control = ctrl.Control as Control;
				int newX = x + control.Size.Width;
				if (newX > Width)
				{
					x = 10;
					y = currentBottom  + verticalSpace;
				}

				if (label != null)
				{
					label.Location = new Point(x, y);
				}

				control.Location = new Point(x, y + 16);
				x += control.Size.Width + horizontalSpace;
				currentBottom = Math.Max(currentBottom, control.Bottom);

			}

		}

		//---------------------------------------------------------------------
		public UIDynamicView()
		{
			InitializeComponent();
		}
	}
	class CompositeControl
	{
		UILabel label;
		IUIControl control;
	
		public UILabel Label
		{
			get { return label; }
		}
		
		public IUIControl Control
		{
			get { return control; }
		}

		public CompositeControl(IUIControl control, UILabel label)
		{
			this.control = control;
			this.label = label;
		}
	}
}
