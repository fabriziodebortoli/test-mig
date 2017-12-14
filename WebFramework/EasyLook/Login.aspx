<%@ Page Language="c#" CodeBehind="Login.aspx.cs" AutoEventWireup="True" Inherits="Microarea.Web.EasyLook.Login" %>

<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >
<html>
<head runat="server">
	<title>Login</title>
	<meta content="True" name="vs_showGrid"/>
	<meta content="Microsoft Visual Studio .NET 7.1" name="GENERATOR"/>
	<meta content="C#" name="CODE_LANGUAGE"/>
	<meta content="JavaScript" name="vs_defaultClientScript"/>
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema"/>
	<link href="EasyLook.css" rel="stylesheet"/>
	<link rel="shortcut icon" href="Files/Images/EasyLookShortcut.ico" />

	<script type="text/javascript">

		function RedirectIfNeeded() {
			if (parent && parent != this) {
				document.body.style.visibility = "hidden";
				document.body.style.display = "none";
				parent.location.href = 'default.aspx';
			}
		}
		function AskForOverWrite(message) {
			if (!window.originalAction)
				window.originalAction = document.forms[0].action;
			if (window.confirm(message)) {
				var toAdd = '&OverwriteLogin=true';
				document.forms[0].action = window.originalAction + toAdd;
				$get('OkButton').click();
			}
			else {
				document.forms[0].action = window.originalAction;
			}
		}

		function saveCompanyCookie() {
			
			var today = new Date();
			var expire = new Date();
			expire.setTime(today.getTime() + 3600000 * 24 * 100);
			//pesca il nome dalla combo della company
			var companyCombo = document.getElementById("CompanyComboBox");
			if (companyCombo)
				document.cookie = "lastUsedCompanyName=" + escape(companyCombo.options[companyCombo.selectedIndex].value) + ";expires=" + expire.toGMTString();
			}

		function onLoad() {
			RedirectIfNeeded();
			getCompanyCookie();
		}

		function getCompanyCookie() {
			if (document.cookie.length > 0) {
				var cookieName = "lastUsedCompanyName";
				var cookieStart = document.cookie.indexOf(cookieName + "=");
				if (cookieStart != -1) {
					cookieStart = cookieStart + cookieName.length + 1;
					var cookieEnd = document.cookie.indexOf(";", cookieStart);
					if (cookieEnd == -1)
						cookieEnd = document.cookie.length;
					//setta il valore della company nel campo hidden
					var lastCompanyUsed = unescape(document.cookie.substring(cookieStart, cookieEnd));

					var hiddenLastCompanyField = document.getElementById("lastCompanyUsed");
					if (hiddenLastCompanyField)
						hiddenLastCompanyField.value = lastCompanyUsed;

				}
			}
		}
	</script>

