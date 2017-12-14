using System;
using System.ComponentModel;
using System.Windows.Forms;
using Microarea.TaskBuilderNet.Core.Generic;
using Microarea.TaskBuilderNet.Interfaces.View;

namespace Microarea.TaskBuilderNet.Forms.Controls
{
    public class UIPictureBox : PictureBox, IUIControl
	{
        TBWFCUIControl cui;

        [Browsable(false)]
        virtual public ITBCUI CUI { get { return cui; } }
        private string imageResourceName;
        private string imageResourceFileName;
        private string imageFilePath;
        private NameSpace imageNamespace;
	
        //Proprietà per caricare immagine
        [Browsable(false)]
        public string ImageResourceName { get { return imageResourceName; } set { imageResourceName = value; SetImage(); } }
        [Browsable(false)]
        public string ImageResourceFileName { get { return imageResourceFileName; } set { imageResourceFileName = value; SetImage(); } }
        [Browsable(false)]
        public string ImageFilePath { get { return imageFilePath; } set { imageFilePath = value; SetImage(); } }
        [Browsable(false)]
        public NameSpace ImageNamespace { get { return imageNamespace; } set { imageNamespace = value; SetImage(); } }
	
        //-------------------------------------------------------------------------
        public UIPictureBox()
		{
	        cui = new TBWFCUIControl(this, Interfaces.NameSpaceObjectType.Control);
  		}
         
        //Caricamento immagine utilizzando namespace, file, o risorsa dell'assembly corrente
        //-------------------------------------------------------------------------
        private void SetImage()
        {
            // TODOILARIA ... se sostituisco un immagine con un'altra il rilascio della memoria
            // della prima non so chi lo fa. Sarebbe da verificare.
            if (!String.IsNullOrWhiteSpace(ImageNamespace))
                Image = ImageLoader.GetImageFromNamespace(ImageNamespace);
            else if (!String.IsNullOrWhiteSpace(ImageResourceName))
                Image = ImageLoader.GetImageFromResourceStream(ImageResourceName);
            else if (!String.IsNullOrWhiteSpace(ImageFilePath))
                Image = ImageLoader.GetImageFromPath(ImageFilePath);
            else if (!String.IsNullOrWhiteSpace(ImageResourceFileName))
                Image = ImageLoader.GetImageFromResourceManager(ImageResourceFileName);

        }

        //-------------------------------------------------------------------------
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (cui != null)
            {
                cui.Dispose();
                cui = null;
            }
        }
	}
}
