using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.UI.WinControls
{
	/// <summary>
	/// Mostra una Splash, in attesa che venga caricata l'interfaccia grafica del chiamante, 
	/// visualizza inoltre una progressBar che mostra il passare del tempo.
	/// </summary>
	//=========================================================================
	internal partial class Splash : System.Windows.Forms.Form
	{
		private Image bitmap;
		//private const string imageNameSpace = "Microarea.TaskBuilderNet.UI.WinControls.Splashes.Images.";
		//private const string imageMagoNet = "SplashMenuManager.jpg";
		private static int waitValue = 100;

		/// <param name="resourceName">Nome dell'immagine embeddata in questa DLL</param>
		/// <param name="alternativeImagePath">PErcorso da cui caricare un'immagine se la 
		/// <code>resourceName</code> non è presente in questa DLL</param>
		//---------------------------------------------------------------------
		internal Splash(Image img)
		{
			InitializeComponent();

            float dpiScale = 1;
            Graphics g = this.CreateGraphics();
            try
            {
                dpiScale = g.DpiY / 96;
            }
            finally
            {
                g.Dispose();
            }

            if (dpiScale != 1)
            {
                int w = (int)(img.Width * dpiScale);
                int h = (int)(img.Height * dpiScale);

                var newImage = new Bitmap(w, h);

                using (var graphics = Graphics.FromImage(newImage))
                    graphics.DrawImage(img, 0, 0, w, h);

                bitmap = newImage;
            }
            else
            {
                bitmap = img;
            }
			PostInitializeComponent();
		}

		/// <summary>
		/// Settaggio delle posizioni dei controlli
		/// </summary>
		//---------------------------------------------------------------------
		private void PostInitializeComponent()
		{
			if (bitmap == null) return;

			this.SuspendLayout();

			SplashBox.Image = bitmap;
			SplashBox.Size = bitmap.Size;
			this.Size = SplashBox.Size;
            this.CenterToScreen();
            this.ResumeLayout();
		}

		/// <summary>
		/// Show della finestra solo se la bitmap non è null
		/// </summary>
		//---------------------------------------------------------------------
		internal new void Show()
		{
			if (bitmap == null)
				return;
			base.Show();
           // base.BringToFront();
			Application.DoEvents(); //necessario per poter visualizzare la splash
		}

		/// <summary>
		/// Incrementa la progress.
		/// </summary>
		//---------------------------------------------------------------------
		internal void ActivateProgress()
		{
           // base.BringToFront();
            while (!SplashStarter.stopThread)
			{
                
                if (Progressbar.Value == Progressbar.Maximum)
					Progressbar.Value = Progressbar.Minimum;

				Progressbar.PerformStep();
				Thread.Sleep(waitValue);
			}

			Progressbar.Value = Progressbar.Maximum;
			Thread.Sleep(waitValue);
			Close();
		}
	}

	//=========================================================================
	public class SplashStarter
	{
		internal static bool stopThread = false;
		private static Thread splashThread = null;
		private static Splash dummySplash = null;
		private static Image bitmapSplash = null;

		//---------------------------------------------------------------------
		public static void Start(Image bitmap)
		{
			bitmapSplash = bitmap;
			Start();
		}

		//---------------------------------------------------------------------
		private static void Start()
		{
			stopThread = false;
			//tapullo: siccome se la prima finestra che viene visualizzata e' su un altro thread
			//le successive 'perdono il fuoco', col risultato che vedo la splash, poi la form successiva va a finire
			//sotto la finestra di un'altra applicazione, metto una form 'finta' sul thread principale, che chiudo DOPO
			//quella del thread secondario

			dummySplash = new Splash(bitmapSplash);
			dummySplash.Show();

			splashThread = new Thread(new ThreadStart(ActivateProgress));
			splashThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
			splashThread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			splashThread.Start();
		}

		//---------------------------------------------------------------------
		private static void ActivateProgress()
		{
            Splash splash = new Splash(bitmapSplash);
			splash.Show();
            splash.Activate();
			splash.ActivateProgress();
		}

		/// <summary>
		/// Chiude la splash
		/// </summary>
		//---------------------------------------------------------------------
		public static void Finish()
		{
			try
			{
				if (stopThread)
					return;
				
				if (dummySplash != null)
				{
					dummySplash.Progressbar.Value = dummySplash.Progressbar.Maximum;
					dummySplash.Update();
				}

				stopThread = true;
				//aspetto che il thread della splash termini
				if (splashThread != null)
					splashThread.Join();

				if (dummySplash != null)
				{
					dummySplash.Close();
					dummySplash = null;
				}
			}
			catch { } //in chiusura: non ha senso gestire l'errore
		}
	}
}