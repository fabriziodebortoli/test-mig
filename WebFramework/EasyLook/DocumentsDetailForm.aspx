<%@ Page Language="c#" CodeBehind="DocumentsDetailForm.aspx.cs" AutoEventWireup="True"
	Inherits="Microarea.Web.EasyLook.DocumentsDetailForm" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
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
     
    	#DetailsDoumentsForm
		{
			height: 800px;
		}
     
    </style>


	<script type="text/javascript" language="javascript">
		function initPage() {
			
			//se la funzione e' stata registrata dinamicamente vuol dire che ci sono delle finestre del browser 
			//associate al documento  che devono essere chiuse
			if (typeof closeWindows == 'function') {
				closeWindows();
			}
			if (typeof Ping == 'function') {
					Ping();
				}

			}
			

	</script>
    <style type="text/css">
     .tableToolBar
     {
		border: solid 1px gray; 
		width:100%; 
		height: 42px;
	}
	</style> 
</head>
<body onload="initPage();"  style="margin: 0px;" bgcolor="#ffffff" >
	<form runat="server" id="DetailsDoumentsForm"    
	style="background-image: url('Files/Images/wizard-128x128.png'); background-position: right bottom; background-repeat: no-repeat">

	<asp:Table  runat="server"  cellpadding="0" cellspacing="0" ID="ToolBarTable"  style="background-color:lavender; border: solid 1px gray; width:100%; height: 42px;">
		<asp:TableRow >
			<asp:TableCell>
				&nbsp;
				<asp:ImageButton ID="CloseImageButton" runat="server" ImageUrl="~/Files/Images/Close-32x32.png" />
			</asp:TableCell>
			<asp:TableCell>
				&nbsp;
			</asp:TableCell>
			<asp:TableCell>
				<asp:ImageButton ID="RefreshImageButton" runat="server" ImageUrl="~/Files/Images/Refresh-32x32.png" />
			</asp:TableCell>
			<asp:TableCell>
				<asp:Label  ID="RefreshTimeLabel" runat="server" Text="Label"></asp:Label>&nbsp;
				<asp:DropDownList ID="RefreshTimeDropDownList" runat="server"  AutoPostBack= "true">
				</asp:DropDownList>
			</asp:TableCell>
			<asp:TableCell>
				&nbsp;&nbsp;&nbsp;&nbsp
			</asp:TableCell>
			<asp:TableCell HorizontalAlign="Right">
				<asp:Label ID="CompanyLabel" runat="server" Text="Label"></asp:Label>&nbsp;
				<asp:DropDownList ID="CompanyDropDownList" runat="server" AutoPostBack= "true">
				</asp:DropDownList>&nbsp;
			</asp:TableCell>
			
			<asp:TableCell HorizontalAlign="Right">
				<asp:Label ID="allUserLabel" runat="server" Text="Label"></asp:Label>&nbsp;
				<asp:DropDownList ID="AllUserDropDownList" runat="server" AutoPostBack= "true">
				</asp:DropDownList>&nbsp;
			</asp:TableCell>

			<asp:TableCell HorizontalAlign="Right">
				<asp:Label ID="TypeLabel" runat="server" Text="Label"></asp:Label>&nbsp;
				<asp:DropDownList ID="TypeFilterDropDownList" runat="server" AutoPostBack= "true">
				</asp:DropDownList>&nbsp;
				&nbsp;
			</asp:TableCell>
		</asp:TableRow>


</asp:Table>
<asp:Table runat="server" Width="100%"  ID="LeggendTable" >
		<asp:TableRow >
			<asp:TableCell ID = "tableCellTable" ColumnSpan= "8" />
		</asp:TableRow>
		<asp:TableRow >
		  <asp:TableCell ID="TableCell1"  ColumnSpan= "8" runat="server" BorderWidth=1 BackColor="#eef0ff" >
				&nbsp;
           		<asp:Image ID="Image1" runat="server" ImageUrl="~/Files/Images/database-24x24.png" /><asp:Label runat="server" id="LegendServer"></asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
				<asp:Image ID="ImageButton1" runat="server" ImageUrl="~/Files/Images/computers-24x24.png"  /><asp:Label runat="server" id="LegendBrowser"></asp:Label>
				&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
           		<asp:Image ID="Image2" runat="server" ImageUrl="~/Files/Images/redBall.png" /><asp:Label runat="server" id="LabelRed"></asp:Label>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
				<asp:Image ID="Image3" runat="server" ImageUrl="~/Files/Images/greenBall.png"  /><asp:Label runat="server" id="LabelGreen"></asp:Label>

            </asp:TableCell>
            </asp:TableRow >
	</asp:Table>
	<br />


</form>
</body>
</html>
