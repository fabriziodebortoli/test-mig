using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microarea.EasyBuilder.Packager;
using Microarea.TaskBuilderNet.Core.EasyBuilder;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.EasyBuilder
{
	internal class EBStaticFunctions
	{
		private static string[] allowedImagesExtensions = new string[] { ".jpg", ".png", ".bmp", ".gif" };

		//---------------------------------------------------------------------------------
		internal static List<string> ImportImages(IWin32Window parent, string easyBuilderCurrentPath)
		{
			List<string> importedImages = new List<string>();
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";
			ofd.Multiselect = true;
			ofd.ShowDialog();

			OverwriteResult overwriteResult = OverwriteResult.None;
			foreach (string current in ofd.FileNames)
			{
				FileInfo fi = new FileInfo(current);
				//copiamo nella cartella di modulo (standard o custom a seconda che sia una standardizzazione)
				string destination = Path.Combine(easyBuilderCurrentPath, fi.Name);
				if (!Directory.Exists(easyBuilderCurrentPath))
					Directory.CreateDirectory(easyBuilderCurrentPath);

				if (File.Exists(destination))
				{
					if (overwriteResult == OverwriteResult.NoToAll)
						continue;

					if (overwriteResult == OverwriteResult.YesToAll)
					{
						CopyFile(current, destination);
						continue;
					}

					OverwriteFileWindow or = new OverwriteFileWindow(destination);
					or.ShowDialog(parent);

					if (or.Result == OverwriteResult.NoToAll || or.Result == OverwriteResult.YesToAll)
						overwriteResult = or.Result;


					if (or.Result == OverwriteResult.No || or.Result == OverwriteResult.NoToAll)
					{
						continue;
					}

					if (or.Result == OverwriteResult.Yes || or.Result == OverwriteResult.YesToAll)
					{
						CopyFile(current, destination);
						continue;
					}

				}
				else
				{
					File.Copy(current, destination, true);
					importedImages.Add(destination);
				}
			}

			return importedImages;
		}

		//---------------------------------------------------------------------------------
		private static bool CopyFile(string current, string destination)
		{
			try
			{
				if (File.Exists(destination))
				{
					File.SetAttributes(destination, FileAttributes.Normal);
					File.Delete(destination);
				}
				File.Copy(current, destination, true);
				return true;
			}
			catch { return false; }
		}

		//---------------------------------------------------------------------------------
		internal static string GetCurrentEasyBuilderAppPath()
		{
			IEasyBuilderApp app = BaseCustomizationContext.CustomizationContextInstance.CurrentEasyBuilderApp;
			string path = PathFinderWrapper.GetImageFolderPath(app.ApplicationName,app.ModuleName);
			return path;
		}

		//---------------------------------------------------------------------------------
		internal static FileInfo[] GetImagesFiles(string path)
		{
			FileInfo[] files = new FileInfo[0];

			if (!Directory.Exists(path))
				return files;

			DirectoryInfo di = new DirectoryInfo(path);
			files = di.EnumerateFiles().Where(f => allowedImagesExtensions.Contains(f.Extension.ToLower())).ToArray();
			return files;
		}
	}
}
