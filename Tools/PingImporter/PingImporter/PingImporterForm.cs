using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Microarea.Internals.PingImporter
{
	//=========================================================================
	public partial class PingImporterForm : Form
	{
		int filesCount;
		FileInfo[] fileInfos;
		long startTicks;
		int executed = 0;
		private CancellationTokenSource token;

		//---------------------------------------------------------------------
		public PingImporterForm()
		{
			InitializeComponent();

			btnStopImport.Enabled = false;
			btnImport.Enabled = false;
		}

		//---------------------------------------------------------------------
		private void FileSelectionButton_Click(object sender, EventArgs e)
		{
			FolderBrowserDialog fbd = new FolderBrowserDialog();
			fbd.ShowNewFolderButton = false;
			if (fbd.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
			{
				txtPingsPath.Text = fbd.SelectedPath;
				DirectoryInfo dirInfo = new DirectoryInfo(fbd.SelectedPath);
				fileInfos = dirInfo.GetFiles();
				filesCount = fileInfos.Length;
				txtPingsPath_Leave(this, EventArgs.Empty);
			}
		}

		//---------------------------------------------------------------------
		private void ImportButton_Click(object sender, EventArgs e)
		{
			double pingPerSeconds = 0D;
			if (!Double.TryParse(txtPingRate.Text.Replace(".", ","), out pingPerSeconds))
			{
				Task.Factory.StartNew(new Action(
					() => BlinkForError(txtPingRate)
					));
				return;
			}

			startTicks = Environment.TickCount;

			ckbMaxRate.Enabled = false;
			btnImport.Enabled = false;
			btnStopImport.Enabled = true;
			btnFileSelection.Enabled = false;
			
			txtUrl.Enabled = false;
			txtPingRate.Enabled = false;
			txtOutput.Clear();
			progressTasksCounter.Value = 0;

			double rate = 1000 / pingPerSeconds;
			string url = txtUrl.Text;
			int itemsCount = filesCount;

			executed = 0;
			token = new CancellationTokenSource();
			Task importTask = Task.Factory.StartNew(
				new Action
				(
				()
				=>
				{
					using (PingImporterSvc.Registration r = new PingImporterSvc.Registration())
					{
						r.Url = url;

						DateTime pingTimeStamp = DateTime.MinValue;
						string pingInfo = null;
						PingImporterSvc.ResponseBag response = null;
						for (int i = 0; i < itemsCount; i++)
						{
							pingInfo = GetPingInfo(i);
							pingTimeStamp = GetPingTimeStamp(i);
							Task.Factory.StartNew(
								new Action
								(
								()
								=>
								{
									try
									{
										if (token.Token.IsCancellationRequested)
										{
											return;
										}
										this.BeginInvoke(new Action(() => { progressTasksCounter.PerformStep(); }));
										response = r.ImportPing("1CCAF7DA-6289-422E-B907-9F07270D8424", pingTimeStamp, pingInfo);

										if (response.ReturnCode != 0)
											this.BeginInvoke(new Action(() => { txtOutput.AppendText(response.ReturnCodeExpl); txtOutput.AppendText(Environment.NewLine); }));
									}
									catch (Exception exc)
									{
										this.BeginInvoke(new Action(() => { txtOutput.AppendText(exc.ToString()); txtOutput.AppendText(Environment.NewLine); }));
									}

									if (Interlocked.Increment(ref executed) == filesCount)
										this.BeginInvoke(new Action(() => { TakeEndTimeAndPrintStatistics(); }));
								}
								),
								token.Token);
							
							if (rate > 0 && !ckbMaxRate.Checked)
								System.Threading.Thread.Sleep(Convert.ToInt32(rate));

							if (token.Token.IsCancellationRequested)
							{
								break;
							}
						}
					}
				}
				),
				token.Token
				);
		}

		private void BlinkForError(TextBox txtPingRate)
		{
			Color originalBackColor = txtPingRate.BackColor;
			for (int i = 0; i < 3; i++)
			{
				this.BeginInvoke(new Action(
					() => txtPingRate.BackColor = Color.Red
					));
				
				Thread.Sleep(150);

				this.BeginInvoke(new Action(
					() => txtPingRate.BackColor = originalBackColor
					));

				Thread.Sleep(150);
			}
		}


		//---------------------------------------------------------------------
		private void TakeEndTimeAndPrintStatistics()
		{
			long endTicks = Environment.TickCount;

			progressTasksCounter.Value = progressTasksCounter.Maximum;

			ckbMaxRate.Enabled = true;
			btnImport.Enabled = true;
			btnFileSelection.Enabled = true;
			btnStopImport.Enabled = false;

			txtUrl.Enabled = true;
			if (!ckbMaxRate.Checked)
				txtPingRate.Enabled = true;

			TimeSpan importTime = new TimeSpan(((endTicks - startTicks) * 10000L));

			txtOutput.AppendText(Environment.NewLine);
			txtOutput.AppendText(executed.ToString() + " files imported in " + importTime.ToString("c"));
			txtOutput.AppendText(Environment.NewLine);
			txtOutput.AppendText((executed / (double)importTime.TotalSeconds).ToString("N") + " files per second");
		}

		//---------------------------------------------------------------------
		private string GetPingInfo(int index)
		{
			using (StreamReader sr = new StreamReader(fileInfos[index].FullName))
				return sr.ReadToEnd();
		}

		//---------------------------------------------------------------------
		private DateTime GetPingTimeStamp(int index)
		{
			string filePath = fileInfos[index].Name;
			string[] tokens = Path.GetFileName(filePath).Split(new char[]{'_', '.'});

			int year = 0;
			bool ok = Int32.TryParse(tokens[2], out year);

			int month = 0;
			ok = Int32.TryParse(tokens[3], out month);

			int day = 0;
			ok = Int32.TryParse(tokens[4], out day);

			int hours24 = 0;
			ok = Int32.TryParse(tokens[6], out hours24);

			int minutes = 0;
			ok = Int32.TryParse(tokens[7], out minutes);

			int secs = 0;
			ok = Int32.TryParse(tokens[8], out secs);

			return new DateTime(year, month, day, hours24, minutes, secs);
		}

		//---------------------------------------------------------------------
		private void btnStopImport_Click(object sender, EventArgs e)
		{
			TakeEndTimeAndPrintStatistics();

			this.token.Cancel();
			btnStopImport.Enabled = false;

			btnFileSelection.Enabled = true;
			
			txtUrl.Enabled = true;
			txtPingRate.Enabled = true;

			txtOutput.AppendText("Importazione interrotta");
			txtOutput.AppendText(Environment.NewLine);
		}

		//---------------------------------------------------------------------
		private void ckbMaxRate_CheckedChanged(object sender, EventArgs e)
		{
			txtPingRate.Enabled = !ckbMaxRate.Checked;
		}

		//---------------------------------------------------------------------
		private void informazioniToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Version v = this.GetType().Assembly.GetName().Version;
			string version = v.ToString(4);

			DateTime start = new DateTime(2000, 1, 1);
			start = start.AddDays(v.Build);
			start = start.AddSeconds(v.Revision * 2);

			MessageBox.Show(this,
				String.Format(" Versione {0} del {1}", version, start.ToString("dd/MM/yyyy HH:mm:ss")),
				"Informazioni su...", MessageBoxButtons.OK, MessageBoxIcon.Information
				);
		}

		//---------------------------------------------------------------------
		private void txtPingsPath_Leave(object sender, EventArgs e)
		{
			btnImport.Enabled = filesCount > 0;
			progressTasksCounter.Maximum = filesCount * progressTasksCounter.Step;
		}
	}
}
