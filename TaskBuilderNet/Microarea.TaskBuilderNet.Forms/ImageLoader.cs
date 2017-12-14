using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using Microarea.TaskBuilderNet.Core.NameSolver;
using Microarea.TaskBuilderNet.Interfaces;

namespace Microarea.TaskBuilderNet.Forms
{
    //=========================================================================
    public static class ImageLoader
    {
        
        //-------------------------------------------------------------------------
        public static Image GetImageFromPath(string imagePath)//path assoluto 
        {
            if (String.IsNullOrWhiteSpace(imagePath) || !File.Exists(imagePath))
                return null;
            return Image.FromFile(imagePath);
        }

        //-------------------------------------------------------------------------
        public static Image GetImageFromNamespace(INameSpace nameSpace)//namespace
        {
            PathFinder pf = new PathFinder(CUtility.GetCompany(), CUtility.GetUser());
            string path = pf.GetFilename(nameSpace, "");

            return GetImageFromPath(path);

        }

        //-------------------------------------------------------------------------
        public static Image GetImageFromResourceManager(string imageName)//nome della risorsa inserita nel file resources.resx dell'assembly corrente
        {
            if (String.IsNullOrWhiteSpace(imageName))
                return null;
            object obj = Microarea.TaskBuilderNet.Forms.Properties.Resources.ResourceManager.GetObject(imageName);
            return obj as Image;
        }

        //-------------------------------------------------------------------------
        public static Image GetImageFromResourceStream(string imageName)//nome del file completo di estensione, deve essere embedded resources nell'assembly corrente
        {
            if (String.IsNullOrWhiteSpace(imageName))
                return null;
            Assembly currentAssembly = typeof(ImageLoader).Assembly;
            string ns = "Microarea.TaskBuilderNet.Forms.Images.{0}";
            Stream imageStream = currentAssembly.GetManifestResourceStream(String.Format(CultureInfo.InvariantCulture, ns, imageName));
            return Image.FromStream(imageStream);          
        }



        //-------------------------------------------------------------------------
        public static Bitmap GetBitmapFromResourceManager(string imageName)
        {
            if (String.IsNullOrWhiteSpace(imageName))
                return null;
            Assembly currentAssembly = typeof(ImageLoader).Assembly;
            string ns = "Microarea.TaskBuilderNet.Forms.Images.{0}";
            Bitmap bmp = new Bitmap(currentAssembly.GetManifestResourceStream(String.Format(CultureInfo.InvariantCulture, ns, imageName)));
            bmp.MakeTransparent(bmp.GetPixel(1, 1));
            return bmp;
        }

    }
}
