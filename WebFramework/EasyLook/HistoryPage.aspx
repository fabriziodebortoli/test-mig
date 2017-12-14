<%@ Page language="c#" Codebehind="HistoryPage.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.HistoryPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>HistoryPage</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<LINK href="EasyLook.css" rel="stylesheet">
		<script type="text/javascript">
			function Redirect()
			{
				if (this.Delete != null)
				{
					this.Delete();
					this.Delete = null;
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
			height: 662px;
		}
     
    	.style3
		{
			border-color: #eef0ff;
			width: 38px;
		}
     
    </style>
	</HEAD>
	<body onload="Redirect();" MS_POSITIONING="GridLayout" style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px"
		scroll="yes">
		<form id="Form1" method="post" runat="server" 
		style="background-position: right bottom; PADDING-RIGHT: 0px; BACKGROUND-IMAGE: url(Files/Images/ReportBackGround.GIF); PADDING-BOTTOM: 0px; WIDTH: 100%; MARGIN-RIGHT: 0px; PADDING-TOP: 0px; BACKGROUND-REPEAT: no-repeat;  PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px; background-attachment: fixed;">
		 <asp:Table ID="ToolBarTable" Visible= "false" runat="server" width="100%" style="background-color:lavender; border: solid 1px gray; width:100%; height: 42px;" cellpadding="0" cellspacing="0">
		<asp:TableRow> 
			<asp:TableCell CssClass="style1"><asp:ImageButton  ID="refreshBtn"  ImageUrl="~/Files/Images/Refresh-32x32.png" runat="server" Width="38px" /></asp:TableCell>
			<asp:TableCell CssClass="style2">&nbsp;</asp:TableCell>
			<asp:TableCell CssClass="style3"><asp:ImageButton  ID="deleteBtn" ImageUrl="~/Files/Images/delete-32x32.png"  runat="server"  onclick="deleteReportButton_Click" /></asp:TableCell>
			<asp:TableCell CssClass="style2">&nbsp;</asp:TableCell>
			<asp:TableCell id="runReportCellIcon"><asp:HyperLink ID="runReportBtn" ImageUrl="~/Files/Images/add-file-32x32.png"  runat="server" /></asp:TableCell>
			<asp:TableCell id="runReportCellLink"></asp:TableCell>	
			</asp:TableRow>
			</asp:Table><TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%">
				<TR valign="top">
					<TD style="HEIGHT: 141px" width="100%">
						<asp:datagrid id="historyDataGrid" runat="server" Width="100%" AutoGenerateColumns="False"
							AllowSorting="True">
							<SelectedItemStyle Font-Bold="True"></SelectedItemStyle>
							<ItemStyle HorizontalAlign="Left"></ItemStyle>
							<HeaderStyle Font-Bold="True" Wrap="False" HorizontalAlign="Center" ForeColor="White" BackColor="Blue"></HeaderStyle>
							<Columns>
								<asp:HyperLinkColumn DataNavigateUrlField="Link" DataTextField="NomeReport">
									<HeaderStyle Wrap="False" HorizontalAlign="Center"></HeaderStyle>
									<ItemStyle HorizontalAlign="Left"></ItemStyle>
								</asp:HyperLinkColumn>
								<asp:BoundColumn DataField="User">
									<ItemStyle HorizontalAlign="Left"></ItemStyle>
								</asp:BoundColumn>
								<asp:BoundColumn DataField="Data" SortExpression="Data">
									<HeaderStyle Wrap="False"></HeaderStyle>
									<ItemStyle Wrap="False" HorizontalAlign="Left"></ItemStyle>
								</asp:BoundColumn>
								<asp:TemplateColumn>
									<ItemTemplate>
										<asp:CheckBox ID="DeleteReport" Runat="server" OnCheckedChanged="DataGrid_ChekChanged" AutoPostBack="true"></asp:CheckBox>
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:BoundColumn Visible="False" DataField="NomeFile" HeaderText="NomeFile">
									<ItemStyle HorizontalAlign="Left"></ItemStyle>
								</asp:BoundColumn>
							</Columns>
						</asp:datagrid></TD>
				</TR>
</TABLE></form></body></HTML>