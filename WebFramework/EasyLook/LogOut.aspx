<%@ Page Language="c#" CodeBehind="LogOut.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.LogOut" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>LogOut</title>
	<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
	<meta name="CODE_LANGUAGE" content="C#">
	<meta name="vs_defaultClientScript" content="JavaScript">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	<script type="text/javascript">
		function getHttpRequest() {

			try {
				return new XMLHttpRequest();
			}
			catch (ex) {
			}

			var progIDs = ['Msxml2.XMLHTTP.3.0', 'Msxml2.XMLHTTP'];
			for (var i = 0, l = progIDs.length; i < l; i++) {
				try {
					return new ActiveXObject(progIDs[i]);
				}
				catch (ex) {
				}
			}

			return null;
		}
		function disconnect() {
			var req = getHttpRequest();
			req.open('GET', 'LogOut.aspx?AutoDisconnect=true', false);
			req.send(null);
		}

		window.onunload = disconnect;
	</script>
</head>
<body style="background-color:#ffffff; text-align: center; vertical-align: middle;">
	<form id="Form1" method="post" runat="server" style="left: 0px; width: 100%; position: absolute;
	top: 0px; height: 100%" >
	<div runat="server" style="margin: 20px auto 20px auto; border: thin outset #0000FF;
		width:550px; text-align: center; background-color:#E6E6FA">
		<img alt="Logoff" src="Files/Images/Logoff.gif" style="float:left;padding:10px;"/>
		<asp:Label ID="FirstLabel" style="margin:20% auto 20% auto; display:block; text-align: center; " tyle="z-index: 103; " runat="server"></asp:Label>
		<asp:Label ID="SecondLabel" Style="margin:20% auto 20% auto; display:block; z-index: 102; " runat="server"></asp:Label>
		<asp:Button Style="z-index: 101; height: 24px; margin-bottom:20px;" ID="buttonDisconnect" runat="server"
			OnClick="OnDisconnect" />
		<asp:Button Style="z-index: 101; height: 24px; margin-bottom:20px;" ID="buttonKeepAlive" runat="server"
			OnClick="OnKeepAlive" />
	</div>
	</form>
</body>
</html>
