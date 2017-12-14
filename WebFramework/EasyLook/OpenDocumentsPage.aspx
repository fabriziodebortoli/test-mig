<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OpenDocumentsPage.aspx.cs" Inherits="Microarea.Web.EasyLook.OpenDocumentsPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style type="text/css">
     .tableDocuments
     {
     	border-collapse: collapse;
     	border: solid 1px gray;
     	width: 100%;
     }
     .cellHeader
     {
     	white-space: nowrap;
     	border: solid 1px gray
     }
     .cellDocuments
     {
     	border: solid 1px gray;
     }
     
    	.style1
		{
			border-color: #eef0ff;
			width: 35px;
		}
		.style2
		{
			border-color: #eef0ff;
			width: 1px;
		}
    	.style3
		{
			border-color: #eef0ff;
			width: 38px;
		}
     
    </style>
    
    <script type="text/javascript" language="javascript">
    	function initPage() {

    		if (this.link != null) {
    			this.link();
    			this.link = null;
    		}
            //verifico che le funzioni siano presenti prima di invocarle
            if (typeof initTableRows == 'function') {
                initTableRows();
            }
            //se la funzione e' stata registrata dinamicamente vuol dire che ci sono delle finestre del browser 
            //associate al documento  che devono essere chiuse
            if (typeof closeWindows == 'function') {
                closeWindows();
               }  
        }
        
        //funzione per determinare se e' supportata la proprieta innerText (non e' cross-Browser, mozilla supporta contentText )
        function innerTextSupported()
        {
            return (document.getElementsByTagName("body")[0].innerText != undefined) ? true : false;
        }
    </script>
</head>
<body onload="initPage()";  style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px;">

    <form id="form1" runat="server" 
	style="background-image: url('Files/Images/QuarkXPress-Passport-48x48.png'); background-repeat: no-repeat; background-position: right bottom; background-attachment: fixed;">
    <asp:Table runat="server" width="100%" style="background-color:lavender; border: solid 1px gray; width:100%; height: 42px;" cellpadding="0" cellspacing="0">
		<asp:TableRow> 
			<asp:TableCell CssClass="style1"><asp:ImageButton  ID="refreshBtn"  
					ImageUrl="~/Files/Images/Refresh-32x32.png" runat="server" Width="38px" /></asp:TableCell>
			<asp:TableCell CssClass="style2">&nbsp;</asp:TableCell>
			<asp:TableCell CssClass="style3"><asp:HyperLink  ID="detailsBtn" ImageUrl="~/Files/Images/Detail-32x32.png" NavigateUrl="javascript:parent.ShowDetails();" runat="server"  /></asp:TableCell>
			<asp:TableCell CssClass="style2">&nbsp;</asp:TableCell>
			<asp:TableCell><asp:ImageButton ID="attachSelectedBtn" ImageUrl="~/Files/Images/AttachToServer-32x32.png"  runat="server" /></asp:TableCell>
			
			<asp:TableCell ID="newDocumentCellIcon" runat="server" VerticalAlign="Middle"></asp:TableCell>
			<asp:TableCell ID="newDocumentCellLink" runat="server" ColumnSpan="2" VerticalAlign="Middle"></asp:TableCell>
			</asp:TableRow>
</asp:Table>

    <asp:Table ID="FormatTable" runat="server" Width="100%" >        
        <asp:TableRow runat="server"> <%--riga per inserire la tabella con riepilogo documenti aperti--%>
            <asp:TableCell ID="documentsCell"  runat="server" ColumnSpan="3"></asp:TableCell>
        </asp:TableRow>
        <asp:TableRow ID="TableRow1" runat="server"> <%--riga per inserire la tabella con riepilogo documenti aperti--%>
            <asp:TableCell ID="TableCell1"  runat="server" ColumnSpan="3" BorderWidth=1 BackColor="#eef0ff" >
				&nbsp;
           		<asp:Image ID="Image1" runat="server" ImageUrl="~/Files/Images/database-24x24.png" /><asp:Label runat="server" id="LegendServer"></asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
				<asp:Image ID="ImageButton1" runat="server" ImageUrl="~/Files/Images/computers-24x24.png"  /><asp:Label runat="server" id="LegendBrowser"></asp:Label>
            </asp:TableCell>
        </asp:TableRow>
    </asp:Table>
     </form>
</body>
</html>
