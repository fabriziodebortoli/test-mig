<%@ Page language="c#" Codebehind="SearchPage.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.SearchPage" %>
<%@ Register TagPrefix="cc1" Namespace="Microarea.TaskBuilderNet.UI.WebControls" Assembly="Microarea.TaskBuilderNet.UI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html xmlns="http://www.w3.org/1999/xhtml" >
	<head  runat="server">
		<title>Easy Look</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<script type="text/javascript">
			
			function Redirect()
			{
				if (this.CloseWindow != null)
				{
					this.CloseWindow();
					this.CloseWindow = null;
				}

			}	
			
			function SelectCommand(url, urlReportPage, title, nameSpace, menuTitle, isReport)
			{
				opener.parent.SelectCommand(url, urlReportPage, title, nameSpace, menuTitle, isReport);
			}

			
			window.onunload = opener.parent.CloseSearchPopUp;

			function SelectAtLeastOne() 
			{
				var checkBoxReport = document.getElementById('<%=SearchReportCheckbox.ClientID %>');
				var checkBoxDocument = document.getElementById('<%=SearchDocumentCheckbox.ClientID %>');
				var errore = document.getElementById('<%=ResultLabel.ClientID %>');

				if (checkBoxReport.checked == 0 && checkBoxDocument.checked == 0)
    			{
					errore.innerHTML = CheckboxNotCheched;
					return false;
				}

				return true;
			}
			
		</script>
	</HEAD>
	<body scroll="no" onload="Redirect();" MS_POSITIONING="GridLayout">
		<form id="Form1" method="post" runat="server">
			<asp:panel id="Panel1" 
				style="Z-INDEX: 102; LEFT: 1px; POSITION: absolute; TOP: 0px" runat="server"
				Height="260px" Width="328px" BorderColor="Lavender" BackColor="Lavender" 
				DefaultButton="SearchButton">
<asp:label id="LookForLabel" runat="server" BackColor="Transparent" Width="296px" Font-Names="Verdana"
					Font-Size="Smaller"> Label</asp:label>&nbsp; 
<asp:textbox id="LookForTextBox" runat="server" Width="304px"></asp:textbox>
<asp:label id="FilterByLabel" runat="server" Width="288px" Height="11px" Font-Names="Verdana"
					Font-Size="Smaller">Label</asp:label>
<asp:dropdownlist id="FilterByDropDownList" runat="server" Width="304px" Font-Names="Verdana" Font-Size="Smaller"></asp:dropdownlist>
&nbsp;&nbsp;&nbsp;
<table border= 0>
<tr>
	<td>&nbsp;	</td>
</tr>
<tr>
	<td>
		<asp:checkbox id="TitlesOnlyCheckBox" runat="server" Width="296px" Font-Names="Verdana" Font-Size="Smaller"></asp:checkbox>&nbsp;
	</td>
</tr>
<tr>
	<td>
		<asp:checkbox id="PrevResaultCheckBox" runat="server" Width="296px" Font-Names="Verdana" Font-Size="Smaller" AutoPostBack="True" oncheckedchanged="PrevResaultCheckBox_CheckedChanged"></asp:checkbox>
	</td>
</tr>
<tr>
	<td>
		<asp:checkbox id="ExactlyCheckBox" runat="server" Width="296px" Font-Names="Verdana" Font-Size="Smaller"></asp:checkbox>
	</td>
</tr>
<tr>
	<td>
		<asp:checkbox id="MatchCaseCheckBox" runat="server" BorderColor="#C0C0FF" Width="296px" Font-Names="Verdana" Font-Size="Smaller" BorderStyle="None"></asp:checkbox>
	</td>
</tr>
<tr>
	<td>
		<asp:CheckBox ID="SearchReportCheckbox" runat="server" BorderColor="#C0C0FF" 
		BorderStyle="None" Checked="true" Font-Names="Verdana" Font-Size="Smaller" 
		height="20px" Width="296px" />
	</td>
