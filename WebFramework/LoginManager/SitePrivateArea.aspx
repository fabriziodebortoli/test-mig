<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SitePrivateArea.aspx.cs" Inherits="Microarea.WebServices.LoginManager.SitePrivateArea" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
 	<title></title>

	<script type="text/javascript">
		function submitform() {
			document.forms[0].submit();
		}
	</script>

</head>
<body onload="submitform();">
	<form  method="post" action="http://www.microarea.it/int/MyAccount/MagoLoginAdmittance.aspx">
	<input name="ActivationKey" id="ActivationKey" type="hidden" runat="server" value="" />
	</form>
</body>
</html>
