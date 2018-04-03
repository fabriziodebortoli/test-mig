using Microarea.Common.NameSolver;
using System;
using System.Collections.Generic;
using System.IO;




namespace Microarea.Common.Generic
{
    /// <summary>
    /// Classe per la gestione delle immagini
    /// </summary>
    /// <remarks>
    /// Si occupa della generazione dei file di immagine a partire dallo stream di byte ricevuti da tbloader.
    /// Gestisce una cache in memoria per minimizzare gli accessi a file system
    /// </remarks>
    public static class ImagesHelper
	{
		public const string TempImagesUrl = @".\Files\Temp";
		private static string tmpImagesPath = "";
		public static string TempImagesPath 
		{
			get
			{
                //if (tmpImagesPath == "" && HttpContext.Current != null)                              TODO rsweb
                //	tmpImagesPath = HttpContext.Current.Server.MapPath(TempImagesUrl);
                return tmpImagesPath;
			}
		}

		/// <summary>
		/// Cache dei file gia` presenti su disco
		/// </summary>
		private static List<string> FileCache = new List<string>();
		/// <summary>
		/// Semaforo per l'accesso alla cache dei file
		/// </summary>
		//private static ReaderWriterLock lockTicket = new ReaderWriterLock();        TODO rsweb
		private const string Disabled = "dis.";

        //--------------------------------------------------------------------------------------
        /*
                public static Bitmap CreateDisabled(this Bitmap bmp)
                {
                    Bitmap bmpDisabled = null;
                    try
                    {
                        bmpDisabled = (Bitmap)bmp.Clone();
                        using (Graphics gr = Graphics.FromImage(bmpDisabled))
                        {
                            System.Windows.Forms.ControlPaint.DrawImageDisabled(gr, bmp, 0, 0, Color.DarkGray);
                        }
                        return bmpDisabled;
                    }
                    catch
                    {
                        return bmpDisabled;
                    }
                }
        */
        //--------------------------------------------------------------------------------------
        //public static void LoadBitmapWithoutLockFile(string file)
        //{
        //Image image;
        //using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))                         TODO rsweb
        //{
        //	image = Image.FromStream(stream);
        //	return new Bitmap(image);
        //}
        //}

        //--------------------------------------------------------------------------------------
        //public static Image LoadImageWithoutLockFile(string file)
        //{
        //	Image image;
        //	using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))                        TODO rsweb
        //	{
        //		image = Image.FromStream(stream);
        //		return image;
        //	}
        //}

        //--------------------------------------------------------------------------------
        /*
                public static Bitmap GetStaticImage(string imageName, Type referringType)
                {
                    if (IsDisabled(imageName))
                        return GetStaticDisabledImage(imageName.Substring(Disabled.Length), referringType);

                    return GetEmbeddedImage(imageName, referringType);
                }
        */
        //--------------------------------------------------------------------------------
        public static bool IsDisabled(string imageName)
		{
			return imageName.StartsWith(Disabled);
		}

		//--------------------------------------------------------------------------------
		public static string PrefixedName(string imageName, bool enabled)
		{
			return enabled ? imageName : Disabled + imageName;
		}

        //--------------------------------------------------------------------------------
        /// <summary>
        /// Restituisce un'immagine statica incorporata nell'assembly
        /// </summary>
        //private static Bitmap GetEmbeddedImage(string image, Type referringType)
        //{
        //	string ns = string.Format("{0}.Images.{1}", referringType.Namespace, image);
        //	Stream s = referringType.Assembly.GetManifestResourceStream(ns);
        //	if (s == null)                                                                                                  TODO rsweb
        //		throw new ApplicationException(string.Format(GenericStrings.ResourceNotFound, ns));
        //	Bitmap original = Image.FromStream(s) as Bitmap;
        //	switch (original.PixelFormat)
        //	{
        //		//GDI+ non gestisce immagini indexed, quindi se per caso quella originaria lo fosse 
        //		//occorre trasformarla in una non indexed.
        //		case System.Drawing.Imaging.PixelFormat.Undefined:
        //		case System.Drawing.Imaging.PixelFormat.Format1bppIndexed:
        //		case System.Drawing.Imaging.PixelFormat.Format4bppIndexed:
        //		case System.Drawing.Imaging.PixelFormat.Format8bppIndexed:
        //		case System.Drawing.Imaging.PixelFormat.Format16bppGrayScale:
        //		case System.Drawing.Imaging.PixelFormat.Format16bppArgb1555:

        //			Bitmap modified = new Bitmap(original);
        //			original.Dispose();
        //			return modified;

