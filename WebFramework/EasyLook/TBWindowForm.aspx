<%@ Page Language="C#" EnableEventValidation="false" AutoEventWireup="true" CodeBehind="TBWindowForm.aspx.cs" ValidateRequest="false" Inherits="Microarea.Web.EasyLook.TBWindowForm" %>
<%@ Register Assembly="Microarea.TaskBuilderNet.UI" Namespace="Microarea.TaskBuilderNet.UI.TBWebFormControl"
	TagPrefix="cc1" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Task Builder.Net</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
	<script type="text/javascript">
		function refreshOpenDocumentsPage() {
			try
			{
				if (opener != null) {
				    opener.parent.HistoryArea.location.href = 'tbloader/serverMonitor.html'; //'OpenDocumentsPage.aspx';
				}
			}
			catch(ex)
			{
			}
		}
	</script>


</head>

<body style="width: 100%; height: 100%" onload = "refreshOpenDocumentsPage();" onunload = "refreshOpenDocumentsPage()";>
	<form id="form1" runat="server" style="width: 100%; height: 100%">
		<asp:ScriptManager ID="ScriptManager1" runat="server" />
		<cc1:TBWebFormControl ID="wcf" runat="server" OwnerApplication="WebFramework" OwnerModule="EasyLook" />
	</form>
</body>
</html>
