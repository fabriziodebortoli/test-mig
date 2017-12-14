<%@ Register TagPrefix="uc1" TagName="DateSelectionUserControl" Src="DateSelectionUserControl.ascx" %>

<%@ Page Language="c#" CodeBehind="ChangeApplicationDate.aspx.cs" AutoEventWireup="True"
	Inherits="Microarea.Web.EasyLook.ChangeApplicationDate" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head runat="server">
	<title></title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
	<meta content="C#" name="CODE_LANGUAGE">
	<meta content="JavaScript" name="vs_defaultClientScript">
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">

	<script language="javascript">
		function Redirect() {
			if (this.CloseWindow != null) {
				this.CloseWindow();
				this.CloseWindow = null;
			}

		}

		window.onunload = opener.parent.CloseApplicationDate;
	</script>

</head>
<body bottommargin="0" rightmargin="0" leftmargin="0" topmargin="0" onload="Redirect();"
	ms_positioning="GridLayout">
	<form id="Form1" style="background-image: none" method="post" runat="server">
	<asp:ScriptManager runat="server">
	</asp:ScriptManager>
	<asp:UpdatePanel UpdateMode="Conditional" runat="server">
		<ContentTemplate>
			<asp:Panel ID="BackGroundPanel" runat="server" BorderColor="#C0C0FF" BorderStyle="Ridge"
				BackColor="Lavender" Height="100%" Width="100%" BorderWidth="4px" HorizontalAlign="Center">
				<asp:Label ID="TitleLabel" runat="server" Width="352px" Height="32px" Font-Size="10pt"
					Font-Bold="True" ForeColor="RoyalBlue"></asp:Label>
				<table style="padding-right: 5px; z-index: 100; left: 4px; position: relative"  width="100%"
					align="center" border="0">
					<tr>
						<td nowrap width="40%">
							<asp:Label ID="CurrentDateCaptionLabel" runat="server" Font-Size="10pt" ForeColor="Navy"></asp:Label>
						</td>
						<td>
							&nbsp;&nbsp;
						</td>
						<td>
							<asp:Label ID="CurrentDateValueLabel" runat="server" Font-Size="10pt" ForeColor="Navy"></asp:Label>
						</td>
					</tr>
					<tr>
						<td style="background-position: center center; left: 4px; background-image: url(Files/Images/ChangeApplicationDateBackGround.GIF);
							width: 35%; background-repeat: no-repeat; position: relative; height: 204px"
							valign="top">
							<asp:Label ID="NewDateCaptionLabel" runat="server" Font-Size="10pt" ForeColor="Navy"></asp:Label>
						</td>
						<td>
							&nbsp;
						</td>
						<td style="position: relative; top: -4px" valign="top">
							<asp:Panel runat="server" ID="DateSelectionPanel">
								<uc1:DateSelectionUserControl ID="DateSelectionControl" runat="server"></uc1:DateSelectionUserControl>
							</asp:Panel>
						</td>
					</tr>
					<tr>
						<td valign="top" align="right" width="40%" height="100%">
						</td>
						<td>
							&nbsp;
						</td>
						<td valign="top" align="left" height="110%">
							&nbsp;
							<asp:Button ID="OkButton" runat="server" BorderWidth="1px" BackColor="#EEF0FF" BorderStyle="Inset"
								BorderColor="Navy" Font-Size="10pt" ForeColor="Navy" OnClick="OkButton_Click">
							</asp:Button>&nbsp;&nbsp;&nbsp;
							<asp:Button ID="CancelButton" runat="server" BorderWidth="1px" BackColor="#EEF0FF"
								BorderStyle="Inset" BorderColor="Navy" Font-Size="10pt" ForeColor="Navy" OnClick="CancelButton_Click">
							</asp:Button>
						</td>
					</tr>
				</table>
			</asp:Panel>
		</ContentTemplate>
	</asp:UpdatePanel>
	</form>
</body>
</html>
