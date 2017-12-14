<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ErrorPage.aspx.cs" Inherits="Microarea.Web.EasyLook.ErrorPage" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title></title>
	<style type="text/css">
		p
		{
			margin-left: 20px;
			margin-right: 20px;
		}
	</style>
	<link href="EasyLook.css" rel="stylesheet" />
	
	<script language="javascript" type="text/javascript">

		function OnToggleDetails(stringHideDetails, stringShowDetails) {
			var button = document.getElementById('ButtonDetails');
			var detailsPanel = document.getElementById('DetailsPanel');

			if (button && detailsPanel) {
				if (detailsPanel.style.visibility == "hidden") {
					detailsPanel.style.visibility = "visible";
					button.value = stringHideDetails;
				}
				else {
					detailsPanel.style.visibility = "hidden";
					button.value = stringShowDetails;
				}
			}
		}
	</script>
	
	
</head>
<body style="background-color: #EEF0FF">
	<form id="form1" runat="server">
			<div style="position: absolute; width: 600px; min-height: 300px; border-style: inset;
				border-width: medium; margin-left: auto; margin-right: auto; position: relative;
				top: 100px; overflow:auto">
				<asp:Image ID="Image1" runat="server" Height="180px" Width="180px" ImageUrl="~/Files/Images/Error.gif"
					Style="float: left; margin: 20px;" />
				<p>
					<asp:Label ID="ErrorLabel" runat="server" Text=""></asp:Label></p>
				<p>
					<asp:Label ID="ErrorLabelDescription" runat="server" Text="..."></asp:Label></p>
				<asp:Button ID="ButtonDetails" runat="server" Text=""/>
				<asp:Panel runat="server" ID="DetailsPanel" Visible="true" Style="clear: left">
					<p>
						<asp:Label ID="DetailLabel" runat="server" Text=""></asp:Label></p>
					<p>
						<asp:Label ID="DetailLabelDescription" runat="server" Text="..."></asp:Label></p>
					<p>
						<asp:Label ID="StackTrace" runat="server" Text=""></asp:Label></p>
					<p>
						<asp:Label ID="StackTraceDescription" runat="server" Text="..."></asp:Label></p>
				</asp:Panel>
				<p style="text-align: center;clear: left;">
					<asp:Button runat="server" ID="CloseButton" Text="" OnClientClick="window.close();" /></p>
			</div>
	</form>
</body>
</html>
