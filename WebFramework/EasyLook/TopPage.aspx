<%@ Page Language="c#" CodeBehind="TopPage.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.TopPage" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head runat="server">
	<title>ImagePage</title>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR"/>
	<meta content="C#" name="CODE_LANGUAGE"/>
	<meta content="JavaScript" name="vs_defaultClientScript"/>
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema"/>

	<script language="javascript" type="text/javascript">

		if (parent.HistoryArea.location.pathname == "/HistoryPage.aspx")
			parent.HistoryArea.location.href = 'HistoryPage.aspx?Type=ReportType';

		function Redirect() {
			if (this.RedirectFunction != null) {
				this.RedirectFunction();
				this.RedirectFunction = null;
			}

			parent.selectedObj = document.getElementById("runReportButton");
		}

		function demoAlert(message) {
			window.setTimeout(function() { alert(message); }, 100);
		}

		function PerformClosingOperations() {
			closeOpenedDocs();
			if (parent.suppressLogoff === true)
				return;
			window.open("LogOut.aspx", "LogOut", "width=500, height=500, location=no, menubar=no, resizable=no, scrollbars=no, status=no, titlebar=no, toolbar=no");
		}


		function Logoff() {
			parent.suppressLogoff = true;
		}

		function setCookie() {
			var today = new Date();
			var expire = new Date();
			expire.setTime(today.getTime() + 3600000 * 24 * 100);
			var value1 = (window.EasylookMenuArea == undefined) ? "" : EasylookMenuArea;
			var value2 = (window.EasylookMenuReportArea == undefined) ? "" : EasylookMenuReportArea;
			var cookieValue = value1 + ';' + value2;
			document.cookie = "menu=" + escape(cookieValue) + ";expires=" + expire.toGMTString();
		}

		function getCookie() {
			if (document.cookie.length > 0) {
				var cookieName = "menu";
				var cookieStart = document.cookie.indexOf(cookieName + "=");
				if (cookieStart != -1) {
					cookieStart = cookieStart + cookieName.length + 1;
					var cookieEnd = document.cookie.indexOf(";", cookieStart);
					if (cookieEnd == -1)
						cookieEnd = document.cookie.length;
					return unescape(document.cookie.substring(cookieStart, cookieEnd));
				}
			}
			return "";
		}

		function LoadMenuFromCookie() {
			var cookieValue = getCookie();
			if (cookieValue == "") {
				return;
			}
			var separatorIdx = cookieValue.indexOf(';');
			window.EasylookMenuArea = cookieValue.substr(0, separatorIdx);
			window.EasylookMenuReportArea = cookieValue.substring(separatorIdx + 1, cookieValue.length);

			if (window.EasylookMenuArea != "")
				parent.MenuArea.location.href = window.EasylookMenuArea;
			if (window.EasylookMenuReportArea != "")
				parent.MenuReportArea.location.href = window.EasylookMenuReportArea;
		}

	</script>

	<style type="text/css">
		#trail
		{
			z-index: 200;
			visibility: hidden;
			position: absolute;
		}
	</style>
	<link href="header.css" type="text/css" rel="stylesheet" />
</head>
<body style="padding-right: 0px; padding-left: 0px; padding-bottom: 0px; margin: 0px;
	padding-top: 0px;" onload="Redirect(); LoadMenuFromCookie()">
	<span id="trail" style="visibility: hidden"></span>
	<form id="Form1" method="post" runat="server">
	<div style="position: absolute; width: 100%; height: 100%; top: 0px; left: 0px;">
		<asp:Table ID="TableToolbar" runat="server" CellSpacing="0" CellPadding="0" Width="100%"
			Height="103px" bgcolor="lavender" border="0">
			<asp:TableRow>
				<asp:TableCell RowSpan="2" Style="width: 200px; height: 70px">
					<asp:Image ImageAlign="Bottom" ID="logoImage" runat="server" ImageUrl="Files/Images/EasyLookLogo.gif">
					</asp:Image>
				</asp:TableCell>
				<asp:TableCell class="cell" Style="height: 70px" align="left" Width="10">
					<img alt="" src="Files/Images/Left.GIF" border="0"/>
				</asp:TableCell>
				<asp:TableCell class="cell" Style="height: 70px" align="right" background="Files/Images/SfondoNewTrasparente.gif">
					<asp:Table ID="TablePulsantiera" runat="server" CellSpacing="0" CellPadding="0" BorderWidth="0">
						<asp:TableRow>
							<asp:TableCell CssClass="cell" ID="Login">
								<asp:ImageButton ID="LogoffButton" runat="server" OnClientClick="Logoff();" OnClick="LogoffButton_Click"
									ImageUrl=""></asp:ImageButton>
							</asp:TableCell>
							<asp:TableCell CssClass="cell" ID="ChangeData">
								<asp:HyperLink ID="changeDataButton" ImageUrl="" NavigateUrl="javascript:parent.ChangeApplicationDate();"
									runat="server" />
							</asp:TableCell><asp:TableCell CssClass="cell" ID="RunReport">
								<asp:HyperLink ID="runReportButton" runat="server" ImageUrl="" NavigateUrl=""></asp:HyperLink>
							</asp:TableCell><asp:TableCell CssClass="cell" ID="SearchCommand">
								<asp:HyperLink ID="SearchCommandButton" runat="server" ImageUrl="" NavigateUrl="javascript:parent.SearchPopUp();" />
							</asp:TableCell>
                            <asp:TableCell CssClass="cell" ID="TableCell1">
                                <asp:HyperLink ID="LinkSitePrivateAreaButton" runat="server" ImageUrl="" NavigateUrl="javascript:parent.OpenPrivateArea();" />
							</asp:TableCell>
                            <asp:TableCell CssClass="cell2" ID="filterReportLabel" Wrap="False">
								<asp:Label CssClass="LabelStyle" ID="reportTypeLabel" runat="server" Width="148px"
									Font-Names="Verdana" Font-Size="X-Small" ForeColor="White" Height="16px" Font-Bold="True"></asp:Label>&nbsp;
							</asp:TableCell><asp:TableCell CssClass="cell2" ID="filterReportCombo">
								<asp:DropDownList ID="reportTypeCombo" runat="server" Width="184px" Font-Name="Verdana"
									Font-Size="X-Small" AutoPostBack="True" OnSelectedIndexChanged="reportTypeCombo_SelectedIndexChanged">
								</asp:DropDownList>
							</asp:TableCell></asp:TableRow>
					</asp:Table>
				</asp:TableCell><asp:TableCell class="cell" Style="height: 70px" align="left" Width="10">
					<img alt="" src="Files/Images/Right.GIF" border="0"/> </asp:TableCell></asp:TableRow>
			<asp:TableRow>
				<asp:TableCell>
				</asp:TableCell>
				<asp:TableCell>
				</asp:TableCell>
				<asp:TableCell ID="cellCompanyLogo">
				</asp:TableCell>
			</asp:TableRow>
		</asp:Table>
	</div>
	</form>
</body>
</html>
