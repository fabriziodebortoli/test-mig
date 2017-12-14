<%@ Page language="c#" Codebehind="MenuArea.aspx.cs" AutoEventWireup="false" Inherits="Microarea.Web.EasyLook.MenuArea" %>
<%@ Register TagPrefix="cc1" Namespace="Microarea.TaskBuilderNet.UI.WebControls" Assembly="Microarea.TaskBuilderNet.UI" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>MenuArea</title>
		<meta name="GENERATOR" Content="Microsoft Visual Studio .NET 7.1">
		<meta name="CODE_LANGUAGE" Content="C#">
		<meta name="vs_defaultClientScript" content="JavaScript">
		<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
		<LINK href="MenuArea.css" rel="stylesheet">
		<script language='javascript'>
		var oldObjectSelect = null;
		
		function doLink(url) {
		
		    if (url != '')
			{
				parent.MenuReportArea.location.href = url;
				parent.HistoryArea.location.href = 'tbloader/serverMonitor.html'; //'OpenDocumentsPage.aspx';
				parent.topFrame.EasylookMenuReportArea = url;
				parent.topFrame.setCookie();
			}
		}
		</script>
	</HEAD>
	<body  style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px"
		scroll="yes">
		<form id="menuForm" method="post" runat="server" style="WIDTH: 100%; HEIGHT: 100%; BACKGROUND-COLOR: #eef0ff">
            &nbsp;<asp:TreeView ID="MenuTreeView" runat="server" Font-Names="Verdana" Font-Size="X-Small" ShowLines="True">
				<NodeStyle Font-Size="12px" />
            </asp:TreeView>
		</form>
	</body>
</HTML>
