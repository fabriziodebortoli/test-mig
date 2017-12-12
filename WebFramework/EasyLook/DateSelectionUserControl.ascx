<%@ Control Language="c#" AutoEventWireup="True" Codebehind="DateSelectionUserControl.ascx.cs" Inherits="Microarea.Web.EasyLook.DateSelectionUserControl" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="cc1" Namespace="Microarea.TaskBuilderNet.UI.WebControls" Assembly="Microarea.TaskBuilderNet.UI" %>
<table style="TABLE-LAYOUT: fixed; DISPLAY: block; VISIBILITY: visible; OVERFLOW: auto; WIDTH: 271px; BORDER-COLLAPSE: collapse; HEIGHT: 218px"
	height="218" cellSpacing="4" cols="2" cellPadding="6" width="271" align="left">
	<tr>
		<td style="WIDTH: 153px; POSITION: relative; TOP: 0px; HEIGHT: 38px" vAlign="top" align="left"><asp:dropdownlist id="MonthDropDownList" AutoPostBack="True" Font-Size="10pt" Font-Names="Verdana"
				ForeColor="Navy" runat="server" Width="100%" Height="30px" onselectedindexchanged="MonthDropDownList_SelectedIndexChanged"></asp:dropdownlist></td>
		<td style="LEFT: 0px; WIDTH: 100px; POSITION: relative; TOP: -4px; HEIGHT: 40px" vAlign="top"
			align="right"><cc1:numericupdowncontrol id="YearUpDown" tabIndex="1" Font-Size="10pt" Font-Names="Verdana" ForeColor="Navy"
				runat="server" Width="65px" Height="23px" InputCharSize="4" PixelWidth="82" PixelHeight="30" FontSize="10" BorderWidth="0px"
				Text="2004" MaxValue="2200" MinValue="1900" Value="2004" onvaluechanged="YearUpDown_ValueChanged"></cc1:numericupdowncontrol></td>
	</tr>
	<tr>
		<td style="LEFT: 0px; WIDTH: 100%; POSITION: relative; TOP: 0px" vAlign="top" noWrap
			align="left" colSpan="2"><asp:calendar id="SelectDayCalendar" tabIndex="2" Font-Size="10pt" Font-Names="Verdana" ForeColor="Navy"
				runat="server" Width="252px" Height="161px" BorderWidth="0px" CellPadding="0" ShowTitle="False" ShowNextPrevMonth="False"
				SelectWeekText=" " SelectMonthText=" " PrevMonthText=" " NextMonthText=" " onselectionchanged="SelectDayCalendar_SelectionChanged">
				<DayStyle Wrap="False" HorizontalAlign="Right" VerticalAlign="Middle"></DayStyle>
				<DayHeaderStyle HorizontalAlign="Right" BorderWidth="0px" VerticalAlign="Middle" BackColor="LightSteelBlue"></DayHeaderStyle>
				<SelectedDayStyle BorderWidth="1px" ForeColor="White" BorderStyle="Solid" BorderColor="CornflowerBlue"
					BackColor="LightSteelBlue"></SelectedDayStyle>
				<OtherMonthDayStyle Wrap="False" HorizontalAlign="Right" ForeColor="LightSlateGray" VerticalAlign="Middle"></OtherMonthDayStyle>
			</asp:calendar></td>
	</tr>
</table>
