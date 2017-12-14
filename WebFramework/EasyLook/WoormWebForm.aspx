<%@ Register TagPrefix="woorm" Namespace="Microarea.TaskBuilderNet.Woorm.WoormWebControl" Assembly="Microarea.TaskBuilderNet.Woorm" %>

<%@ Page Language="c#" CodeBehind="WoormWebForm.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.WoormWebForm" %>

<!DOCTYPE html>
<html>
<head runat="server">
	<title></title>
	<meta http-equiv="X-UA-Compatible" content="IE=9" />
</head>
<body style="margin: 0px; overflow:auto" bgcolor="#ffffff">
	<form method="post" runat="server" id="Form1">
		<asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true">
		</asp:ScriptManager>
		<woorm:WoormWebControl runat="server" ID="woorm"  OwnerApplication="WebFramework" OwnerModule="EasyLook"/>
	</form>
</body>
</html>
