using System;


namespace Microarea.RSWeb.Applications
{
    //================================================================================
    public class ManifestImageLoader
	{
		//--------------------------------------------------------------------------------
/* TODO RSWEB
		public static void RenderImage ( string assembly, string image, HttpContext context )
		{
			try
			{
				Assembly   resourceAssem = Assembly.Load ( assembly ) ;

				// Get the resource
				Stream imageStream = resourceAssem.GetManifestResourceStream(image);
				// And if that's OK, write it out
			
				System.Drawing.Image theImage = System.Drawing.Image.FromStream ( imageStream );
			
				string type = Path.GetExtension(image).ToLower(CultureInfo.InvariantCulture);

				string contentType;
				ImageFormat imageFormat;
				GetFormat(type, out contentType, out imageFormat);
				context.Response.ContentType = contentType ;
				theImage.Save ( context.Response.OutputStream , imageFormat) ;
			}
			catch(Exception ex)
			{
				context.Response.Write(ex.Message);
			}
		}
		*/
		//--------------------------------------------------------------------------------
		private static void GetFormat(string extension, out string contentType /*,out ImageFormat imageFormat TODO rsweb*/)
		{
			switch (extension)
			{
                //case ".jpg": contentType = "image/jpg"; imageFormat = ImageFormat.Jpeg; break;            TODO resweb 
                //case ".bmp": contentType = "image/bmp"; imageFormat = ImageFormat.Bmp; break;
                //case ".gif": contentType = "image/jpg"; imageFormat = ImageFormat.Gif; break;
                default: throw new NotSupportedException("Image format not supported");
			}
		}
	}

}
