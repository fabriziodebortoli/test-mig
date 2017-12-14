using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	/// <summary>
	/// Descrizione di riepilogo per SimpleControl.
	/// </summary>
	public class SimpleControl : Control, INamingContainer//, IPostBackDataHandler
	{

		//--------------------------------------------------------------------------
		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
		}
		//--------------------------------------------------------------------------
		protected void submit_Click(object sender, EventArgs e)
		{
			enumera(this);
		}
		
		//--------------------------------------------------------------------------
		protected void cancel_Click(object sender, EventArgs e)
		{
		}
		
		//--------------------------------------------------------------------------
		protected void enumera(Control xx)
		{
			foreach (Control ct in xx.Controls)
			{
				string nn = ct.ID;
				if (ct.Controls.Count > 0)
					enumera(ct);
			}
		}
		

//		//--------------------------------------------------------------------------
//		bool IPostBackDataHandler.LoadPostData(string key, NameValueCollection postCollection)
//		{
//			return true;
//		}
//
//
//		//--------------------------------------------------------------------------
//		void IPostBackDataHandler.RaisePostDataChangedEvent()
//		{
//		}

		//--------------------------------------------------------------------------
		protected override void CreateChildControls()
		{
			LiteralControl text = new LiteralControl("<h1> Prova di SimpleControl </h1>");
			Controls.Add(text);

			TextBox txt = new TextBox();
			txt.Text = "aaa";
			Controls.Add(txt);

			Button submit = new Button();
			submit.Text = "submit";
			Controls.Add(submit);
			submit.Click += new EventHandler(this.submit_Click);

			Button cancel = new Button();
			cancel.Text = "Cancel";
			Controls.Add(cancel);
			cancel.Click += new EventHandler(this.cancel_Click);
		}
	}
}