</head>
<body bgcolor="#ffffff" ms_positioning="GridLayout" onload="onLoad();">
	<form id="Form1" method="post" runat="server" style="left: 0px; width: 100%; position: absolute;
	top: 0px; height: 100%">
	<asp:ScriptManager runat="server">
	</asp:ScriptManager>
	 <asp:hiddenfield id="lastCompanyUsed" value="" runat="server"/>
			
			<asp:UpdatePanel UpdateMode="Conditional" runat="server" ID="OuterUpdatePanel">
			<ContentTemplate>
			
			<center>
				<asp:Label ID="TitleLabel1" runat="server" Width="985px" Height="16px" Font-Names="Verdana"
					Font-Size="10pt" Font-Bold="True" BackColor="Transparent"></asp:Label>
				<table>
					<tr>
						<td>
							&nbsp;
						</td>
					</tr>
				</table>
				<asp:Panel ID="BkgndPanel" Style="z-index: 101" runat="server" Width="499px" BackColor="Lavender"
					BorderColor="#C0C0FF" BorderStyle="Ridge" Height="400px">
					&nbsp;
					<table id="MasterTable" style="z-index: 101; width: 496px; height: 304px" 
						cellspacing="0" cellpadding="0" width="496" bgcolor="lavender">
						<tr>
							<td style="width: 56px;" valign="top" align="center" bgcolor="#e6e6fa" >
								<asp:Image ID="Image1" runat="server"  style="margin-left: 13px;" ImageUrl="~/Files/Images/Login.JPG"></asp:Image>
							</td>
							<td runat="server" id="ImageLogoCell" align="right" bgcolor="#e6e6fa">
		
							</td>
						</tr>
						<tr>
							<td valign="middle" align="center" colspan="2">
								<asp:Label ID="TitleLabel" Font-Bold="True" Font-Size="10pt" Font-Names="Verdana"
									runat="server" Font-Name="Verdana" ForeColor="#3560D0"></asp:Label>
								<br />
							</td>
							<td>
						    </td>
						</tr>
						<tr >
							<td valign="top" align="right" colspan="2" >
								
								
								<asp:Panel runat="server" ID="PanelLogin" Visible="true">
								<table id="LoginTable" style="width: 383px;" cellspacing="0" cellpadding="0" width="383" align="center"
									bgcolor="#e6e6fa" border="0">
									<tr style="height:50px;">
										<td bgcolor="lavender" colspan="1" rowspan="1" >
											<asp:Label ID="UserLabel" runat="server" Font-Size="10pt" Font-Names="Verdana" Width="48px"></asp:Label>
											<br />
											<asp:TextBox ID="UserTextBox" TabIndex="1" runat="server" Font-Size="10pt" Font-Names="Verdana"
												Width="380" AutoPostBack="True" OnTextChanged="UserTextBox_TextChanged"></asp:TextBox>
										</td>
									</tr>
									<tr style="height:50px;">
										<td bgcolor="lavender" >
											<asp:Label ID="PasswordLabel" runat="server" Font-Size="10pt" Font-Names="Verdana"></asp:Label>
										<br />
										<asp:TextBox ID="PasswordTextBox" TabIndex="2" runat="server" Font-Size="10pt" Font-Names="Verdana"
												Width="380px" TextMode="Password"></asp:TextBox>
										</td>
									</tr>
									
									
									<tr style="height:50px;">
										<td style="height: 15px;" bgcolor="lavender">
										<asp:UpdatePanel UpdateMode="Conditional" runat="server" ID="UpdatePanelCompanies">
											<ContentTemplate>
													<asp:Label ID="CompanyLabel" runat="server" Font-Size="10pt" Font-Names="Verdana"
														Visible="False"></asp:Label>
												<br />
												
													<asp:DropDownList ID="CompanyComboBox" TabIndex="3" runat="server" Font-Size="10pt"
														Font-Names="Verdana" Width="380px" Visible="False">
													</asp:DropDownList>
											
											</ContentTemplate>
											<Triggers>
												<asp:AsyncPostBackTrigger ControlID="UserTextBox"/> 
											</Triggers> 
										</asp:UpdatePanel>
										</td>
										</tr>	
									
								</table>
								</asp:Panel>

								<asp:Panel runat="server" ID="PanelChangePwd" Visible="false">
								<table id="Table1" style="width: 383px;" cellspacing="0" cellpadding="0" width="383" align="center"
									bgcolor="#e6e6fa" border="0">
									<tr style="height:50px;">
										<td bgcolor="lavender" colspan="1" rowspan="1" >
											<asp:Label ID="LabelOldPwd" runat="server" Font-Size="10pt" Font-Names="Verdana" >OldPwd</asp:Label>
											<br />
											<asp:TextBox ID="TextBoxOldPwd" TabIndex="1" runat="server" Font-Size="10pt" Font-Names="Verdana"
												Width="380" TextMode="Password"></asp:TextBox>
										</td>
									</tr>

									<tr style="height:50px;">
										<td bgcolor="lavender" colspan="1" rowspan="1" >
											<asp:Label ID="LabelNewPwd" runat="server" Font-Size="10pt" Font-Names="Verdana" >New pwd</asp:Label>
											<br />
											<asp:TextBox ID="TextBoxNewPwd" TabIndex="1" runat="server" Font-Size="10pt" Font-Names="Verdana"
												Width="380" TextMode="Password"></asp:TextBox>
										</td>
									</tr>
									<tr style="height:50px;">
										<td bgcolor="lavender" >
											<asp:Label ID="LabelConfirmNewPwd" runat="server" Font-Size="10pt" Font-Names="Verdana">Confirm new pwd</asp:Label>
										<br />
										<asp:TextBox ID="TextBoxConfirmNewPwd" TabIndex="2" runat="server" Font-Size="10pt" Font-Names="Verdana"
												Width="380px" TextMode="Password"></asp:TextBox>
										</td>
									</tr>
									
									
									<tr style="height:50px;">
										<td style="height: 15px; bgcolor:lavender; text-align:center;">
											<asp:Button ID="ChangePwdBtn" runat="server" Font-Names="Verdana" Font-Size="10pt" 
											style="margin-top:15px;" 
											Width="170px"
											OnClick="ChangePwdButton_Click" />	
										</td>
									</tr>	

									<tr style="height:50px;">
										<td style="height: 15px; bgcolor:lavender; text-align:center;">
											<asp:Label ID="ChangePwdLabelMessage" runat="server" Font-Size="10pt" Font-Names="Verdana" Visible="False"
											ForeColor="red"></asp:Label>
										</td>
									</tr>	
									
								</table>
								</asp:Panel>
							</td>
		
						</tr>
						<tr>
							<td align="center" bgcolor="lavender" colspan="2">
							<asp:Panel runat="server" ID="OkPanel">
								<asp:UpdatePanel UpdateMode="Conditional" runat="server" ID="UpdatePanel1">
									<ContentTemplate>
										<asp:Button ID="OkButton" runat="server" Font-Names="Verdana" Font-Size="10pt" 
											OnClick="OkButton_Click" OnClientClick="saveCompanyCookie();"
											 style="margin-left:auto; margin-right:auto;margin-top:15px;" 
											TabIndex="4" Width="170px" />
										<br />
										<br />
										<asp:Label ID="MsgLabel" runat="server" Font-Size="10pt" Font-Names="Verdana" Visible="False"
											ForeColor="red"></asp:Label>
									</ContentTemplate>
									<Triggers>
										<asp:AsyncPostBackTrigger ControlID="UserTextBox"/> 
									</Triggers> 
								</asp:UpdatePanel>
							</asp:Panel>
							</td>
						</tr>
					</table>
				</asp:Panel>
				<table>
					<tr>
						<td>
							&nbsp;
						</td>
					</tr>
				</table>
				<asp:Label ID="BottomLabel" runat="server" Width="985px" Height="9px" Font-Names="Verdana"
					Font-Size="10pt" Font-Bold="True" BackColor="Transparent"></asp:Label>
			</center>
			
			</ContentTemplate>
			</asp:UpdatePanel>
	</form>
</body>
</html>
