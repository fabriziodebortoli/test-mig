using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Microarea.TaskBuilderNet.Forms.Controls;
using Microarea.TaskBuilderNet.Interfaces.Model;
using Microarea.TaskBuilderNet.Interfaces.View;
using Microarea.TaskBuilderNet.Model.TBCUI;

namespace Microarea.TaskBuilderNet.Forms
{
	//================================================================================================================
	public static class IUIControlExtensionsMethods
	{
		//-------------------------------------------------------------------------
		public static void AddTbBinding<TRecord, TProperty>(this IUIControl control, TRecord record, Expression<Func<TRecord, TProperty>> property) where TRecord : IRecord
		{
			ITBBindableObject bindableControl = control as ITBBindableObject;
			Debug.Assert(bindableControl != null); 
			
			AddTbBinding(control, bindableControl.DefaultBindingProperty, record, property);
		}

		//-------------------------------------------------------------------------
		public static void AddTbBinding(this IUIControl control, IDataManager dataManager, string fieldName)
		{
			ITBBindableObject bindableControl = control as ITBBindableObject;
			Debug.Assert(bindableControl != null);
			
			AddTbBinding(control, bindableControl.DefaultBindingProperty, dataManager, fieldName);
		}

		//-------------------------------------------------------------------------
		public static void AddTbBinding<TRecord, TProperty>(this IUIControl control, string controlPropertyName, TRecord record, Expression<Func<TRecord, TProperty>> property) where TRecord : IRecord
		{
			TBWFCUIControl cui = control.CUI as TBWFCUIControl;
			cui.AddTbBinding(control, controlPropertyName, record, property);
		}

		//-------------------------------------------------------------------------
		public static void AddTbBinding(this IUIControl control, string controlPropertyName, IDataManager dataManager, string fieldName)
		{
			TBWFCUIControl cui = control.CUI as TBWFCUIControl;
			cui.AddTbBinding(control, controlPropertyName, dataManager, fieldName);
		}

		//-------------------------------------------------------------------------
		public static void AddExtender(this IUIControl control, ITBUIExtenderProvider extenderProvider)
		{
			TBCUIControl controller = control.CUI as TBCUIControl;
			if (controller == null)
			{
				return;
			}

			controller.ExtendersManager.AddExtender(extenderProvider);
		}

		//-------------------------------------------------------------------------
		public static void AddExtender<T>(this IUIControl control) where T : ITBUIExtenderProvider
		{
			TBCUIControl controller = control.CUI as TBCUIControl;
			if (controller == null)
			{
				return;
			}

			ITBUIExtenderProvider extenderProvider =
				Activator.CreateInstance(typeof(T), controller) as ITBUIExtenderProvider;

			controller.ExtendersManager.AddExtender(extenderProvider);
		}

		//-------------------------------------------------------------------------
		public static void AddExtender(this UIGrid grid, string gridColumnName, ITBUIExtenderProvider extenderProvider)
		{
			TBWFCUIGrid controller = grid.CUI as TBWFCUIGrid;
			if (controller == null)
			{
				return;
			}

			controller.AddExtender(gridColumnName, extenderProvider);
		}

		//-------------------------------------------------------------------------
		public static void AddExtender<T>(this UIGrid grid, string gridColumnName) where T : ITBUIExtenderProvider
		{
			TBWFCUIGrid controller = grid.CUI as TBWFCUIGrid;
			if (controller == null)
			{
				return;
			}

			ITBUIExtenderProvider extenderProvider =
				Activator.CreateInstance(typeof(T), controller) as ITBUIExtenderProvider;

			controller.AddExtender(gridColumnName, extenderProvider);
		}

        //-------------------------------------------------------------------------
        public static void SetNumCharsPerLine(this UIGrid grid, string gridColumnName, int numCharsPerLine)
        {
            TBWFCUIGrid controller = grid.CUI as TBWFCUIGrid;
            if (controller == null)
            {
                return;
            }

            controller.SetNumCharsPerLine(gridColumnName, numCharsPerLine);
        }

        //-------------------------------------------------------------------------
        public static void SetFormatStyle(this IUIControl control, string formatStyleName)
        {
            TBWFCUIControl controller = control.CUI as TBWFCUIControl;
            if (controller == null)
            {
                return;
            }

            controller.FormatStyle = formatStyleName;
        }

        //-------------------------------------------------------------------------
        public static void SetFormatStyle(this UIGrid grid, string gridColumnName, string formatStyleName)
        {
            TBWFCUIGrid controller = grid.CUI as TBWFCUIGrid;
            if (controller == null)
            {
                return;
            }

            controller.SetFormatStyle(gridColumnName, formatStyleName);
        }
	}
}