        //		default:
        //			return original;
        //	}
        //}
        //--------------------------------------------------------------------------------
        /*
                private static Bitmap GetStaticDisabledImage(string image, Type referringType)
                {
                    using (Bitmap bmp = GetStaticImage(image, referringType))
                        return bmp.CreateDisabled();
                }

                //--------------------------------------------------------------------------------
                /// <summary>
                /// Ritorna l'url di un'immagine recuperata dalle embedded resources dell'assembly in cui 
                /// e` definito il tipo di riferimento, usando come namespace la concatenazione del namespace del tipo
                /// di riferimento + 'Image' + il nome dell'immagine; se l'immagine su file system non esiste la crea
                /// </summary>
                /// <param name="imageFile">Nome dell'immagine</param>
                /// <param name="referringType">Tipo di riferimento</param>
                /// <returns></returns>
                public static string CreateImageAndGetUrl(string imageFile, Type referringType)
                {
                    Diagnostic diagnostic = new Diagnostic(Diagnostic.EventLogName, true);

                    try
                    {
                        //prima cerco nella cache in memoria
                        if (!HasImageInCache(imageFile))
                        {
                            string file = GetImagePath(imageFile);
                            //poi su file system
                            if (!PathFinder.PathFinderInstance.ExistFile(file))
                            {
                                string folder = Path.GetDirectoryName(file);
                                if (!Directory.Exists(folder))
                                    Directory.CreateDirectory(folder);

                                using (Bitmap bmp = GetStaticImage(imageFile, referringType))
                                    if (bmp != null)
                                    {
                                        try
                                        {
                                            bmp.Save(file);
                                        }
                                        catch(Exception) //#20425 se non si riesce a salvare l'immagine per qualche motivo non blocca tutto il caricamento del menu
                                        {
                                            diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(GenericStrings.ErrorSavingFile, file));
                                        }
                                    }
                            }
                            //infine aggiungo alla cache
                            AddImageToCache(imageFile);
                        }
                        string imagePath = Path.Combine(TempImagesUrl, imageFile);
                        // Per ottenere un indirizzo Url ben formato devo sostituire \ (backslash) con / (forward slash)
                        return imagePath.Replace('\\', '/');
                    }
                    catch (Exception ex)
                    {
                        diagnostic.Set(DiagnosticType.LogInfo | DiagnosticType.Error, string.Format(GenericStrings.ErrorRetrievingImage, imageFile, referringType.Assembly.FullName, ex.Message));
                        return string.Empty;
                    }
                }
        */
        //--------------------------------------------------------------------------------
        /// <summary>
        /// Restituisce l'url del file di immagini
        /// </summary>
        /// <param name="imageFile">Nome del file di immagine</param>
        public static string GetImageUrl(string imageFile)
		{
			// Per ottenere un indirizzo Url ben formato devo sostituire \ (backslash) con / (forward slash)
			string imagePath = Path.Combine(TempImagesUrl, imageFile);	
			return imagePath.Replace('\\', '/');
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Restituisce il path completo del file di immagine
		/// </summary>
		public static string GetImagePath(string imageFile)
		{
            if (!PathFinder.PathFinderInstance.ExistPath(TempImagesPath))
                PathFinder.PathFinderInstance.CreateFolder(TempImagesPath, true);

            return Path.Combine(TempImagesPath, imageFile);
		}

		//---------------------------------------------------------------------
		/// <summary>
		/// Copia le immagini in cache se non gia' presenti
		/// </summary>
		public static string CopyImageFileIfNeeded(string appImageFile)
		{
			if (!string.IsNullOrEmpty(appImageFile))
			{
				string imageName = Path.GetFileName(appImageFile);
				if (!ImagesHelper.HasImageInCache(imageName))
				{
					if (PathFinder.PathFinderInstance.ExistFile(appImageFile))
					{
						string file = ImagesHelper.GetImagePath(imageName);

                        if (!PathFinder.PathFinderInstance.ExistFile(file))
						{
							try
							{
                                PathFinder.PathFinderInstance.CopyFile(appImageFile, file, false);
							}
							catch (Exception)
							{

							}
						}
					}
					ImagesHelper.AddImageToCache(imageName);
				}
				return ImagesHelper.GetImageUrl(imageName);
			}
			return string.Empty;
		}

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Aggiunge un'immagine alla cache dei file esistenti su disco
		/// </summary>
		public static void AddImageToCache(string imageName)
		{
            //lockTicket.AcquireWriterLock(Timeout.Infinite);      TODO rsweb
            try
            {
				if (!FileCache.Contains(imageName))
					FileCache.Add(imageName);
			}
			finally
			{
                //lockTicket.ReleaseLock();                             TODO rsweb
            }
        }

		//--------------------------------------------------------------------------------
		/// <summary>
		/// Controlla se l'immagine e' gia` presente in cache per evitare di rigenerarla o di accedere al file system
		/// </summary>
		public static bool HasImageInCache(string imageName)
		{
            //lockTicket.AcquireReaderLock(Timeout.Infinite);                        TODO rsweb
            try
            {
				return FileCache.Contains(imageName);
			}
			finally
			{
                //lockTicket.ReleaseLock();                                       TODO rsweb
            }
        }
	}
}
