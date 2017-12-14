using System;
using System.IO;
using System.Reflection;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for Tag.
	/// </summary>
	//================================================================================
	public class Template
	{
		private string[] mTemplates = 
		{	
			"DblInterface.cpp", 
			"TableDefinition.cpp", 
			"TableDeclaration.h", 
			"TableSource.cpp",
			"TableHeader.h", 
			"CreateTable.sql",
			"CreateTableOracle.sql",
			"AddField.sql",
			"AddFieldOracle.sql",
			"EditField.sql",
			"EditFieldKeepConstraint.sql",
			"EditFieldOracle.sql",
			"DeleteField.sql",
			"DeleteFieldOracle.sql",
			"UpdateScript.sql",
			"CreateIndex.sql",
			"CreateIndexOracle.sql",
			"DeleteIndex.sql",
			"DeleteIndexOracle.sql"
	};

		private string mFilename = string.Empty;
		private string mError = string.Empty;
		private Tag mTag = null;

		private bool mOpened = false;
		private string mLine = string.Empty;
		StreamReader mTextStream = null;
		private string mTemplate = "";

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Template()
		{
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Template(string aTemplate, Tag aTag)
		{
			Initialize(aTemplate, aTag, string.Empty);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Template(string aTemplate, Tag aTag, string aFilename)
		{
			Initialize(aTemplate, aTag, aFilename);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Template(int aTemplateNumber, Tag aTag)
		{
			Initialize(mTemplates[aTemplateNumber], aTag, string.Empty);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public Template(int aTemplateNumber, Tag aTag, string aFilename)
		{
			Initialize(mTemplates[aTemplateNumber], aTag, aFilename);
		}

		//--------------------------------------------------------------------------------
		private void Initialize(string aTemplate, Tag aTag, string aFilename)
		{
			mTemplate = aTemplate;
			mTag = aTag;
			mFilename = aFilename;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Line
		{
			get
			{
				return mLine;
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string GetError()
		{
			return mError;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public int GetCount()
		{
			return mTemplates.GetUpperBound(0);
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Get(int i)
		{
			return mTemplates[i];
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool Copy()
		{
			if (mFilename == string.Empty)
				return false;

			using (StreamReader textStream = OpenTemplate(mTemplate))
			{
				string path = Path.GetDirectoryName(mFilename);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename, File.Exists(mFilename)))
				{

					string l = string.Empty;

					try
					{
						while ((l = textStream.ReadLine()) != null)
						{
							l = mTag.Replace(l);
							writeStream.WriteLine(l);
						}
						writeStream.Flush();
					}
					catch (Exception ex)
					{
						mError = ex.Message;
						return false;
					}
				}
			}
			return true;
		}

		//--------------------------------------------------------------------------------
		private StreamReader OpenTemplate(string templateName)
		{
			return new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Microarea.EasyBuilder.Templates." + templateName));
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string Load(int aTemplateNumber)
		{
			string t = string.Empty;

			using (StreamReader textStream = OpenTemplate(mTemplates[aTemplateNumber]))
			{

				string l = string.Empty;

				try
				{
					while ((l = textStream.ReadLine()) != null)
						t += (l + "\r\n");

					textStream.Close();
				}
				catch (Exception ex)
				{
					mError = ex.Message;
					return string.Empty;
				}
				return t;
			}
		}


		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool Open()
		{
			mTextStream = OpenTemplate(mTemplate);
			mOpened = true;
			return true;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool Read()
		{
			if (!mOpened)
				return false;

			try
			{
				if ((mLine = mTextStream.ReadLine()) != null)
				{
					mLine = mTag.Replace(mLine);
					return true;
				}
				else
					return false;
			}
			catch (Exception ex)
			{
				mError = ex.Message;
			}
			return false;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void Close()
		{
			mTextStream.Close();
			mTextStream = null;
			mOpened = false;
		}
	}
}
