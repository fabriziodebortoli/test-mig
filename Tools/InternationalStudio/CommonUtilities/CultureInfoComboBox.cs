using System.Globalization;
using System.Windows.Forms;

namespace Microarea.Tools.TBLocalizer.CommonUtilities
{

	//================================================================================
	public class CultureInfoComboBox : ComboBox
	{
		public const string	CultureInfoCode			= "Name";	
		public const string	CultureInfoDescription	= "EnglishName";	
		
		CultureInfo[] cultures;

		//--------------------------------------------------------------------------------
		public CultureInfo[] Cultures { set { this.cultures = value; } }

		//--------------------------------------------------------------------------------
		public string[] CultureStrings 
		{
			set 
			{
				this.cultures = new CultureInfo[value.Length];
				for (int i = 0; i < value.Length; i++)
					this.cultures[i] = new CultureInfo(value[i]);
			} 
		}

		//--------------------------------------------------------------------------------
		public CultureInfoComboBox()
		{
			this.DropDownStyle = ComboBoxStyle.DropDownList;
		}
		
		//--------------------------------------------------------------------------------
		public CultureInfoComboBox(CultureInfo[] cultures)
		{
			this.Cultures = cultures;
		}

		//--------------------------------------------------------------------------------
		public CultureInfoComboBox(string[] cultures)
		{
			this.CultureStrings = cultures;
		}

		//--------------------------------------------------------------------------------
		protected override void OnCreateControl()
		{
			if (!DesignMode)
			{
				DataSource = cultures;
				DisplayMember	= CultureInfoDescription;
				ValueMember		= CultureInfoCode;
			
			}
			base.OnCreateControl ();
		}

	}
}
