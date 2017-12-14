using System.IO;
using System.Text;

namespace Microarea.EasyBuilder.DBScript
{
	/// <summary>
	/// Writer for Create sql script..
	/// </summary>
	//================================================================================
	public class CreateSQLScriptWriter
	{
		private string mFilename = string.Empty;
		private string mFilenameOracle = string.Empty;
		private Tag mTag = null;

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of CreateSQLScriptWriter
		/// </summary>
		/// <param name="aDatabaseScriptPath">Path for the database script</param>
		/// <param name="aTag">Tag for the script.</param>
		/// <seealso cref="Microarea.EasyBuilder.DBScript.Tag"/>
		public CreateSQLScriptWriter(string aDatabaseScriptPath, Tag aTag)
		{
			mTag = aTag;

			string aFolder = aDatabaseScriptPath;
			aFolder = Path.Combine(aFolder, "Create");

			FileFinder ff = new FileFinder();

			mFilename = Path.Combine(aFolder, "All");
			mFilename = ff.Find(mFilename, "*.sql", "CREATE TABLE", "[" + mTag.TablePhysicalName + "]");

			if (mFilename == string.Empty)
			{
				mFilename = Path.Combine(aFolder, "All");
				mFilename = Path.Combine(mFilename, mTag.TablePhysicalName + ".sql");
			}

			mFilenameOracle = Path.Combine(aFolder, "Oracle");
			mFilenameOracle = ff.Find(mFilenameOracle, "*.sql", "CREATE TABLE", "\"" + mTag.TablePhysicalName.ToUpper() + "\"");

			if (mFilenameOracle == string.Empty)
			{
				mFilenameOracle = Path.Combine(aFolder, "Oracle");
				mFilenameOracle = Path.Combine(mFilenameOracle, mTag.TablePhysicalName + ".sql");
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Returns the file name for the script
		/// </summary>
		/// <param name="bOracle">True is for Oracle, otherwise false.</param>
		/// <returns>the file name for the script</returns>
		public string GetFilename(bool bOracle)
		{
			if (bOracle)
				return mFilenameOracle;
			else
				return mFilename;
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Adds a field management to the script.
		/// </summary>
		/// <param name="bPrimaryKey">True if the field is primary key, otherwise false.</param>
		public void AddField(bool bPrimaryKey)
		{
			mTag.Oracle = false;
			AddFieldAll(bPrimaryKey);
			mTag.Oracle = true;
			AddFieldOracle(bPrimaryKey);
			mTag.Oracle = false;
		}

		//--------------------------------------------------------------------------------
		private void AddFieldAll(bool bPrimaryKey)
		{
			using (TempFile tf = new TempFile(mFilename))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename))
					{

						string l = string.Empty;
						string c = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;
						bool bFoundStartPK = false;
						bool bFoundEndPK = false;

						while ((l = textStream.ReadLine()) != null)
						{
							c = string.Empty;

							if (bFoundStart && !bFoundEnd)
							{
								bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

								if (bFoundEnd)
									if (bPrimaryKey)
										writeStream.WriteLine("    [" + mTag.FieldPhysicalName + "] " + mTag.Type + " NOT NULL,");
									else
										writeStream.WriteLine("    [" + mTag.FieldPhysicalName + "] " + mTag.Type + " NULL CONSTRAINT " + mTag.Constraint + " DEFAULT " + mTag.DefaultValue + ",");
							}

							if (bPrimaryKey && bFoundStartPK && !bFoundEndPK)
							{
								bFoundEndPK = (l.IndexOf(")") != -1 && l.IndexOf("(") == -1);

								if (bFoundEndPK)
									writeStream.WriteLine("        [" + mTag.FieldPhysicalName + "]");
								else if (l.IndexOf("(") == -1 && l.IndexOf(",") == -1)
									c = ",";
							}

							writeStream.WriteLine(l + c);

							if (!bFoundStart)
								bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalName) != -1);

							if (!bFoundStartPK)
								bFoundStartPK = (l.IndexOf("CONSTRAINT") != -1 && l.IndexOf("PRIMARY KEY") != -1);
						}
						writeStream.Flush();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void AddFieldOracle(bool bPrimaryKey)
		{
			using (TempFile tf = new TempFile(mFilenameOracle))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilenameOracle))
					{

						string l = string.Empty;
						string c = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;
						bool bFoundStartPK = false;
						bool bFoundEndPK = false;

							while ((l = textStream.ReadLine()) != null)
							{
								c = string.Empty;

								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

									if (bFoundEnd)
										if (bPrimaryKey)
											writeStream.WriteLine("    \"" + mTag.FieldPhysicalNameOracle + "\" " + mTag.TypeOracle + " NOT NULL,");
										else
											writeStream.WriteLine("    \"" + mTag.FieldPhysicalNameOracle + "\" " + mTag.TypeOracle + " DEFAULT " + mTag.DefaultValueOracle + ",");
								}

								if (bPrimaryKey && bFoundStartPK && !bFoundEndPK)
								{
									bFoundEndPK = (l.IndexOf(")") != -1 && l.IndexOf("(") == -1);

									if (bFoundEndPK)
										writeStream.WriteLine("        \"" + mTag.FieldPhysicalNameOracle + "\"");
									else if (l.IndexOf("(") == -1 && l.IndexOf(",") == -1)
										c = ",";
								}

								writeStream.WriteLine(l + c);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalNameOracle) != -1);

