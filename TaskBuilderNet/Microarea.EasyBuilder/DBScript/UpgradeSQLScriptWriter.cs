using System;
using System.IO;
using System.Text;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Summary description for UpgradeSQLScriptWriter.
	/// </summary>
	//================================================================================
	public class UpgradeSQLScriptWriter
	{
		private string mFilename = string.Empty;
		private string mFilenameOracle = string.Empty;
		private Tag mTag = null;

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public UpgradeSQLScriptWriter(string allScriptPath, string oracleScriptPath, bool aOnNewTable, Tag aTag)
		{
			mTag = aTag;

			if (aOnNewTable)
				mFilename = Path.Combine(allScriptPath, string.Format("{0}.sql", mTag.TablePhysicalName));
			else
				mFilename = Path.Combine(allScriptPath, string.Format("Alter_{0}.sql", mTag.TablePhysicalName));

			if (aOnNewTable)
				mFilenameOracle = Path.Combine(oracleScriptPath, string.Format("{0}.sql", mTag.TablePhysicalName));
			else
				mFilenameOracle = Path.Combine(oracleScriptPath, string.Format("Alter_{0}.sql", mTag.TablePhysicalName));
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public string GetFilename(bool bOracle)
		{
			if (bOracle)
				return mFilenameOracle;
			else
				return mFilename;
		}


		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool AddField()
		{
			bool bOk = true;

			bOk = AddFieldTemplate(false);
			bOk = bOk && AddFieldTemplate(true);
			return bOk;
		}

		//--------------------------------------------------------------------------------
		private bool AddFieldTemplate(bool bOracle)
		{
			mTag.Oracle = bOracle;
			Template t = null;
			if (bOracle)
				t = new Template("AddFieldOracle.sql", mTag, mFilenameOracle);
			else
				t = new Template("AddField.sql", mTag, mFilename);
			return t.Copy();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool EditField(bool bKeepConstraint)
		{
			bool bOk = EditFieldTemplate(false, bKeepConstraint);
			bOk = bOk && EditFieldTemplate(true, bKeepConstraint);
			return bOk;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		private bool EditFieldTemplate(bool bOracle, bool bKeepConstraint)
		{
			mTag.Oracle = bOracle;
			Template t = null;
			if (bOracle)
				t = new Template("EditFieldOracle.sql", mTag, mFilenameOracle);
			else
				if (bKeepConstraint)
					t = new Template("EditFieldKeepConstraint.sql", mTag, mFilename);
				else
					t = new Template("EditField.sql", mTag, mFilename);
			return t.Copy();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool DeleteField()
		{
			bool bOk = DeleteFieldTemplate(false);
			bOk = bOk && DeleteFieldTemplate(true);
			return bOk;
		}

		//--------------------------------------------------------------------------------
		private bool DeleteFieldTemplate(bool bOracle)
		{
			mTag.Oracle = bOracle;
			Template t = null;
			if (bOracle)
				t = new Template("DeleteFieldOracle.sql", mTag, mFilenameOracle);
			else
				t = new Template("DeleteField.sql", mTag, mFilename);
			return t.Copy();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public void AddUpdateScript()
		{
			if (mTag.UpdateScript != string.Empty)
				AddUpdateScript(false, mFilename);
			if (mTag.UpdateScriptOracle != string.Empty)
				AddUpdateScript(true, mFilenameOracle);
		}

		//--------------------------------------------------------------------------------
		private void AddUpdateScript(bool bOracle, string aFilename)
		{
			mTag.Oracle = bOracle;
			using (TempFile tf = new TempFile(aFilename))
			{
				bool bExists = tf.New();
				string path = Path.GetDirectoryName(aFilename);
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
				using (SignalStreamWriter writeStream = new SignalStreamWriter(aFilename))
				{

					if (bExists)
					{
						using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
						{
							string l = string.Empty;

							while ((l = textStream.ReadLine()) != null)
								writeStream.WriteLine(l);
						}
					}

					Template t = new Template("UpdateScript.sql", mTag);

					if (!t.Open())
						throw new ApplicationException(t.GetError());

					while (t.Read())
						writeStream.WriteLine(t.Line);

					t.Close();

					writeStream.Flush();
				}
			}
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool AddIndex()
		{
			bool bOk = AddIndexTemplate(false);
			bOk = bOk && AddIndexTemplate(true);
			return bOk;
		}

		//--------------------------------------------------------------------------------
		private bool AddIndexTemplate(bool bOracle)
		{
			mTag.Oracle = bOracle;
			Template t = null;
			if (bOracle)
				t = new Template("CreateIndexOracle.sql", mTag, mFilenameOracle);
			else
				t = new Template("CreateIndex.sql", mTag, mFilename);
			return t.Copy();
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool EditIndex()
		{
			bool bOk = DeleteIndex();
			bOk = bOk && AddIndex();
			return bOk;
		}

		//--------------------------------------------------------------------------------
		/// <remarks/>
		public bool DeleteIndex()
		{
			bool bOk = DeleteIndexTemplate(false);
			bOk = bOk && DeleteIndexTemplate(true);
			return bOk;
		}

		//--------------------------------------------------------------------------------
		private bool DeleteIndexTemplate(bool bOracle)
		{
			mTag.Oracle = bOracle;
			Template t = null;
			if (bOracle)
				t = new Template("DeleteIndexOracle.sql", mTag, mFilenameOracle);
			else
				t = new Template("DeleteIndex.sql", mTag, mFilename);
			return t.Copy();
		}
	}
}
