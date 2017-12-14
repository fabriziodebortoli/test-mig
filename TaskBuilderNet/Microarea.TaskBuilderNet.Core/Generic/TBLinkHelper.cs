using System;
using System.Windows.Forms;

namespace Microarea.TaskBuilderNet.Core.Generic
{
   public class TBLinkHelper    
    {

        // Sample class for Copying and Pasting HTML fragments to and from the clipboard.
        // Mike Stall. http://blogs.msdn.com/jmstall
        /// <summary>
        /// Helper class to decode HTML from the clipboard.
        /// See http://blogs.msdn.com/jmstall/archive/2007/01/21/html-clipboard.aspx for details.
        /// </summary>

        //------------------------------------------------------------------------------------------------------------------------------
        public static bool CopyTBLinkToClipboard(string tblink)
        {
            if (string.IsNullOrWhiteSpace(tblink)) return false;

            return CopyToClipboard(String.Format("<a href =\"{0}\">{0}</a>", tblink), tblink, null);

        }

        /// <summary>
        /// Clears clipboard and copy a HTML fragment to the clipboard, providing additional meta-information.
        /// </summary>
        /// <param name="htmlFragment">a html fragment</param>
        /// <param name="title">optional title of the HTML document (can be null)</param>
        /// <param name="sourceUrl">optional Source URL of the HTML document, for resolving relative links (can be null)</param>
        //------------------------------------------------------------------------------------------------------------------------------
        private static bool CopyToClipboard(string htmlFragment, string txtFragment, string title = null, Uri sourceUrl = null)
        {
            if (string.IsNullOrWhiteSpace(htmlFragment) && string.IsNullOrWhiteSpace(txtFragment)) return false;

            try
            {
                if (title == null) title = "TBLinkHelper";

                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                // Builds the CF_HTML header. See format specification here:
                // http://msdn.microsoft.com/library/default.asp?url=/workshop/networking/clipboard/htmlclipboard.asp

                // The string contains index references to other spots in the string, so we need placeholders so we can compute the offsets. 
                // The <<<<<<<_ strings are just placeholders. We'll backpatch them actual values afterwards.
                // The string layout (<<<) also ensures that it can't appear in the body of the html because the <
                // character must be escaped.
                string header =
        @"Format:HTML Format
Version:1.0
StartHTML:<<<<<<<1
EndHTML:<<<<<<<2
StartFragment:<<<<<<<3
EndFragment:<<<<<<<4
StartSelection:<<<<<<<3
EndSelection:<<<<<<<3
";

                string pre =
        @"<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML><HEAD><TITLE>" + title + @"</TITLE></HEAD><BODY><!--StartFragment-->";

                string post = @"<!--EndFragment--></BODY></HTML>";

                sb.Append(header);
                if (sourceUrl != null)
                {
                    sb.AppendFormat("SourceURL:{0}", sourceUrl);
                }
                int startHTML = sb.Length;

                sb.Append(pre);
                int fragmentStart = sb.Length;

                sb.Append(htmlFragment);
                int fragmentEnd = sb.Length;

                sb.Append(post);
                int endHTML = sb.Length;

                // Backpatch offsets
                sb.Replace("<<<<<<<1", To8DigitString(startHTML));
                sb.Replace("<<<<<<<2", To8DigitString(endHTML));
                sb.Replace("<<<<<<<3", To8DigitString(fragmentStart));
                sb.Replace("<<<<<<<4", To8DigitString(fragmentEnd));


                // Finally copy to clipboard.
                string data = sb.ToString();
                Clipboard.Clear();

                DataObject mydataobject = new DataObject();
                if (!string.IsNullOrWhiteSpace(data))
                    mydataobject.SetText(data, TextDataFormat.Html);
                if (!string.IsNullOrWhiteSpace(txtFragment))
                    mydataobject.SetText(txtFragment, TextDataFormat.Text);
                Clipboard.SetDataObject(mydataobject);

                return true;
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.ToString());
                return false;
            }

        }

        // Helper to convert an integer into an 8 digit string.
        // String must be 8 characters, because it will be used to replace an 8 character string within a larger string.    
        //------------------------------------------------------------------------------------------------------------------------------
        private static string To8DigitString(int x)
        {
            return String.Format("{0,8}", x);
        }


    } // end of class
}

