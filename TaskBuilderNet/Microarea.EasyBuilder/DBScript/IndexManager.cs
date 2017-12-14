
namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for Tag.
	/// </summary>
	public class IndexManager
	{
		private string 	mName = string.Empty;

		private string 	mField1 = string.Empty;
		private string 	mField2 = string.Empty;
		private string 	mField3 = string.Empty;
		private string 	mField4 = string.Empty;
		private string 	mField5 = string.Empty;
		private string 	mField6 = string.Empty;
		private string 	mField7 = string.Empty;
		private string 	mField8 = string.Empty;
		private string 	mField9 = string.Empty;
		
		//-----------------------------------------------------------------------------
		/// <remarks/>
		public IndexManager(string aIndexName)
		{
			Name = aIndexName;
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Name
		{
			get
			{
				return mName;
			}

			set
			{
				mName = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field1
		{
			get
			{
				return mField1;
			}

			set
			{
				mField1 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field2
		{
			get
			{
				return mField2;
			}

			set
			{
				mField2 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field3
		{
			get
			{
				return mField3;
			}

			set
			{
				mField3 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field4
		{
			get
			{
				return mField4;
			}

			set
			{
				mField4 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field5
		{
			get
			{
				return mField5;
			}

			set
			{
				mField5 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field6
		{
			get
			{
				return mField6;
			}

			set
			{
				mField6 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field7
		{
			get
			{
				return mField7;
			}

			set
			{
				mField7 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field8
		{
			get
			{
				return mField8;
			}

			set
			{
				mField8 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string Field9
		{
			get
			{
				return mField9;
			}

			set
			{
				mField9 = value;
			}
		}

		//-----------------------------------------------------------------------------
		/// <remarks/>
		public string GetFields(bool bOracle)
		{
			if (mField1 == string.Empty)
				return string.Empty;

			string i = string.Empty;

			if (bOracle)
			{
				i = "\"" + mField1.ToUpper() + "\"";
				if (mField2 == string.Empty)
					return i;
				i = i + ", \"" + mField2.ToUpper() + "\"";
				if (mField3 == string.Empty)
					return i;
				i = i + ", \"" + mField3.ToUpper() + "\"";
				if (mField4 == string.Empty)
					return i;
				i = i + ", \"" + mField4.ToUpper() + "\"";
				if (mField5 == string.Empty)
					return i;
				i = i + ", \"" + mField5.ToUpper() + "\"";
				if (mField6 == string.Empty)
					return i;
				i = i + ", \"" + mField6.ToUpper() + "\"";
				if (mField7 == string.Empty)
					return i;
				i = i + ", \"" + mField7.ToUpper() + "\"";
				if (mField8 == string.Empty)
					return i;
				i = i + ", \"" + mField8.ToUpper() + "\"";
				if (mField9 == string.Empty)
					return i;
				return i + ", \"" + mField9.ToUpper() + "\"";
			}
			else
			{
				i = "[" + mField1 + "]";
				if (mField2 == string.Empty)
					return i;
				i = i + ", [" + mField2 + "]";
				if (mField3 == string.Empty)
					return i;
				i = i + ", [" + mField3 + "]";
				if (mField4 == string.Empty)
					return i;
				i = i + ", [" + mField4 + "]";
				if (mField5 == string.Empty)
					return i;
				i = i + ", [" + mField5 + "]";
				if (mField6 == string.Empty)
					return i;
				i = i + ", [" + mField6 + "]";
				if (mField7 == string.Empty)
					return i;
				i = i + ", [" + mField7 + "]";
				if (mField8 == string.Empty)
					return i;
				i = i + ", [" + mField8 + "]";
				if (mField9 == string.Empty)
					return i;
				return i + ", [" + mField9 + "]";
			}
		}
	}
}
