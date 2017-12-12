<%@ Page language="c#" Codebehind="MenuItemsPage.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.ReportPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>ReportPage</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<LINK href="EasyLook.css" rel="stylesheet"/>
		<script language="javascript">
		function doLink(url, link) {
		
			if (url != '') {
				parent.HistoryArea.location.href = url;
				parent.SetLink(link);
			}
			
		}

		
		</script>
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
     
    	#form1
		{
			height: 597px;
		}
     
    	.style3
		{
			border-color: #eef0ff;
			width: 38px;
		}
     
    </style>

	</HEAD>
	<body style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px"
		scroll="yes" MS_POSITIONING="GridLayout">
		<form id="reportForm" style="PADDING-RIGHT: 0px; BACKGROUND-POSITION: right bottom; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; WIDTH: 100%; MARGIN-RIGHT: 0px; PADDING-TOP: 0px; BACKGROUND-REPEAT: no-repeat;"
			method="post" runat="server">
			
			<TABLE id="Table1" style="Z-INDEX: 101; LEFT: 0px; POSITION: absolute; TOP: 0px" cellSpacing="0"
				cellPadding="0" width="100%" border="0">
				<tr>
				</tr>
			</TABLE>
			<table  style="background-color:lavender; border: solid 1px gray; width:100%;">
				<tr>
					<td ><asp:checkbox id="DescriptionCheckBox" runat="server" CssClass="Title_Report_Element"
							Width="248px" AutoPostBack="True" Font-Size="10pt"></asp:checkbox></td>
					<td ><asp:checkbox id="DataCheckBox" runat="server" CssClass="Title_Report_Element" Width="248px"
							AutoPostBack="True" Font-Size="10pt"></asp:checkbox></td>
				</tr>
			</table>
			<table border = 0 width="100%" >
				<TR  style= "background-color:RoyalBlue">
					<TD ><asp:label id="TitleLabel" runat="server" Height="15px" BorderColor="Blue" BackColor="RoyalBlue"
							ForeColor="White" Font-Bold="True"  Font-Size="10pt"></asp:label></TD>
				</TR>
				<TR>
					<TD ><asp:table id="reportTable" runat="server" 
							Font-Size="10pt"></asp:table></TD>
				</TR>
			</table>
		</form>
	</body>
</HTML>
