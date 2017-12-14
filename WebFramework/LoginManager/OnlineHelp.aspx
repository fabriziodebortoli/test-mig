<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OnlineHelp.aspx.cs" Inherits="Microarea.WebServices.LoginManager.OnlineHelp" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title></title>

	<script type="text/javascript">
		function submitform() {
			document.forms[0].submit();
		}
	</script>

</head>
<body onload="submitform();">
	<form  method="post" id="HelpForm" runat="server">
	<input name="Namespace" id="Namespace" type="hidden" runat="server" value="" />
	<input name="Language" id="Language" type="hidden" runat="server" value="" />
	<input name="SerialNumber" id="SerialNumber" type="hidden" runat="server" value="" />
	</form>
</body>
</html>
