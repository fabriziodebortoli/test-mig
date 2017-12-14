using System;
using System.Text;
using System.Web;

namespace Microarea.Tools.TBLocalizer
{
    /// <summary>
    /// Scrive html.
    /// </summary>
    //=========================================================================
    public class HtmlWriter
    {
        public static string Blank = "&nbsp;";
        string htmlOpen = "<html>\r\n";
        string htmlClose = "</html>\r\n";
        string tableOpen = "<table id=\"{0}\" width=\"100%\"{1}{2}{3}>\r\n";
        string tableClose = "</table>\r\n";
        string headOpen = "<head>\r\n";
        string headClose = "</head>\r\n";
        string titleOpen = "<title>";
        string titleClose = "</title>\r\n";
        string bodyOpen = "<body>";
        string bodyClose = "</body>\r\n";
        string columnOpen = "<td id=\"{0}\" {1} class=\"{2}\">";
        string columnOpenWithAttr = "<td id=\"{0}\" {1} class=\"{2}\" {3}=\"{4}\">";
        string columnClose = "</td>";
        string rowOpen = "<tr id=\"{0}\" {1}>";
        string rowClose = "</tr>\r\n";
        string line = "<hr width=\"100%\" />\r\n";
        string emptyLine = "<p />\r\n";
        string border = " border=\"{0}\"";
        string color = " color=\"{0}\"";
        string indent = " indent=\"5\"";
        string height = " height=\"{0}\"";
        string width = " width=\"{0}\"";
        string paragraphWAttribute = "<p{0}>{1}</p>\r\n";
        string paragraph = "<p>{0}</p>\r\n";
        string newLine = "<br>\r\n";
        string fontOpen = "<font{0}>";
        string fontClose = "</font>";
        string cellSpacing = " cellSpacing=\"{0}\"";
        string cellPadding = " cellPadding=\"{0}\"";
        string css = @"<style type=""text/css"">
								body {font-size:12px; font-family:verdana; color:black; text-align:justify;}
								p {font-size:12px; font-family:verdana; color:black; text-align:justify;}
								td {font-size:10px; font-family:verdana; color:black; text-align:justify;}
                                .refId {color:Gray; text-align:center;}
								.content {color:Black; text-align:left;}
							</style>";
        string divOpen = "<div id=\"{0}\">\r\n";
        string divClose = "</div>\r\n";
        string titleRow = "<tr><td class=\"refId\" colspan=\"3\">{0}</td></tr>\r\n";


        private StringBuilder source = new StringBuilder(String.Empty);
        public string Source { get { return source.ToString(); } }

        /// <summary>
        /// Inizia un xmlDocument col suo title
        /// </summary>
        /// <param name="title">titolo da dare alla pagina html</param>
        //---------------------------------------------------------------------
        public void Open(string title)
        {
            source.Append(htmlOpen);
            source.Append(headOpen);
            source.Append(css);
            source.Append(titleOpen);
            source.Append(title);
            source.Append(titleClose);
            source.Append(headClose);
            source.Append(bodyOpen);
        }

        //---------------------------------------------------------------------
        public void Close()
        {
            source.Append(bodyClose);
            source.Append(htmlClose);

        }

        public string Encode(string text)
        {
            text = text.Replace("\r\n", "{CR-LF}");
            text = text.Replace("\n\r", "{LF-CR}");
            text = text.Replace("\n", "{LF}");
            text = text.Replace("\r", "{CR}");
            return HttpUtility.HtmlEncode(text);
        }

        static public string Decode(string text)
        {
            text = text.Replace("{CR-LF}", "\r\n");
            text = text.Replace("{LF-CR}", "\n\r" );
            text = text.Replace("{LF}", "\n" );
            text = text.Replace("{CR}", "\r");
            return HttpUtility.HtmlDecode(text);
        }

        //---------------------------------------------------------------------
        public void Write(string text)
        {
            source.Append(text);
        }
        //---------------------------------------------------------------------
        public void WriteLine()
        {
            source.Append(line);
        }