								if (!bFoundStartPK)
									bFoundStartPK = (l.IndexOf("CONSTRAINT") != -1 && l.IndexOf("PRIMARY KEY") != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Modifies a field.
		/// </summary>
		public void EditField()
		{
			mTag.Oracle = false;
			EditFieldAll();
			mTag.Oracle = true;
			EditFieldOracle();
			mTag.Oracle = false;
		}

		//--------------------------------------------------------------------------------
		private void EditFieldAll()
		{
			using (TempFile tf = new TempFile(mFilename))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

									if (l.IndexOf("[" + mTag.FieldPhysicalName + "]") != -1)
										writeStream.WriteLine("    [" + mTag.FieldPhysicalName + "] " + mTag.Type + " NULL CONSTRAINT " + mTag.Constraint + " DEFAULT " + mTag.DefaultValue + ",");
									else
										writeStream.WriteLine(l);
								}
								else
									writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalName) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void EditFieldOracle()
		{
			using (TempFile tf = new TempFile(mFilenameOracle))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilenameOracle))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

						while ((l = textStream.ReadLine()) != null)
						{
							if (bFoundStart && !bFoundEnd)
							{
								bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

								if (l.IndexOf("\"" + mTag.FieldPhysicalNameOracle + "\"") != -1)
									writeStream.WriteLine("    \"" + mTag.FieldPhysicalNameOracle + "\" " + mTag.TypeOracle + " DEFAULT " + mTag.DefaultValueOracle + ",");
								else
									writeStream.WriteLine(l);
							}
							else
								writeStream.WriteLine(l);

							if (!bFoundStart)
								bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalNameOracle) != -1);
						}
						writeStream.Flush();
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Deletes a field.
		/// </summary>
		public void DeleteField()
		{
			mTag.Oracle = false;
			DeleteFieldAll();
			mTag.Oracle = true;
			DeleteFieldOracle();
			mTag.Oracle = false;
		}

		//--------------------------------------------------------------------------------
		private void DeleteFieldAll()
		{
			using (TempFile tf = new TempFile(mFilename))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

									if (l.IndexOf("[" + mTag.TablePhysicalName + "]") == -1)
										writeStream.WriteLine(l);
								}
								else
									writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalName) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void DeleteFieldOracle()
		{
			using (TempFile tf = new TempFile(mFilenameOracle))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilenameOracle))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("PRIMARY KEY") != -1);

									if (l.IndexOf("\"" + mTag.TablePhysicalNameOracle + "\"") == -1)
										writeStream.WriteLine(l);
								}
								else
									writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalNameOracle) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Adds an index to the script.
		/// </summary>
		public void AddIndex()
		{
			mTag.Oracle = false;
			AddIndexAll();
			mTag.Oracle = true;
			AddIndexOracle();
			mTag.Oracle = false;
		}

		//--------------------------------------------------------------------------------
		private void AddIndexAll()
		{
			using (TempFile tf = new TempFile(mFilename))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("END") != -1);

									if (bFoundEnd)
									{
										writeStream.WriteLine("CREATE INDEX [" + mTag.IndexName + "] ON [dbo].[" + mTag.TablePhysicalName + "] (" + mTag.IndexFields + ") ON [PRIMARY]");
										writeStream.WriteLine("");
									}
								}
								writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalName) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void AddIndexOracle()
		{
			using (TempFile tf = new TempFile(mFilenameOracle))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilenameOracle))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundEnd = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart && !bFoundEnd)
								{
									bFoundEnd = (l.IndexOf("GO") != -1);

									if (bFoundEnd)
									{
										writeStream.WriteLine("GO");
										writeStream.WriteLine("");
										writeStream.WriteLine("CREATE INDEX \"" + mTag.IndexNameOracle + "\" ON \"" + mTag.TablePhysicalNameOracle + "\" (" + mTag.IndexFieldsOracle + ")");
									}
								}
								writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalNameOracle) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Modifies the index
		/// </summary>
		public void EditIndex()
		{
			DeleteIndex();
			AddIndex();
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Deletes the index
		/// </summary>
		public void DeleteIndex()
		{
			mTag.Oracle = false;
			DeleteIndexAll();
			mTag.Oracle = true;
			DeleteIndexOracle();
			mTag.Oracle = false;
		}

		//--------------------------------------------------------------------------------
		private void DeleteIndexAll()
		{
			using (TempFile tf = new TempFile(mFilename))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilename))
					{

						string l = string.Empty;
						bool bFoundStart = false;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart)
								{
									if (l.IndexOf(mTag.IndexName) != -1 && l.IndexOf("INDEX") != -1)
										continue;
									else
										writeStream.WriteLine(l);
								}
								else
									writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalName) != -1);
							}
							writeStream.Flush();
					}
				}
			}
		}

		//--------------------------------------------------------------------------------
		private void DeleteIndexOracle()
		{
			using (TempFile tf = new TempFile(mFilenameOracle))
			{
				tf.New();

				using (StreamReader textStream = new StreamReader(tf.GetTempFilename(), Encoding.Default))
				{
					using (SignalStreamWriter writeStream = new SignalStreamWriter(mFilenameOracle))
					{

						string l = string.Empty;
						bool bFoundStart = false;
						bool bFoundIndex = false;
						bool bFoundGo = false;
						bool bSkipLines = true;

							while ((l = textStream.ReadLine()) != null)
							{
								if (bFoundStart)
								{
									if (bSkipLines)
										bFoundGo = (bFoundIndex && (l.IndexOf("GO") != -1));
									else
										bFoundGo = false;

									if (bSkipLines)
										bFoundIndex = (l.IndexOf(mTag.IndexNameOracle) != -1 && l.IndexOf("INDEX") != -1);
									else
										bFoundIndex = false;

									if (bFoundGo)
										bSkipLines = false;

									if (!bFoundIndex && !bFoundGo)
										writeStream.WriteLine(l);
								}
								else
									writeStream.WriteLine(l);

								if (!bFoundStart)
									bFoundStart = (l.IndexOf("CREATE TABLE") != -1 && l.IndexOf(mTag.TablePhysicalNameOracle) != -1);
							}
							writeStream.Flush();
					}

				}
			}
		}

		//-----------------------------------------------------------------------------
		/// <summary>
		/// Creates a new script
		/// </summary>
		/// <param name="bOracle">True if the generated script is for Oracle, otherwise false.</param>
		public void New(bool bOracle)
		{
			mTag.Oracle = bOracle;
			Template t = null;

			if (bOracle)
				t = new Template("CreateTableOracle.sql", mTag, mFilenameOracle);
			else
				t = new Template("CreateTable.sql", mTag, mFilename);
			t.Copy();
		}

	}
}
