using System.IO;
using System.Reflection;
using System.Web;
using System.Web.UI;

namespace Microarea.TaskBuilderNet.Woorm.WoormWebControl
{
	//================================================================================
	public class GenericFunctions
	{
        //------------------------------------------------------------------------------
        internal static string GetImageUrl(string imageName, Page userPage)
        {
            // se il file di immagine non è presente,
            // lo creo nella sudirectory "Images" della web application
            // estraendolo dalle embedded resources
            string imageRelativeFolder = "WoormImages";
            string imageFile = imageName + ".jpg";
            string imageFolder = userPage.MapPath(imageRelativeFolder);
            string imagePath = Path.Combine(imageFolder, imageFile); 

            if (!File.Exists(imagePath))
            {
                // creo la cartella e la referenzio nell'oggetto application
                // (così verrà rimossa al termine dell'applicazione)
                if (!Directory.Exists(imageFolder))
                    Directory.CreateDirectory(imageFolder);

                string ns = typeof(GenericFunctions).Namespace + ".Image." + imageFile;

                Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream(ns);
                if (s == null) return string.Empty;

                byte[] bytes = new byte[s.Length];
                int read = s.Read(bytes, 0, (int)s.Length);
                FileStream fs = new FileStream(imagePath, FileMode.Create);
                fs.Write(bytes, 0, read);
                fs.Close();

            }

            // referenzio la cartella nell'oggetto application
            // (così verrà rimossa al termine dell'applicazione)
            WoormFileSystemObject parentObject = userPage.Application[WoormFileSystemObject.GetName(imageFolder)] as WoormFileSystemObject;
            if (parentObject == null)
            {
                parentObject = new WoormFileSystemObject(imageFolder);
                userPage.Application.Add(parentObject.Name, parentObject);
            }

            // referenzio il file creato nell'oggetto application
            // (così verrà rimosso al termine dell'applicazione)
            WoormFileSystemObject fileObject = userPage.Application[WoormFileSystemObject.GetName(imagePath)] as WoormFileSystemObject;
            if (fileObject == null)
            {
                fileObject = new WoormFileSystemObject(imagePath, parentObject);
                userPage.Application.Add(fileObject.Name, fileObject);
            }

            return Path.Combine(imageRelativeFolder, imageFile);
        }	
                
        static public string UrlEncode(string s)
        {
            return HttpUtility.UrlEncode(s);
        }

        static public string UrlDecode(string s)
        {
            return HttpUtility.UrlDecode(s);
        }
        static public string HtmlEncode(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }
        static public string HtmlDecode(string s)
        {
            return HttpUtility.HtmlDecode(s);
        }

    }
}