        //---------------------------------------------------------------------
        public void WriteEmptyLine()
        {
            source.Append(emptyLine);
        }

        //---------------------------------------------------------------------
        public void OpenTable(string id, float aBorder, int cellspacing, int cellpadding)
        {
            string borderInfo = String.Empty;
            if (aBorder > 0)
            {
                borderInfo = String.Format(border, aBorder);
            }
            string spacingInfo = String.Format(cellSpacing, cellspacing);
            string paddingInfo = String.Format(cellPadding, cellpadding);
            string tableInfo = String.Format(tableOpen, id, borderInfo, spacingInfo, paddingInfo);
            source.Append(tableInfo);


        }

        //---------------------------------------------------------------------
        public void WriteTitleRow(string id)
        {
            source.Append(String.Format(titleRow, id));
        }

        //---------------------------------------------------------------------
        public void OpenRow(string id, int aHeight, bool percentage)
        {
            if (aHeight > 0)
            {
                StringBuilder aValue = new StringBuilder(aHeight.ToString());
                if (percentage) aValue.Append("%");
                string heightInfo = String.Format(height, aValue);
                string rowInfo = String.Format(rowOpen, id, heightInfo);
                source.Append(rowInfo);
            }
            else
                source.Append(String.Format(rowOpen, id, String.Empty));
        }


        //---------------------------------------------------------------------
        public void OpenColumn(string id, int aWidth, bool percentage, string cssClass)
        {
            if (aWidth > 0)
            {
                StringBuilder aValue = new StringBuilder(aWidth.ToString());
                if (percentage) aValue.Append("%");
                string widthInfo = String.Format(width, aValue);
                string columnInfo = String.Format(columnOpen, id, widthInfo, cssClass);
                source.Append(columnInfo);
            }
            else
                source.Append(String.Format(columnOpen, String.Empty));

        }

        //---------------------------------------------------------------------
        public void OpenColumnWithAttribute(string id, int aWidth, bool percentage, string cssClass, string attrTag, string attrValue)
        {
            if (aWidth <= 0)
            {
                source.Append(String.Format(columnOpen, String.Empty));
                return;
            }

            StringBuilder aValue = new StringBuilder(aWidth.ToString());
            if (percentage) aValue.Append("%");
            string widthInfo = String.Format(width, aValue);
            string columnInfo = String.Format(columnOpenWithAttr, id, widthInfo, cssClass, attrTag, attrValue);
            source.Append(columnInfo);
        }

        //---------------------------------------------------------------------
        public void CloseTable()
        {
            source.Append(tableClose);
        }

        //---------------------------------------------------------------------
        public void CloseRow()
        {
            source.Append(rowClose);

        }

        //---------------------------------------------------------------------
        public void CloseColumn()
        {
            source.Append(columnClose);
        }

        //---------------------------------------------------------------------
        public void WriteRedParagraph(string text)
        {
            StringBuilder font = new StringBuilder(String.Format(fontOpen, String.Format(color, "red")));
            font.Append(text);
            font.Append(fontClose);
            source.Append(String.Format(paragraph, font.ToString()));
        }

        //---------------------------------------------------------------------
        public void WriteParagraph(string text)
        {
            source.Append(String.Format(paragraph, text));
        }

        //---------------------------------------------------------------------
        public void WriteTabbedRedParagraph(string text)
        {
            StringBuilder font = new StringBuilder(String.Format(fontOpen, String.Format(color, "red")));
            font.Append(text);
            font.Append(fontClose);
            source.Append(String.Format(paragraphWAttribute, indent, font.ToString()));
        }

        //---------------------------------------------------------------------
        public void Newline()
        {
            source.Append(newLine);
        }

        //---------------------------------------------------------------------
        public void OpenDiv(string id)
        {
            source.Append(String.Format(divOpen, id));
        }

        //---------------------------------------------------------------------
        public void CloseDiv()
        {
            source.Append(divClose);
        }

    }
}
