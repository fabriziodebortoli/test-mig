/// <reference path="https://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery.min.js" />

$(function () {
	$(".IntFormatter").keypress(new IntFormatter().onKeyPress);
});

//======================================================================
function LongFormatter() {
	this.onKeyPress = function (e) {
	}
}

//======================================================================
function IntFormatter() {
	var	nMin = 0;
	var nMax = 50;

	// events
	this.onKeyPress = function (e) { DoOnChar(e); }

	//------------------------------------------------------------------
	function DoOnChar(e) {

		if (e.target.disabled == true)
			return true;

		if (istcntrl(e.charCode) && e.charCode != 8 /*VK_BACK*/)
			return false;

		var nChar = e.charCode;
		var loPos = e.target.selectionStart;
		var hiPos = e.target.selectionEnd;
		var strValue = e.target.value;

		var thousandSeparator = ".";
		var nVal = Unformat(strValue);

		if (nChar == '-')
		{	
			if (nMin >= 0 || (nVal == 0 && (loPos > 0 || hiPos > 0)))
			{
				alert("Bad input!");
				return true;
			}
		
			if (nVal)
			{
				nVal = -nVal;

				SetValue(nVal); // TODO

				var	strTmp; 

				CParsedCtrl::GetValue(strTmp); // TODO

				SetModifyFlag(TRUE); // NON SERVE PIU'

				if (strTmp[0] != strValue[0] && istdigit(strTmp[0]) != istdigit(strValue[0])) 
				{
					if (istdigit(strTmp[0]))
						loPos--;
					else
						loPos++;
				}
				e.target.selectionStart = loPos;
				e.target.selectionEnd = loPos;

				return true;
			}

			return false;
		}

		if	((!istdigit(nChar) && nChar != 8/*VK_BACK*/) ||
			(loPos == 0 && hiPos == 0 && strValue != "" && !istdigit(strValue[0])))
		{
			alert("Bad input!");
			return true;
		}

		if (nChar == 8/*VK_BACK*/)
			switch (ManageNumericBackKey(e, strValue, dwPos, nPos, ch1000Sep))
			{
				case -1 : return true;
				case 0	: return false;
			}

		var nCurNr1000Sep = UpdateNumericString(nVal, strValue, dwPos, nPos, nChar, ch1000Sep);
		if (nCurNr1000Sep < 0)
		{
			alert("Bad input!");
			return true;
		}

		var dVal = UnFormat(strValue);

		if (dVal < -32768 || dVal > 32767)
		{
			alert("Bad input!");
			return true;
		}

		UpdateNumericInput(dVal, strValue, dwPos, nPos, ch1000Sep, nCurNr1000Sep);

		return true;
	}
}

//------------------------------------------------------------------
function istcntrl(nChar) {
	return ((nChar >= 0 && nChar <= 31) || nChar == 127);
}

//------------------------------------------------------------------
function istdigit(nChar) {
	return (nChar >= 48 && nChar <= 57);
}

//------------------------------------------------------------------
function Unformat(strValue) {
}

//------------------------------------------------------------------
function ManageNumericBackKey(e, strValue, loPos, hiPos, ch1000Sep, chDecSep) {
	if (strValue == "")
		return -1;

		if (loPos == hiPos && loPos > 1 && strValue[loPos - 1] == chDecSep)
		{
			e.target.selectionStart = loPos - 1;
			e.target.selectionEnd = loPos - 1;
			return -1;
		}

		if (loPos == hiPos && loPos > 1 && strValue[loPos - 1] == ch1000Sep)
		{
			loPos -= 2;
			dwPos = MAKELONG(nPos, nPos + 2); // TODO
		}

		var n = (loPos == hiPos) ? 1 : 0;
		var i = 0;
		for (i = 0; i < loPos - n; i++)
			if (strValue[i] != '0' && strValue[i] != ch1000Sep && strValue[i] != '-')
				break;
		if	(
				hiPos != strValue.length &&
				(
					strValue[hiPos] == chDecSep	||
					strValue[hiPos] == '0'		||
					(
						strValue[hiPos] == ch1000Sep &&
						strValue[hiPos + 1] == '0'
					)
				) &&
				(i == nPos - n || nPos - n <= 0)
			)
		{
			var minPos = Math.min(loPos, strValue[0] == '-' ? 1 : 0);
			e.target.selectionStart = minPos;
			e.target.selectionEnd = hiPos;
			return 0;
		}

		return 1;
}

//------------------------------------------------------------------
function UpdateNumericString(strValue, loPos, hiPos, /* verificare passaggio per ref! int& nPos,*/ nChar, ch1000Sep)
{
	if (nChar == 8/*VK_BACK*/)
	{
		if (loPos == hiPos)
		{
			strValue = strValue.slice(0, loPos - 1) + strValue.slice(loPos, strValue.lenght);
			loPos -= 2;
		}
		else
		{
			strValue = strValue.slice(0, loPos) + strValue.slice(hiPos, strValue.lenght);
			loPos -= 1;
		}
	}
	else
		if (loPos == hiPos)
			strValue = strValue.slice(0, loPos) + (nChar + strValue.slice(loPos, strValue.lenght));
		else
			strValue = strValue.slice(0, loPos) + (nChar + (hiPos <= strValue.length ? strValue.slice(hiPos, strValue.lenght) : ""));

	var nCurNr1000Sep = 0;

	for (var i = 0; i < strValue.length; i++)
		if (strValue[i] == ch1000Sep) 
			nCurNr1000Sep++;

	return nCurNr1000Sep;
}

//IntFormatter.prototype = new LongFormatter();