</tr>
<tr>
	<td>
		<asp:checkbox id="SearchDocumentCheckbox" runat="server" 
		BorderColor="#C0C0FF" Width="296px" Font-Names="Verdana" Font-Size="Smaller" 
		BorderStyle="None" Checked= "true" height="20px"></asp:checkbox>
	</td>
</tr>
<tr>
	<td>
	
	</td>
</tr>
</table>

</asp:panel><asp:panel id="Panel2" 
				style="Z-INDEX: 103; LEFT: 1px; POSITION: absolute; TOP: 260px" runat="server"
				Height="100%" Width="328px" BorderColor="Lavender" BackColor="Lavender" 
				HorizontalAlign="Center" DefaultButton="SearchButton">
<cc1:PleaseWaitButton id="SearchButton" runat="server" Width="167px" Height="32px" PleaseWaitType="ImageOnly"
					PleaseWaitImage="Files/Images/EyeSmall.gif" onclick="SearchButton_Click" OnClientClick="if (!SelectAtLeastOne()) return false" ></cc1:PleaseWaitButton>
				<br />
				&nbsp;
					
&nbsp;&nbsp;&nbsp; 
								
					<asp:Label id="ResultLabel" runat="server" Font-Names="Verdana" Font-Size="Smaller"></asp:Label>
				</P __designer:mapid="35"></asp:panel></form>
		<div style="LEFT: 344px; OVERFLOW: auto; WIDTH: 745px; POSITION: absolute; TOP: 8px; HEIGHT: 377px">
		<asp:datagrid id="SearchResultDataGrid" runat="server" Width="728px" AllowSorting="True" AutoGenerateColumns="False" AllowPaging="false">
				<SelectedItemStyle Font-Bold="True"></SelectedItemStyle>
				<ItemStyle HorizontalAlign="Left"></ItemStyle>
				<HeaderStyle Font-Bold="True" Wrap="False" HorizontalAlign="Center" ForeColor="White" BackColor="CornflowerBlue"
					Height="20"></HeaderStyle>
				<Columns>
					<asp:TemplateColumn>
						<ItemTemplate>
							<center><%# GetFieldTypeImage((bool)DataBinder.Eval(Container.DataItem, "IsReport"))%></center>
						</ItemTemplate>
					</asp:TemplateColumn>
					<asp:TemplateColumn>
						<HeaderStyle BorderWidth="0px" CssClass="ReportHeader"></HeaderStyle>
						<ItemStyle Width="50%"></ItemStyle>
						<ItemTemplate>
							<asp:Table Runat="server" BorderWidth="0" ID="ReportsTable" NAME="ReportsTable">
								<asp:TableRow>
									<asp:TableCell Font-Size="Smaller" Font-Name="<%# GetFontNames()%>">
										<%# GetUrlField((string)DataBinder.Eval(Container.DataItem, "ReportName"), (string)DataBinder.Eval(Container.DataItem, "Link"), (string)DataBinder.Eval(Container.DataItem, "MenuPathWithName"), (string)DataBinder.Eval(Container.DataItem, "MenuTitle"), (string)DataBinder.Eval(Container.DataItem, "ReportNameSpace"), (bool)DataBinder.Eval(Container.DataItem, "IsReport"))%>
									</asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell Font-Size="Smaller" Font-Name="<%# GetFontNames()%>">
										<%# DataBinder.Eval(Container.DataItem, "Description")%>
									</asp:TableCell>
								</asp:TableRow>
							</asp:Table>
						</ItemTemplate>
					</asp:TemplateColumn>
					<asp:TemplateColumn>
						<ItemTemplate>
							<asp:Label Font-Size="Small" Font-Names="Verdana">
								<%# DataBinder.Eval(Container.DataItem, "MenuPath") %>
							</asp:Label>
						</ItemTemplate>
					</asp:TemplateColumn>
				</Columns>
			</asp:datagrid></div>
	</body>
</HTML>
