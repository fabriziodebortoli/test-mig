<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PingViaSMS.aspx.cs" Inherits="Microarea.WebServices.LoginManager.PingViaSMS" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
<META Http-Equiv="Cache-Control" Content="no-cache"/>
<META Http-Equiv="Pragma" Content="no-cache"/>
<META Http-Equiv="Expires" Content="0"/> 
  <style>
.shadowcontainer{
width: 600px; /* container width*/
background-color: #d1cfd0;
}
body, li, ul, ol,td,li,span	{font-family: Verdana,Calibri,Helvetia,sans-serif; font-size: 12px; color: #333366; line-height:20px;}
 p{font-family: Verdana,Calibri,Helvetia,sans-serif; font-size: 9px; color: #333366; line-height:10px;}
  h1{font-family: Verdana,Calibri,Helvetia,sans-serif; font-size: 13px; font-weight:bold; color:Red; line-height:25px;}
.shadowcontainer .innerdiv{
/* Add container height here if desired */
background-color: white;
border: 1px solid gray;
padding: 0px;
position: relative; 
left: -5px; /*shadow depth*/
top: -5px; /*shadow depth*/
}
.ButtonStyleCenter {font-family: Verdana,Calibri,Helvetia,sans-serif; 
	           font-weight: bold; 
	           font-size: 12px; 
	           color: #333366;
	           border-style: none;
      background-image: url("img/BlueButton.png");
      background-repeat: no-repeat;
	           background-position: center;
			   padding: 2px;
			   cursor: hand;
			   width: 156px;
			   height: 26px;
} 
  	.style1
	  {
		  height: 75px;
	  }
      .auto-style1 {
          height: 25px;
      }
  </style>
  <link rel="icon" href="img/Mobile.png"  type="image/png"/>
  <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <br />
        <br />
        <center>
            <div id="sms" class="shadowcontainer" style="width: 600;" runat="server" align="left">
                <div class="innerdiv">
                    <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                        <tr>
                            <td style="background: url(img/HorizontalBlu.png) repeat-x; padding: 0px;">
                                <img id="imgLnkHP" runat="server" /></td>
                        </tr>
                        <tr>
                            <td align="center"><b>
                                <asp:Label ID="LblTitle" runat="server" /></b>
                                <br />
                                <asp:Label ID="LblInfo0" runat="server" /></td>
                        </tr>
                        <tr>
                            <td>
                                <br />
                                <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                                    <tr>
                                        <td width="10%;">
                                            <img src="img/Mobile.png" id="phone" runat="server" alt=" " /></td>
                                        <td>
                                            <asp:Label ID="LblSendSmsInfo1" runat="server" /></td>
                                    </tr>

                                    <tr>

                                        <td colspan="2" align="center">
                                            <asp:Label ID="LblSendSMS" runat="server" BorderStyle="Solid" BorderColor="#4581C3" /></td>
                                    </tr>
                                    <tr>
                                        <td></td>
                                        <td>
                                            <asp:Label ID="LblSendSmsInfo2" runat="server" /></td>
                                        <tr>
                                            <td></td>
                                            <td>
                                                <asp:Label ID="LblInsertAndClick" runat="server" /></td>
                                        </tr>

                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="img/Key.png" id="Img7" runat="server" /></td>
                                        <td>
                                            <asp:TextBox ID="TxtCode" runat="server" Font-Names="Verdana,Calibri,Helvetia,sans-serif" Font-Size="12px"></asp:TextBox>
                                            <asp:Button ID="BtnRegister" runat="server" OnClick="BtnRegister_Click" CssClass="ButtonStyleCenter" />
                                            <br>
                                            <asp:Label ID="LblInfoCase" runat="server" />

                                        </td>
                                    </tr>

                                    <tr>
                                        <td>
                                            <img src="img/ResultRed.png" id="ImgRes" runat="server" /></td>
                                        <td>
                                            <asp:Label ID="LblRes" runat="server" ForeColor="Red" /></td>
                                    </tr>

                                    <tr>
                                        <td colspan="2">
                                            <hr align="center" size="1" width="85%" color="#E3E4FA">
                                        </td>

                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="img/information.png" id="Img8" runat="server" alt=" " /></td>
                                        <td><b>
                                            <asp:Label ID="LabelNote" runat="server"></asp:Label></b></td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <img src="img/Pin36.Png" id="Img9" runat="server" alt=" " /></td>
                                        <td>
                                            <asp:Label ID="LblVatNumber1" runat="server"></asp:Label><br>
                                            <asp:Label ID="LblVatNumber2" runat="server"></asp:Label>
                                        </td>
                                    </tr>

                                    <tr>
                                        <td>
                                            <img src="img/Pin36.Png" id="Img6" runat="server" alt=" " /></td>
                                        <td>
                                            <asp:Label ID="LblRemark1" runat="server"></asp:Label><br>
                                            <asp:Label ID="LblRemark2" runat="server"></asp:Label></td>
                                    </tr>

                                    <tr>
                                        <td>
                                            <img src="img/Pin36.Png" id="Img10" runat="server" alt=" " /></td>
                                        <td>
                                            <asp:Label ID="LblReceivedError1" runat="server"></asp:Label><br>
                                            <asp:Label ID="LblReceivedError2" runat="server"></asp:Label></td>
                                    </tr>

                                    <tr>
                                        <td>
                                            <img src="img/Pin36.Png" id="Img5" runat="server" /></td>
                                        <td>
                                            <asp:Label ID="LblSMSNotReceived1" runat="server" /><br>
                                            <asp:Label ID="LblSMSNotReceived2" runat="server"></asp:Label></td>
                                    </tr>

                                    <tr>
                                        <td>
                                            <img src="img/Pin36.Png" id="Img13" runat="server" alt=" " /></td>
                                        <td>
                                            <asp:Label ID="LblSMSNotReceived3Title" runat="server"></asp:Label><br>
                                            <asp:Label ID="LblSMSNotReceived3" runat="server"></asp:Label></td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>

            <div id="error" class="shadowcontainer" style="width: 600;" runat="server" align="left">
                <div class="innerdiv">
                    <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                        <tr>
                            <td style="background: url(img/HorizontalBlu.png) repeat-x; padding: 0px;" class="auto-style1">
                                <img id="img1" runat="server" /></td>
                        </tr>

                        <tr>
                            <td>
                                <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                                    <tr>
                                        <td width="10%;">
                                            <img src="img/error2.png" id="Img2" runat="server" /></td>
                                        <td>
                                            <asp:Label ID="LblError" runat="server" /></td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <div id="OK" class="shadowcontainer" style="width: 600;" runat="server">
                <div class="innerdiv">
                    <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                        <tr>
                            <td style="background: url(img/HorizontalBlu.png) repeat-x; padding: 0px;">
                                <img src="img/LogoMicroareaMenuLongBlu.png" id="img3" runat="server" /></td>
                        </tr>

                        <tr>
                            <td>
                                <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                                    <tr>
                                        <td width="10%;">
                                            <img src="img/Ok-icon.png" id="Img4" runat="server" alt="OK" /></td>
                                        <td>
                                            <asp:Label ID="LabelOK" runat="server" /></td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </div>
            <div id="NotSupported" class="shadowcontainer" style="width: 600;" runat="server">
                <div class="innerdiv">
                    <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                        <tr>
                            <td style="background: url(img/HorizontalBlu.png) repeat-x; padding: 0px;">
                                <img src="img/LogoMicroareaMenuLongBlu.png" id="img11" runat="server" /></td>
                        </tr>

                        <tr>
                            <td align="center">
                                <table cellspacing="0" cellpadding="5" border="0" width="100%;">
                                    <tr>

                                        <td align="center">
                                            <asp:Label ID="LabelWip" runat="server" />
                                            <br />
                                            <br />

                                            <asp:Label ID="LblSendSMS_NS" runat="server" BorderStyle="Solid" BorderColor="#4581C3" />
                                            <br />
                                            <asp:Label ID="Label1" runat="server" />
                                            <br />
                                            <asp:TextBox ID="TextBox2" runat="server" Font-Names="Verdana,Calibri,Helvetia,sans-serif" Font-Size="12px" />
                                            <asp:Button ID="Button2" runat="server" OnClick="BtnRegister_Click" CssClass="ButtonStyleCenter" />
                                        </td>
                                    </tr>
                                    <tr>
                                        <td align="center">
                                            <img src="img/ResultRed.png" id="ImgresNotsup" runat="server" alt="img" /><asp:Label ID="LblResNotsup" runat="server" ForeColor="Red" />
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>

                    </table>
                </div>
            </div>
        </center>
    </form>
    <table cellspacing="0" cellpadding="5" border="0" width="600;" align="center">
        <tr>
            <td>
                <asp:Label ID="LblInfo" runat="server" /></td>
        </tr>
    </table>
</body>
</html>
