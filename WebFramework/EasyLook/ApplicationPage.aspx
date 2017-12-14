<%@ Register TagPrefix="cc1" Namespace="Microarea.TaskBuilderNet.UI.WebControls" Assembly="Microarea.TaskBuilderNet.UI" %>
<%@ Page language="c#" Codebehind="ApplicationPage.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.ApplicationPage" %>
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<HTML>
	<HEAD>
		<title>ApplicationPage</title>
		<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR">
		<meta content="C#" name="CODE_LANGUAGE">
		<meta content="JavaScript" name="vs_defaultClientScript">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<LINK href="EasyLook.css" rel="stylesheet">
		<script language="javascript">
			var oldObjectSelect = null;
			function doLink(url, objectSelect) {
				if (objectSelect != null) {
					objectSelect.color = "red";
					if (oldObjectSelect != null)
						oldObjectSelect = "Blue";

					oldObjectSelect = objectSelect;
				}
				if (url != '') {
					parent.MenuArea.location.href = url;
					parent.MenuReportArea.location.href = 'MenuItemsPage.aspx';
					parent.HistoryArea.location.href = 'tbloader/serverMonitor.html';//'OpenDocumentsPage.aspx';
					parent.topFrame.EasylookMenuArea = url;
					parent.topFrame.EasylookMenuReportArea = "";
					parent.topFrame.setCookie()
				}
			}
			function Redirect() {
				if (this.DisplayFirstGroup != null)
				{
					this.DisplayFirstGroup();
					this.DisplayFirstGroup = null;
				}
			} 

		</script>
	</HEAD>
	<body id="applicationsBody" bottomMargin="0" leftMargin="0" topMargin="0" onload="Redirect();"
		MS_POSITIONING="GridLayout" scroll="yes" style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 0px; PADDING-TOP: 0px"
		rightMargin="0">
		<form id="applicationForm" method="post" runat="server" style="PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; WIDTH: 100%; MARGIN-RIGHT: 0px; PADDING-TOP: 0px; HEIGHT: 103.18%">
			<center>
				<cc1:menuapplicationspanelbar id="menuApplicationsPanelBar" runat="server"></cc1:menuapplicationspanelbar>
			</center>
		</form>
	</body>
</HTML>
