// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using ICSharpCode.NRefactory.Completion;
using ICSharpCode.NRefactory.Editor;

namespace Microarea.EasyBuilder.CodeCompletion
{
	//=============================================================================================
	class CompletionData : ICompletionData, ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData
    {
		readonly List<ICompletionData> overloadedData = new List<ICompletionData>();
		public System.Windows.Media.ImageSource Image { get; set; }
		private double priority = 1;

		public string						TriggerWord			{ get; set; }
        public int							TriggerWordLength	{ get; set; }
		public CompletionCategory			CompletionCategory	{ get; set; }
        public string						DisplayText			{ get; set; }
        public virtual						string Description	{ get; set; }
        public string						CompletionText		{ get; set; }
        public DisplayFlags					DisplayFlags		{ get; set; }
		public bool							HasOverloads		{ get { return overloadedData.Count > 0; } }
		public IEnumerable<ICompletionData> OverloadedData		{ get { return overloadedData; } }

		//-------------------------------------------------------------------------------
		protected CompletionData()
		{
		}

		//-------------------------------------------------------------------------------
		public CompletionData(string text)
		{
			DisplayText = CompletionText = Description = text;
		}

		//-------------------------------------------------------------------------------
		public void AddOverload(ICompletionData data)
        {
            if (overloadedData.Count == 0)
                overloadedData.Add(this);
            overloadedData.Add(data);
        }

	
		//-------------------------------------------------------------------------------
		public virtual void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
			//TODOLUCA //TODOROBY, in originale c'era this.CompletionText, che nel caso di enums
			//era "Enum.NomeEnumerativo"
			textArea.Document.Replace(completionSegment, this.DisplayText);  
		}

		//-------------------------------------------------------------------------------
		public object Content
        {
            get { return DisplayText; }
        }

		//-------------------------------------------------------------------------------
		object ICSharpCode.AvalonEdit.CodeCompletion.ICompletionData.Description
        {
            get { return this.Description; }
        }

       
		//-------------------------------------------------------------------------------
		public virtual double Priority
        {
            get { return priority; }
            set { priority = value; }
        }

		//-------------------------------------------------------------------------------
		public string Text
        {
            get { return this.CompletionText; }
        }
		
		//-------------------------------------------------------------------------------
		public override string ToString()
        {
            return DisplayText;
        }

		//-------------------------------------------------------------------------------
		public override bool Equals(object obj)
        {
            var other = obj as CompletionData;
            return other != null && DisplayText == other.DisplayText;
        }

		//-------------------------------------------------------------------------------
		public override int GetHashCode()
        {
            return DisplayText.GetHashCode();
        }
    }
}
