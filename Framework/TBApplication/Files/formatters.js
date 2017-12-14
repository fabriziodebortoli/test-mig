
/// <reference path="const.js" />

function attachFormatter(extItem, jsonFormatter) {

	if (extItem && jsonFormatter) {
		if (jsonFormatter.name == textFormatter) {
			// text formatter
			return;
		}
		var oValidator = null;
		var oFormatter = null;
		var oAdapter = null;
		var sFormatType = "";
		if (jsonFormatter.name == dateFormatter) {
			// date formatter
			sFormatType = jsonFormatter.formatType;
			var sDayFormat = jsonFormatter.dayFormat;
			var sMonthFormat = jsonFormatter.monthFormat;
			var sYearFormat = jsonFormatter.yearFormat;
			var sFirstSep = jsonFormatter.firstSep;
			var sSecSep = jsonFormatter.secondSep;

			oFormatter = new DateFormatter({ formatType: sFormatType, prefix: "", postFix: "", yearFormat: sYearFormat, firstSep: sFirstSep,
				monthFormat: sMonthFormat, secSep: sSecSep, dayFormat: sDayFormat
			});
			// signMode, formatType, sThousandSeparator, cDecimalSeparator
			oAdapter = new DateFieldFormatterExtender(extItem, oFormatter, new DateValidator({ firstSep: sFirstSep, sSecSep: sSecSep }));
			return oAdapter;
		}
		if (jsonFormatter.name == integerFormatter || jsonFormatter.name == longFormatter) {
			// integer formatter
			// Document.OFM.OfficeFiles.Documents.Task
			sFormatType = jsonFormatter.formatType;
			var sSignMode = jsonFormatter.sign;
			// IntegerFormatter(formatType, mask, prologue, epilogue, cThousandSeparator, iWidth, sNullValue, signMode)
			oFormatter = new IntegerFormatter({formatType : sFormatType, mask : "", prologue : "", epilogue : "",
				thousandSeparator: jsonFormatter.thousandSeparator, width: 0, nullValue: jsonFormatter.asZeroValue, signMode: sSignMode
			});

			oValidator = new IntegerValidator({ signMode: sSignMode, formatType: sFormatType, thousandSeparator: jsonFormatter.thousandSeparator });
			// this may be shared by all cases.
			oAdapter = new TextBoxFormatterExtender(extItem, oFormatter, oValidator);
			// TODO test it.
			return oAdapter;
		}
		if (jsonFormatter.name == percentFormatter) {
			sFormatType = jsonFormatter.formatType;
			var sSignMode = jsonFormatter.sign;

			oFormatter = new PercentageFormatter({ formatType: sFormatType, mask: "", prologue: "", epilogue: "", thousandSeparator: jsonFormatter.thousandSeparator, 
				decimalSeparator: jsonFormatter.decimalSeparator, width: 0, nullValue: jsonFormatter.asZeroValue, signMode: sSignMode,
				decimals: jsonFormatter.decimals, showZeros: true, roundMode: jsonFormatter.roundingMode, roundQuantum: jsonFormatter.roundingQuantum
			});
			oValidator = new RealValidator({signMode: sSignMode, formatType: sFormatType,
				thousandSeparator: jsonFormatter.thousandSeparator, decimalSeparator: jsonFormatter.decimalSeparator, 
				decimals: jsonFormatter.decimals
			});
			// this may be shared by all cases.
			oAdapter = new TextBoxFormatterExtender(extItem, oFormatter, oValidator);
			// TODO test it.
			return oAdapter;
		}
		if (jsonFormatter.name == moneyFormatter || jsonFormatter.name == notAccountable || jsonFormatter.name == quantity) {
			sFormatType = jsonFormatter.formatType;
			var sSignMode = jsonFormatter.sign;

			oFormatter = new RealFormatter({ formatType: sFormatType, mask: "", prologue: "", epilogue: "", thousandSeparator: jsonFormatter.thousandSeparator,
				decimalSeparator: jsonFormatter.decimalSeparator, width: 0, nullValue: jsonFormatter.asZeroValue, signMode: sSignMode,
				decimals: jsonFormatter.decimals, showZeros: true, roundMode: jsonFormatter.roundingMode, roundQuantum: jsonFormatter.roundingQuantum
			});
			oValidator = new RealValidator({ signMode: sSignMode, formatType: sFormatType,
				thousandSeparator: jsonFormatter.thousandSeparator, decimalSeparator: jsonFormatter.decimalSeparator,
				decimals: jsonFormatter.decimals
			});
			// this may be shared by all cases.
			oAdapter = new TextBoxFormatterExtender(extItem, oFormatter, oValidator);
			// TODO test it.
			return oAdapter;
		}
		if (jsonFormatter.name == elapsedFormatter) {
			sFormatType = jsonFormatter.formatType;
			var sTimeSeparator = jsonFormatter.timeSeparator;
			var sDecSeparator = jsonFormatter.decimalSeparator;
			var sDecNumber = jsonFormatter.decNumber;
			oFormatter = new ElapsedFormatter({ formatType: sFormatType, timeSeparator: sTimeSeparator, decSeparator: sDecSeparator, decNumber: sDecNumber });
			oValidator = new ElapsedValidator({ formatType: sFormatType, timeSeparator: sTimeSeparator, decSeparator: sDecSeparator, decNumber: sDecNumber });
			// this may be shared by all cases.
			oAdapter = new TextBoxFormatterExtender(extItem, oFormatter, oValidator);
			// TODO test it.
			return oAdapter;
		}
		if (jsonFormatter.name == timeFormatterID) {
			// TODO
			sFormatType = jsonFormatter.formatType;
			var sTimeSeparator = jsonFormatter.timeSep;
			var iTimeFormat = jsonFormatter.timeFormat;
			var sTimeAM = jsonFormatter.timeAM;
			var sTimePM = jsonFormatter.timePM;
			var oConfig = {timeFormat: iTimeFormat, timeSeparator: sTimeSeparator, timeAM: sTimeAM, timePM: sTimePM};
			oFormatter = new TimeFormatter(oConfig);
			oValidator = new TimeValidator(oConfig);
			// this may be shared by most cases.
			oAdapter = new TextBoxFormatterExtender(extItem, oFormatter, oValidator);            
			return oAdapter;
		}
		if (jsonFormatter.name == dateTimeFormatter) {
			// date time format.           
			var sTimeSeparator = jsonFormatter.timeSep;
			var iTimeFormat = jsonFormatter.timeFormat;
			var sTimeAM = jsonFormatter.timeAM;
			var sTimePM = jsonFormatter.timePM;

			var sDateOrder = jsonFormatter.formatType;
			var sDayFormat = jsonFormatter.dayFormat;
			var sMonthFormat = jsonFormatter.monthFormat;
			var sYearFormat = jsonFormatter.yearFormat;
			var sFirstSep = jsonFormatter.firstSep;
			var sSecSep = jsonFormatter.secondSep;
			var timeConfig = {timeFormat: iTimeFormat, timeSeparator: sTimeSeparator, timeAM: sTimeAM, timePM: sTimePM};
			var timeFormatter = new TimeFormatter(timeConfig);
			//    //        currentExtItem.formatterAdapter = new TextBoxFormatterExtender(currentExtItem, oFormatter);
			// signMode, formatType, sThousandSeparator, cDecimalSeparator
			oTimeAdapter = new TextBoxFormatterExtender(extItem.getComponent(1), timeFormatter, new TimeValidator(timeConfig));

			oFormatter = new DateFormatter({ formatType: sDateOrder, prefix: "", postFix: "", yearFormat: sYearFormat, firstSep: sFirstSep,
				monthFormat: sMonthFormat, secSep: sSecSep, dayFormat: sDayFormat
			});
			//    //        currentExtItem.formatterAdapter = new TextBoxFormatterExtender(currentExtItem, oFormatter);
			// signMode, formatType, sThousandSeparator, cDecimalSeparator
			var oDateAdapter = new DateFieldFormatterExtender(extItem.getComponent(0), oFormatter, new DateValidator({ firstSep: sFirstSep, sSecSep: sSecSep }));

			extItem.dateFormatter = dateFormatter;

			// this case does not return anything, it works but it is
			// not coherent with the other ones.
		}
	}
}


////////////////////////////////////////////////////////////////////////
/// Adapter class for making a formatter and an input validator
/// available to a textbox. Note that, at the moment, the validator 
/// is called only if a formatter has been set. This may change in the
/// future.
////////////////////////////////////////////////////////////////////////
function TextBoxFormatterExtender(control, formatter, inputValidator) {

	this.oFormatter = formatter;
	this.oInputValidator = inputValidator;

	this.onKeyPress = function (object, e, eOpts) {


		if (this.oFormatter) {
			var cTyped = String.fromCharCode(e.charCode);
			var bValid = true;

			//            if (e.keyCode == 8 || e.keyCode == 46) {
			//                console.log('pressed char code is ' + e.keyCode);
			//            }
			if (e.altKey || e.ctrlKey || e.charCode < 28) {
				// alt/ctrl/arrows case
				// do nothing.
				return true;
			}
			if (cTyped.match(/SPECIAL/g)) {
				return true;
			}

			if (this.oInputValidator) {
				// validate user input.
				bValid = this.oInputValidator.validate(cTyped);
			}
			if (bValid) {
				// get current textbox content.
				var sOldValue = object.getValue();
				var unformattedLength = 0;
				// get selection beginning and end.
				var iSelectionStart = object.inputEl.dom.selectionStart;
				var iSelectionEnd = object.inputEl.dom.selectionEnd;
				// handle the input, it may cause a sign toggle. It is 
				// handled by the formatter as it knows how to handle sign changes
				var sNewContent = this.oFormatter.handleInput(sOldValue, cTyped, iSelectionStart, iSelectionEnd);
				object.setValue(sNewContent);
				// new cursor position.
				if (sOldValue.length == 0) {
					object.inputEl.dom.selectionEnd = 1;
				} else {
					unformattedLength = iSelectionStart + cTyped.length + (sOldValue.length - iSelectionEnd);
					object.inputEl.dom.selectionEnd = iSelectionStart + 1 + (sNewContent.length - unformattedLength);
				}
			}
		}

		e.preventDefault();
		return false;
	}

	this.onBlur = function (object, event, opts) {
		if (this.oFormatter) {
			// TODO: add isDirty() mechanism to avoid validate
			// a not modified string.

			var sInput = object.getValue();
			if (sInput != "") {
				var bValid = true;
				if (this.oInputValidator) {
					// we have a validator
					if (this.oInputValidator.validate(sInput)) {

					} else {

						bValid = false;
					}
				}
				if (bValid) {
					// succesfully validated, format it and set it to 
					// the textbox content. Formatting may be needed as 
					// the textbox content may come from a cut and paste.                    
					if (this.oFormatter.addMissingChars) {
						sInput = this.oFormatter.addMissingChars(sInput);
					}
					var sFormatted = this.oFormatter.handleInput(sInput);

					object.setValue(sFormatted);
				} else {
					// set a default value, if any.
					if (this.oFormatter.sNullValue) {
						object.setValue(this.oFormatter.sNullValue);
					} else {
						object.setValue("");
					}
				}
			}
			event.preventDefault();
			return false;
		}
	}

	this.onFocus = function (object, e, eOpts) {
		var sValue = object.getValue();
		if (this.oFormatter) {
			if (this.oFormatter.sNullValue && sValue == this.oFormatter.sNullValue) {
				// Se il contenuto è il valore nullo, svuota il contenuto 
				// della textbox per permettere all'utente l'inserimento pulito.
				object.setValue("");
				e.preventDefault();
				return false;
			}

			var sInput = this.oFormatter.removePrologueEpilogue(object.getValue());
			object.setValue(sInput);
			if (e.preventDefault) {
				e.preventDefault();
			}
			return false;
		}
	}

	// register event handlers for the control.
	control.on(
            {
            	keypress: this.onKeyPress,
            	blur: this.onBlur,
            	focus: this.onFocus,
            	scope: this

            });
}


////////////////////////////////////////////////////////////////////////
/// String Formatter class.
/// formatType: ASIS|UPPERCASE|LOWERCASE|CAPITALIZED|MASKED|EXPANDED
/// TODO: a constructor with configuration info.
////////////////////////////////////////////////////////////////////////
function StringFormatter(oConfig) {
	if (oConfig) {
		// formatter type
		this.formatType = oConfig.formatType;
		// mask, it may be moved to the input validator.
		this.mask = oConfig.mask;
		// prologue to be applied to the input string.
		this.prologue = oConfig.prologue;
		// epilogue to be applied to the input string.
		this.epilogue = oConfig.epilogue;
		// string to be added in between input chars.
		this.sInterChars = oConfig.sInterChars;
		// Field length.
		this.width = oConfig.iWidth;
	}
}

StringFormatter.prototype.format = function (sValue, bPaddingEnabled) {
	var sResult = "";
	if (sValue) {
		switch (this.formatType) {

			case StringAsIs:
				sResult = sValue;
				break;

			case StringUpperCase:
				// if locale upper use toLocaleUpperCase()
				sResult = sValue.toUpperCase();
				break;

			case StringLowerCase:
				// if locale upper use toLocaleUpperCase()
				sResult = sValue.toLowerCase();
				break;

			case StringCapitalized:
				// if locale upper use toLocaleUpperCase()
				sResult = this.capitalizeString(sValue);
				break;

			case StringMasked:
				// TODO
				break;

			case StringExpanded:
				var iLength = sValue.length;
				for (var iCount = 0; iCount < iLength; iCount++) {
					sResult += sValue[iCount];
					if (iCount < (iLength - 1)) {
						sResult += this.sInterChars;
					}
				}
				break;

		}
              
		// prologue/epilogue handling.
		if (bPaddingEnabled) {
			sResult = this.prologue + sResult;
			sResult += this.epilogue;
		}
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Encloses the input string "sValue" between prologue and epilogue.
////////////////////////////////////////////////////////////////////////
StringFormatter.prototype.encloseString = function (sValue) {
	var sResult = this.prologue;
	sResult += sValue;
	sResult += this.epilogue;
	return sResult;
}

StringFormatter.prototype.capitalizeString = function (sValue) {
	var sResult = "";
	if (sValue) {
		var iFirstLetter = 0;
		var bCapitalizeNext = true;
		for (var iCount = 0; iCount < sValue.length; iCount++) {
			if (sValue.charAt(iCount).match(/[\d\W\s_]/g)) {
				bCapitalizeNext = true;
				sResult += sValue.charAt(iCount);
			}
			else if (bCapitalizeNext == true) {
				sResult += sValue.charAt(iCount).toUpperCase();
				bCapitalizeNext = false;
			} else {
				sResult += sValue.charAt(iCount);
			}
		}
	}
	return sResult;
}

StringFormatter.prototype.removePrologueEpilogue = function (sValue) {
	// check prologue and epilogue are in the string,
	// if not do nothing.
	var sInput = "";
	if (sValue) {
		// remove the prologue, if any. replace() replaces
		// the first occurrence of the given string, so 
		// it actually removes just the prologue and not further 
		// occurrences of it.
		// remove the epilogue:
		if (this.prologue) {
			sInput = sValue.replace(this.prologue, "");
		} else {
			sInput = sValue;
		}
		var sValueHead = sInput;
		var sValueTail = "";
		if (this.epilogue) {
			sValueHead = sInput.slice(0, sInput.length - this.epilogue.length);
			// get the substring which we expect to be the epilogue.
			sValueTail = sInput.slice(sValue.length - this.epilogue.length, sValue.length);
			// replace the epilogue from it.
			sValueTail = sValueTail.replace(this.epilogue, "");
		}
		// add the result to the output string.
		sInput = sValueHead + sValueTail;
	}
	// TODO: if sValue is null it may be worth to
	// return the null value from the formatter instead
	// of an empty string.
	return sInput;
}

StringFormatter.prototype.unformat = function (sValue) {

	var sResult = sValue;
	if (sResult == undefined) {
		sResult = "";
	}
	if (this.formatType == "EXPANDED") {
		// remove extra chars in the string.
		sResult = "";
		// replace interchars 
		var sSeparator = addEscapeForRegex(this.sInterChars);
		var oInterCharRegex = new RegExp(sSeparator, "g");
		sResult = sValue.replace(oInterCharRegex, "");
	}
	// do nothing,
	// TODO: check if a mask affects string formatting.
	return sResult;
}

StringFormatter.prototype.handleInput = function (sOldValue, cTyped, iSelectionStart, iSelectionEnd) {

	sOldValue = this.unformat(sOldValue);
	var sResult = "";

	if (cTyped && iSelectionStart >= 0 && iSelectionEnd >= 0) {

		// get the portion of existing string before selection (head).
		var sNewBegin = sOldValue.slice(0, iSelectionStart);

		// get the portion of existing string after selection (tail).
		var sNewEnd = sOldValue.slice(iSelectionEnd, sOldValue.length);

		// insert new string in betweeen head and tail.
		var sNewValue = sNewBegin + cTyped + sNewEnd;
		sResult = sNewValue;

	} else {
		// nothing to be changed, string to be added does not
		// represent a valid input, skip it.
		sResult = sOldValue;
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// integer formatter constructor.
///
/// formatType: NUMERIC|ZERO_AS_DASH|LETTER|ENCODED
///              
/// signMode: MINUSPREFIX|SIGNPREFIX|ROUNDS|SIGNPOSTFIX|MINUSPOSTFIX
///           
////////////////////////////////////////////////////////////////////////
function IntegerFormatter(oConfig) {
	if (oConfig) {
		this.iWidth = oConfig.width;
		this.cThousandSeparator = oConfig.thousandSeparator;
		this.sNullValue = oConfig.nullValue;
		this.signMode = oConfig.signMode;
		this.formatType = oConfig.formatType;
		this.mask = oConfig.mask;
		this.prologue = oConfig.prologue;
		this.epilogue = oConfig.epilogue;
	}
}

// inherits "StringFormatter" methods.
IntegerFormatter.prototype = Object.create(StringFormatter.prototype);

////////////////////////////////////////////////////////////////////////
/// Formats the given string "sValue" and char "cCurrChar" as a whole integer. 
/// "sValue": a string representing an integer.
/// "cCurrChar": the last inserted char (to be appended to the given string?). 
///              If it represents a number (digit) or a sign it will replace 
///              the selected string of "sValue, i.e. the substring of 
///              "sValue[iSelectionStart:iSelectionEnd]".
/// iSelectionStart: start index (0 based) of the selection on "sValue".
/// iSelectionEnd: end index (0 based) of the selection on "sValue".
////////////////////////////////////////////////////////////////////////
IntegerFormatter.prototype.format = function (sValue, bPaddingEnabled) {

	var sResult = sValue;

	if (this.formatType == IntegerNumeric || this.formatType == RealFixed) {
		// format only if the type is numeric.
		sResult = sResult.replace(/\b0+/g, "");
		// thousand separator should already have been removed, 

		var dot = "[" + this.cThousandSeparator + "]";
		var dotRegex = new RegExp(dot, "g");
		var sNewResult = sResult.replace(dotRegex, "");
		sResult = this.insertThousandSeparator(sNewResult, this.cThousandSeparator);
	}
	// prologue/epilogue handling.
	if (bPaddingEnabled) {
		sResult = this.prologue + sResult;
		sResult += this.epilogue;
	}
	return sResult;
}

IntegerFormatter.prototype.unformat = function (sValue) {

	var sResult = sValue;

	// remove extra chars in the string.
	sResult = "";
	// replace interchars 
	var sSeparator = addEscapeForRegex(this.cThousandSeparator);
	var oInterCharRegex = new RegExp(sSeparator, "g");
	sResult = sValue.replace(oInterCharRegex, "");

	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Adds a thusand separator every 3 chars, no matter whether chars are
/// digits or not, so use with caution.
////////////////////////////////////////////////////////////////////////
IntegerFormatter.prototype.insertThousandSeparator = function (sInput, sThousandSeparator) {

	// 2) apply thousands.
	if (sThousandSeparator == null) {
		return sInput;
	}
	var sResult = "";
	if (sInput != null) {

		var iLength = sInput.length;
		for (var iCount = 0; iCount < iLength; iCount++) {
			if (iCount > 0 && ((iLength - iCount) % 3) == 0) {
				sResult += sThousandSeparator;
			}
			sResult += sInput[iCount];
		}

		return sResult;
	}
}

////////////////////////////////////////////////////////////////////////
/// Replaces the portion of string sOldValue between iSelectionStart and
/// iSelectionEnd with cTyped. If CTyped is a minus the sign of sOldValue 
/// is toggled.
////////////////////////////////////////////////////////////////////////
IntegerFormatter.prototype.handleInput = function (sOldValue, cTyped, iSelectionStart, iSelectionEnd) {

	var bIsNegative = this.isNegative(sOldValue);
	var sResult = "";

	if (cTyped && iSelectionStart != 'undefined' && iSelectionEnd != 'undefined') {

		if (cTyped.match(/-/)) {
			// the input is the minus key                       
			// toggle the minus status
			bIsNegative = !bIsNegative;
			sResult = this.removeSign(sOldValue);
			// sResult = this.toggleMinus(sOldValue);
		} else {
			var bHasSign = 0;
			if ((bIsNegative && ((this.signMode == SignMinusPrefix) || (this.signMode == SignRounds))) || (this.signMode == SignSignPrefix)) {
				bHasSign = 1;
			}
			// remove the sign from the string
			var sAbs = this.removeSign(sOldValue);
			// extract string head and tail, which are preserved by input
			// sanitize index >= 0
			var sHead = sAbs.slice(0, iSelectionStart - bHasSign >= 0 ? iSelectionStart - bHasSign : 0);
			var sTail = sAbs.slice(iSelectionEnd - bHasSign, sOldValue.length);
			// unformat head and tail
			var sUnformatHead = this.unformat(sHead);
			var sUnformatTail = this.unformat(sTail);

			// insert new string in betweeen head and tail.
			var sNewValue = sUnformatHead + cTyped + sUnformatTail;

			sResult = sNewValue;
		}

	} else {
		// nothing to be changed, string to be added 
		// does not represent an integer, skip it.
		sResult = this.removeSign(sOldValue);
	}
	// apply thousand separators
	sResult = this.format(sResult);
	sResult = this.addSign(sResult, !bIsNegative)
	return sResult;
}

IntegerFormatter.prototype.toggleMinus = function (sOldValue) {

	var sResult = sOldValue;
	if (sOldValue) {
		// remove the sign simbols.
		sResult = this.removeSign(sResult);
		// set as sign the opposite of the previous one.
		sResult = this.addSign(sResult, this.isNegative(sOldValue));
	}
	return sResult;
}


IntegerFormatter.prototype.isNegative = function (sOldValue) {
	var bResult = false;
	if (sOldValue) {
		// valid input.
		switch (this.signMode) {
			case SignMinusPrefix:
			case SignSignPrefix:
				// look for minus sign at the string beginning.
				return (sOldValue[0] == "-");

			case SignMinusPostfix:
			case SignSignPostfix:
				// look for minus sign at the string end.
				return (sOldValue[sOldValue.length - 1] == "-");

			case SignRounds:
				// look for opening and closing brackets around the value.
				return ((sOldValue[0] == "(") && (sOldValue[sOldValue.length - 1] == ")"));
		}     
	}
	return bResult;
}

IntegerFormatter.prototype.hasSign = function (sOldValue) {
	var bResult = false;
	if (sOldValue) {
		// valid input.
		switch (this.signMode) {
			case SignMinusPrefix:
			case SignSignPrefix:
				// look for the sign at the string beginning.
				return (sOldValue[0] == "-" || sOldValue[0] == "+");

			case SignMinusPostfix:
			case SignSignPostfix:
				// look for the sign at the string end.
				return (sOldValue[sOldValue.length - 1] == "-" || sOldValue[sOldValue.length - 1] == "+");

			case SignRounds:
				// look for opening and closing brackets around the value.
				return ((sOldValue[0] == "(") && (sOldValue[sOldValue.length - 1] == ")"));
		}         
	}
	return bResult;
}

////////////////////////////////////////////////////////////////////////
/// Removes the proper sign representation to the number string.
////////////////////////////////////////////////////////////////////////
IntegerFormatter.prototype.removeSign = function (sOldValue) {
	var sResult = sOldValue;
	var bHasSign = this.hasSign(sOldValue);
	if (bHasSign) {
		if (sResult) {
			switch (this.signMode) {
				case SignMinusPrefix:
				case SignSignPrefix:
					// remove first char.
					sResult = sResult.slice(1, sOldValue.length);
					break;
				case SignRounds:
					// remove first and last char.
					sResult = sResult.slice(1, sResult.length - 1);
					break;
				case SignMinusPostfix:
				case SignSignPostfix:
					// remove last char.
					sResult = sResult.slice(0, sResult.length - 1);
					break;
			}
		}
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Adds the proper sign representation to the number string.
////////////////////////////////////////////////////////////////////////
IntegerFormatter.prototype.addSign = function (sOldValue, bPositive) {
	var sResult = sOldValue;
	if (sOldValue) {
		switch (this.signMode) {

			case SignMinusPrefix:
				if (!bPositive) {
					// make it negative.
					sResult = "-" + sResult;
				}
				break;

			case SignSignPrefix:
				if (!bPositive) {
					// make it negative.         
					sResult = "-" + sResult;
				} else {
					// make it positive.
					sResult = "+" + sResult;
				}
				break;

			case SignRounds:
				if (!bPositive) {
					// make it negative.
					sResult = "(" + sResult + ")";
				}
				break;

			case SignMinusPostfix:
				if (!bPositive) {
					// make it negative.
					sResult = sResult + "-";
				}
				break;

			case SignSignPostfix:
				if (!bPositive) {
					// make it negative.
					sResult = sResult + "-";
				} else {
					// make it positive.
					sResult = sResult + "+";
				}
		}
	}
	return sResult;
}


////////////////////////////////////////////////////////////////////////
/// real formatter constructor.
///
/// formatType: FIXED|ZERO_AS_DASH|LETTER|ENCODED|EXPONENTIAL|ENGINEER
///              
/// signMode: MINUSPREFIX|SIGNPREFIX|ROUNDS|SIGNPOSTFIX|MINUSPOSTFIX
///           
/// iDecimals: how many decimal digits are allowed for real numbers.
///
/// sRoundMode: how to round real numbers.
///
/// sRoundQuantum: Rounding quantum.
////////////////////////////////////////////////////////////////////////
function RealFormatter(oConfig) {
	if (oConfig) {
		this.iWidth = oConfig.width;
		this.cThousandSeparator = oConfig.thousandSeparator;
		this.cDecimalSeparator = oConfig.decimalSeparator;
		this.sNullValue = oConfig.nullValue;
		this.signMode = oConfig.signMode;
		this.formatType = oConfig.formatType;
		this.mask = oConfig.mask;
		this.prologue = oConfig.prologue;
		this.epilogue = oConfig.epilogue;
		this.decimals = oConfig.decimals;
		this.showFinalZeros = oConfig.showZeros;
		this.roundMode = oConfig.roundMode;
		this.roundQuantum = new Number(oConfig.roundQuantum);
	}
}

// inherits "IntegerFormatter" methods.
RealFormatter.prototype = Object.create(IntegerFormatter.prototype);
RealFormatter.prototype.integerHandleInput = RealFormatter.prototype.handleInput;
RealFormatter.prototype.integerFormat = RealFormatter.prototype.format;
RealFormatter.prototype.integerUnformat = RealFormatter.prototype.unformat;


////////////////////////////////////////////////////////////////////////
/// Replaces the portion of string sOldValue between iSelectionStart and
/// iSelectionEnd with cTyped. If CTyped is a minus the sign of sOldValue 
/// is toggled.
////////////////////////////////////////////////////////////////////////
RealFormatter.prototype.handleInput = function (sOldValue, sInput, iSelectionStart, iSelectionEnd) {

	// result value.
	var sResult = "";
	var bIsNegative = this.isNegative(sOldValue);
	var bHasSign = 0;
	// decimals are allowed, format them as well.
	if (sInput && iSelectionStart != 'undefined' && iSelectionEnd != 'undefined') {

		if (this.decimals <= 0) {
			// if no decimals are allowed it behaves as an integer formatter.
			return this.integerHandleInput(sOldValue, sInput, iSelectionStart, iSelectionEnd);
		}
		// TODO: merge these two cases.
		if (sInput.match(/-/)) {
			// the input is the minus key  
			return this.integerHandleInput(sOldValue, sInput, iSelectionStart, iSelectionEnd);
		}
		// "real number" formatting stuff.

		if ((bIsNegative && ((this.signMode == SignMinusPrefix) || (this.signMode == SignRounds))) || (this.signMode == SignSignPrefix)) {
			bHasSign = 1;
		}

		var sAbs = this.removeSign(sOldValue);
		if (sInput.match(addEscapeForRegex(this.cDecimalSeparator))) {
			// handle decimal separator
			// if a decSep is already in the string, move it,
			// i.e. remove the old and place a new in the curr position
			// remove previuous decSep
			// iSelectionStart, iSelectionEnd, this.decimals
			if (sOldValue.length - (iSelectionEnd - iSelectionStart) - iSelectionStart > this.decimals) {
				// new decimal separator position 
				// textbox value should not be modified.
				console.log("decimal bound not satisfied");
				return sOldValue;
			} else {
				console.log("decimal bound satisfied");
				sAbs = sAbs.replace(this.cDecimalSeparator, "");
			}
		}
		var sHead = sAbs.slice(0, iSelectionStart - bHasSign >= 0 ? iSelectionStart - bHasSign : 0);
		var sTail = sAbs.slice(iSelectionEnd - bHasSign, sOldValue.length);
		var iDecimalsCount = sTail.length;
		// get the first position of not zero char.
		while (iDecimalsCount - 1 >= 0) {
			if (sTail[iDecimalsCount - 1] != "0") {
				break;
			}
			iDecimalsCount--;
		}
		sTail = sTail.slice(0, iDecimalsCount + 1);
		// decimal length check.
		if (sHead.length > 0) {
			var iDecSepPos = sHead.indexOf(this.cDecimalSeparator);
			if (iDecSepPos != -1) {
				// number head contains
				if (sTail.length + sInput.length + (sHead.length - iDecSepPos - 1) > this.decimals) {
					// can not add input as the resulting string decimal would overflow.
					sInput = "";
				}
			}
		}
		// unformat head and tail
		var sUnformatHead = this.unformat(sHead);
		var sUnformatTail = this.unformat(sTail);

		// insert new string in betweeen head and tail.
		sResult = sUnformatHead + sInput + sUnformatTail;

	} else {
		// nothing to be changed, string to be added 
		// does not represent an integer, skip it.
		sResult = this.removeSign(sOldValue);
		// TODO: round sResult if needed.
	}

	sResult = this.format(sResult);
	sResult = this.addSign(sResult, !bIsNegative)
	return sResult;
}

RealFormatter.prototype.format = function (sValue, bPaddingEnabled) {
	var sResult = "";
	if (sValue) {
		sValue = this.round(sValue);
		// split on decimal separator.
		var iDecSepIndex = sValue.indexOf(this.cDecimalSeparator);
		var sInteger = sValue;
		var sReminder = "";
		if (iDecSepIndex > -1) {
			sInteger = sValue.slice(0, iDecSepIndex);
			sReminder = sValue.slice(iDecSepIndex, sValue.length);
		}
		sResult = this.integerFormat(sInteger, false);
		sResult += sReminder;
	}

	// prologue/epilogue handling.
	if (bPaddingEnabled) {
		sResult = this.prologue + sResult;
		sResult += this.epilogue;
	}

	return sResult;
}

RealFormatter.prototype.addMissingChars = function (sInput) {
	var sResult = sInput;
	if (this.showFinalZeros && this.decimals > 0) {
		var bIsNegative = this.isNegative(sResult);
		sResult = this.removeSign(sResult);
		var sDecimals = "";
		var asItems = sResult.split(this.cDecimalSeparator);
		if (asItems.length == 2) {
			// we have decimals.
			sDecimals = asItems[1];

		}
		// add as many zeros as needed to have the required decimals
		for (var iDecimalsCount = sDecimals.length; iDecimalsCount < this.decimals; iDecimalsCount++) {
			sDecimals += "0";
		}
		sResult = asItems[0] + this.cDecimalSeparator + sDecimals;
		sResult = this.addSign(sResult, !bIsNegative)

	}
	return sResult;
}
////////////////////////////////////////////////////////////////////////
/// Rounds the number represented by the input string according to 
/// round settings from the formatter. 
/// sValue: The string representation of the number to be rounded. It is 
///         assumed to not contain extra chars (e.g. thousand separator) 
///         coming from formatting.
/// Returns: The rounded value.
////////////////////////////////////////////////////////////////////////
RealFormatter.prototype.round = function (sValue) {
	var sResult = sValue;
	if (sValue) {
		// replace the custom decimal and thousand separators
		// with the proper one for Javascript.
		sValue = sValue.replace(this.cThousandSeparator, "");
		sValue = sValue.replace(this.cDecimalSeparator, ".");
		sResult = sValue;
		if (!isNaN(sValue)) {
			// sValue represents a valid number.
			if (this.roundMode && this.roundQuantum) {
				// a round mode has been set.
				// check quantum quantity
				if (this.roundQuantum.valueOf() > 0) {
					// It should never be negative, as 
					// UI does not allow user to set it so.

					// convert the string to a number, in order to 
					// be able to perform rounding steps.
					var dNumeric = new Number(sValue);

					// check current round mode.
					switch (this.roundMode) {

						case RoundAbsolute:
							sResult = this.roundSigned(Math.abs(dNumeric));
							break;

						case RoundSigned:
							sResult = this.roundSigned(dNumeric);
							break;

						case RoundZero:
							sResult = this.roundZero(dNumeric);
							break;

						case RoundInfinite:
							sResult = this.roundInfinite(dNumeric);
							break;

					}
				}
			}
		}
	}
	sResult = sResult.toString().replace(".", this.cDecimalSeparator);
	return sResult;
}


RealFormatter.prototype.roundSigned = function (dValue) {
	// straightforward porting of TB round code.
	if (this.roundQuantum == 0.0)
		return dValue;
	var nQuantum = this.roundQuantum;
	var e = 1.;


	while (Math.floor(nQuantum) < nQuantum) {
		nQuantum *= 10.; dValue *= 10.; e *= 10.;
	}

	var r = dValue % nQuantum;
	dValue -= r;
	// TODO: get DataDbl::GetEpsilon()
	//    var p = nQuantum / 2.;

	// Codice Originale Germano.  >if (r >= p)<
	// Deve essere eliminato il rumore decimale 
	// che rimane in r. (ad es. nValue=497.105 
	// e qantum=0.01). Bruna
	//	if ((p - r) <= DataDbl::GetEpsilon())
	//		nValue += nQuantum;

	return dValue / e;

}

RealFormatter.prototype.roundZero = function (dValue) {
	// straightforward porting of TB round code.

	if (this.roundQuantum == 0.0)
		return dValue;

	var sign = (nValue >= 0.) ? 1 : -1;
	nValue = fabs(nValue);

	var nQuantum = quantum;
	var e = 1.;

	while (floor(nQuantum) < nQuantum) {
		nQuantum *= 10.; nValue *= 10.; e *= 10.;
	}

	return sign * nQuantum * Math.floor(nValue / nQuantum) / e;
}

RealFormatter.prototype.roundInfinite = function (dValue) {
	// straightforward porting of TB round code.

	if (this.roundQuantum == 0.0)
		return dValue;


	var sign = (dValue >= 0.) ? 1 : -1;
	nValue = Math.abs(dValue);

	var nQuantum = this.roundQuantum;
	var e = 1.;

	while (Math.floor(nQuantum) < nQuantum) {
		nQuantum *= 10.; nValue *= 10.; e *= 10.;
	}

	// Prevenzione rumore (anomalia 5646)
	// Se nValue ha del rumore lo pulisco 6404.000000000001 diventa 6404
	// se nValue ha dei decimali validi   6404.300000000001 non faccio nulla, ma la ceil lo
	// arotonda all'intero superiore e diventa correttamente 6405 (nQuantum e' diventato intero)
	//	if ((nValue - floor(nValue)) <= DataDbl::GetEpsilon())
	//		nValue = floor(nValue);

	return sign * nQuantum * ceil(nValue / nQuantum) / e;
}

function PercentageFormatter(oConfig) {
	if (oConfig) {
		this.iWidth = oConfig.width;
		this.cThousandSeparator = oConfig.thousandSeparator;
		this.cDecimalSeparator = oConfig.decimalSeparator;
		this.sNullValue = oConfig.nullValue;
		this.signMode = oConfig.signMode;
		this.formatType = oConfig.formatType;
		this.mask = oConfig.mask;
		this.prologue = oConfig.prologue;
		this.epilogue = oConfig.epilogue;
		this.decimals = oConfig.decimals;
		this.showFinalZeros = oConfig.showZeros;
		this.roundMode = oConfig.roundMode;
		this.roundQuantum = new Number(oConfig.roundQuantum);
	}
}

PercentageFormatter.prototype = Object.create(RealFormatter.prototype);
PercentageFormatter.prototype.realHandleInput = PercentageFormatter.prototype.handleInput;

////////////////////////////////////////////////////////////////////////
/// Replaces the portion of string sOldValue between iSelectionStart and
/// iSelectionEnd with cTyped. If CTyped is a minus the sign of sOldValue 
/// is toggled.
////////////////////////////////////////////////////////////////////////
PercentageFormatter.prototype.handleInput = function (sOldValue, sInput, iSelectionStart, iSelectionEnd) {

	var sNewValue = sOldValue;

	if (sInput) {
		if (sInput.match(/-/)) {
			// the input is the minus key, 
			// negative percentage values are not allowed
			// deduced from working experience on TB.
			return sOldValue;
		}
		sNewValue = this.realHandleInput(sOldValue, sInput, iSelectionStart, iSelectionEnd);
		var asNew = sNewValue.split(this.cDecimalSeparator);
		asNew = asNew[0].replace(this.cThousandSeparator, "");
		var iNew = parseInt(asNew);
		if (iNew > 100) {
			sNewValue = sOldValue;
		}
	}
	return sNewValue;
}

function ElapsedFormatter(oConfig) {
	if (oConfig) {
		this.formatType = oConfig.formatType;
		this.timeSeparator = oConfig.timeSeparator;
		this.decSeparator = oConfig.decSeparator;
		this.decNumber = parseInt(oConfig.decNumber);
	}
}



ElapsedFormatter.prototype = Object.create(StringFormatter.prototype);

////////////////////////////////////////////////////////////////////////
/// Extracts the last number in the string (numbers are separated by 
/// this.timeSeparator) and set to iMaxValue if it is greater than that.
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.floorValue = function (sValue, iMaxValue) {
	if (iMaxValue < 0) {
		// negative max value => no upper limit.
		return sValue;
	}
	var sResult = "";
	var iLastSep = sValue.lastIndexOf(this.timeSeparator);
	var sHead = "";
	var sTail = "";
	if (iLastSep > 0) {
		sHead = sValue.slice(0, iLastSep + 1);
		sTail = sValue.slice(iLastSep + 1);
	} else {
		sTail = sValue;
	}
	var iCurrValue = parseInt(sTail);
	if (iCurrValue > iMaxValue) {
		iCurrValue = iMaxValue;
		// need to modify last field of result.
		sResult = sHead + iCurrValue;
	} else {

		sResult = sHead + sTail;
	}

	return sResult;
}



////////////////////////////////////////////////////////////////////////
/// 
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.format = function (sValue, bPaddingEnabled) {

	var sResult = "";
	var iInputLen = sValue.length;
	if (sValue) {
		var oInteger = 0;
		var asSplittedValue = "";
		var sInputInteger = "";
		switch (this.formatType) {

			case TIME_D:
			case TIME_H:
			case TIME_M:
			case TIME_S:
				oInteger = this.handleSingleValueBased(sValue);
				sResult = oInteger.integer;
				break;

			case TIME_DCD:
			case TIME_HCH:
			case TIME_MCM:
				asSplittedValue = sValue.split(this.decSeparator);
				sInputInteger = asSplittedValue[0];
				// process the integer part of input.
				oInteger = this.handleSingleValueBased(sInputInteger);
				sResult = this.addDecimals(oInteger, asSplittedValue);
				break;

			case TIME_DHMS:
			case TIME_DHM:
			case TIME_DH:
				oInteger = this.handleIntegerDayBased(this.formatType, sValue);
				sResult = oInteger.integer;
				break;

			case TIME_HMS:
			case TIME_HM:
			case TIME_MSEC:
				sResult = this.handleIntegerHourBased(this.formatType, sValue);
				break;

			case TIME_DHCH:
			case TIME_DHMCM:
				asSplittedValue = sValue.split(this.decSeparator);
				sInputInteger = asSplittedValue[0];
				// process the integer part of input.
				var sCurrFormat = TIME_DHCH;
				if (this.formatType == TIME_DHMCM) {
					sCurrFormat = TIME_DHMCM;
				}
				oInteger = this.handleIntegerDayBased(sCurrFormat, sInputInteger);
				sResult = this.addDecimals(oInteger, asSplittedValue);
				break;

			case TIME_HMCM:
				asSplittedValue = sValue.split(this.decSeparator);
				sInputInteger = asSplittedValue[0];
				// process the integer part of input.
				oInteger = this.handleIntegerHourBased(TIME_HM, sInputInteger);
				sResult = this.addDecimals(oInteger, asSplittedValue);
				break;
		}
	}
	return sResult;
}

ElapsedFormatter.prototype.addDecimals = function (oInteger, asSplittedValue) {
	var sResult = "";
	var sInputDecimal = null;
	if (asSplittedValue.length > 1) {
		// there is a decimal part form the input.
		sInputDecimal = asSplittedValue[1];
		if (!sInputDecimal) {
			// add default decimals
			sInputDecimal = "00";
		}
	}
	var sDecimal = null;
	if (oInteger.reminder) {
		if (sInputDecimal) {
			sDecimal = oInteger.reminder + sInputDecimal;
		} else {
			sDecimal = oInteger.reminder;
		}
	} else {
		sDecimal = sInputDecimal;
	}
	if (sDecimal) {
		// time separator not allowed in decimal portion.
		sDecimal = sDecimal.replace(this.timeSeparator, "");
		sDecimal = sDecimal.slice(0, this.decNumber);
		sResult = oInteger.integer + this.decSeparator + sDecimal;
	} else {
		sResult = oInteger.integer;
	}
	return sResult;
}
////////////////////////////////////////////////////////////////////////
/// Handles the integer time part for formats 
/// TIME_D || TIME_H || TIME_M || TIME_S. Other formats need a call
/// to another method.
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.handleSingleValueBased = function (sValue) {
	var iDigitsLimit = 5;
	// above this day limit it may overflow on TB server.
	var iMaxValue = 24855;
	var sInteger = sValue;
	var sReminder = "";
	var oResult = {};

	switch (this.formatType) {
		case TIME_H:
			iDigitsLimit = 6;
			iMaxValue = 596523;
			break;

		case TIME_M:
			iDigitsLimit = 8;
			iMaxValue = 35791394;
			break;

		case TIME_S:
			iDigitsLimit = 10;
			iMaxValue = 2147483647;
			break;
	}
   
	if (sValue.length > iDigitsLimit) {
		sInteger = sInteger.slice(0, iDigitsLimit);
		oResult.reminder = sValue.slice(iDigitsLimit, sValue.length);
	}
	oResult.integer = this.floorValue(sInteger, iMaxValue);
	// TODO: evaluate the reminder, i.e. possibile decimal digits.    
	return oResult;
}

////////////////////////////////////////////////////////////////////////
/// Handles the integer time part for formats 
/// TIME_DHMS || TIME_DHM || TIME_DH. Other formats need a call
/// to another method.
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.handleIntegerDayBased = function (formatType, sValue) {
	// scan input string and look for
	// positions where to add time separators.
	var iOutputPos = 0;
	//    var sInteger = sValue;
	var sReminder = "";

	// limit first field range to avoid overflow.
	iDigitsLimit = 5;
	iDigitCount = 0;
	iCurrField = 0;
	iMaxValue = 24000;
	iMaxFields = 4;
	switch (formatType) {

		case TIME_DHM:
		case TIME_DHMCM:
			iMaxFields = 3;
			iMaxValue = 24000;
			break;

		case TIME_DH:
			iMaxFields = 3;
			iMaxValue = 24000;
			break;

	}
   
	var oResult = { integer: "" };
	var iInputLen = sValue.length;
	// look for day digits.
	for (var iInputPos = 0; iInputPos < iInputLen; iInputPos++) {

		if ((iDigitCount >= iDigitsLimit) && (sValue.charAt(iInputPos) != this.timeSeparator)) {
			// field change because of digits limit reached.
			iDigitsLimit = 2;
			iDigitCount = 0;

			// field change.
			// verify the current field is within its validity range.
			// max value for days, it has to do with an arithmetic 
			// overflow on TB.                    

			oResult.integer = this.floorValue(oResult.integer, iMaxValue);
			// need to add a separator.
			oResult.integer += this.timeSeparator;
			iCurrField++;
			if (iCurrField == 1) {
				// day field.
				iMaxValue = 23;
			} else if ((iCurrField == 2) || (iCurrField == 3)) {
				iMaxValue = 59;
			}
		}

		if (sValue.charAt(iInputPos) == this.timeSeparator) {
			if ((iDigitCount != iDigitsLimit) && (iCurrField > 0)) {
				// the current field is not the first one, it must be completed 
				// with the missing digits, so ignore the time separator.
				iDigitCount--;
				continue;
			}
			oResult.integer = this.floorValue(oResult.integer, iMaxValue);

			iDigitsLimit = 2;
			iDigitCount = 0;
			// field change.
			// verify the current field is within its validity range.
			iCurrField++;
			if (iCurrField == 1) {
				// day field.
				iMaxValue = 23;
			} else if ((iCurrField == 2) || (iCurrField == 3)) {
				iMaxValue = 59;
			}
			oResult.integer += sValue.charAt(iInputPos);
			continue;
		}
		if (iCurrField > iMaxFields - 1) {
			// remove last separator
			oResult.integer = oResult.integer.slice(0, -1);
			// get the reminder and exit from the loop.
			oResult.reminder = sValue.slice(iInputPos, iInputLen);
			break;
		}
		iDigitCount++;
		oResult.integer += sValue.charAt(iInputPos);

		if (iInputPos == iInputLen - 1) {
			// last input char, last field may need to be 
			// floored.
			oResult.integer = this.floorValue(oResult.integer, iMaxValue);
		}
	}
	return oResult;
}

////////////////////////////////////////////////////////////////////////
/// Handles the integer time part for formats 
/// TIME_HMS || TIME_HM || TIME_MSEC. Other formats need a call
/// to another method.
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.handleIntegerHourBased = function (formatType, sValue) {
	// scan input string and look for
	// positions where to add time separators.
	var iOutputPos = 0;

	// limit first field range to avoid overflow.
	iDigitsLimit = 5;
	iDigitCount = 0;
	iCurrField = 0;
	iMaxValue = 100000;
	iMaxFields = 3;
	switch (formatType) {

		case TIME_HM:
			iMaxFields = 2;
			iMaxValue = 24000;
			break;

		case TIME_MSEC:
			iMaxFields = 2;
			iDigitsLimit = 6;
			iMaxValue = 500000;
			break;
	}
    
	var oResult = { integer: "" };
	var iInputLen = sValue.length;
	for (var iInputPos = 0; iInputPos < iInputLen; iInputPos++) {

		if ((iDigitCount >= iDigitsLimit) && (sValue.charAt(iInputPos) != this.timeSeparator)) {
			// field change due to digits limit reached.
			iDigitsLimit = 2;
			iDigitCount = 0;

			// verify the current field is within its validity range.
			// max value for days, it has to do with an arithmetic 
			// overflow on TB.                    

			oResult.integer = this.floorValue(oResult.integer, iMaxValue);
			// need to add a separator.
			oResult.integer += this.timeSeparator;
			iCurrField++;
			if (iCurrField != 0) {
				// minutes or seconds field.
				iMaxValue = 59;
			}
		}

		if (sValue.charAt(iInputPos) == this.timeSeparator) {

			if ((iDigitCount != iDigitsLimit) && (iCurrField > 0)) {
				// the current field is not the first one, it must be completed 
				// with the missing digits, so ignore the time separator.
				iDigitCount--;
				continue;
			}

			// field change due to a found time separator.
			// This is the transition from the first field 
			// to the next one.
			oResult.integer = this.floorValue(oResult.integer, iMaxValue);

			iDigitsLimit = 2;
			iDigitCount = 0;
			// verify the current field is within its validity range.
			iCurrField++;
			if (iCurrField != 0) {
				// minutes or seconds field.
				iMaxValue = 59;
			}
			oResult.integer += sValue.charAt(iInputPos);
			continue;
		}
		if (iCurrField > iMaxFields - 1) {
			// remove last separator as we are at the 
			// end of the elapsed time.
			oResult.integer = sResult.slice(0, -1);
			// get the reminder and exit from the loop.
			oResult.reminder = sValue.slice(iInputPos, iInputLen);
			break;
		}
		iDigitCount++;
		oResult.integer += sValue.charAt(iInputPos);

		if (iInputPos == iInputLen - 1) {
			// last input char, last field may need to be 
			// floored.
			oResult.integer = this.floorValue(oResult.integer, iMaxValue);
		}
	}
	return oResult;
}


ElapsedFormatter.prototype.unformat = function (sValue) {

	var sResult = sValue;

	return sResult;
}

ElapsedFormatter.prototype.handleInput = function (sOldValue, sInput, iSelectionStart, iSelectionEnd) {

	var sNewValue = sOldValue;
	if (sInput && iSelectionStart >= 0 && iSelectionEnd >= 0) {
		// get the portion of existing string before selection (head).
		var sNewBegin = sOldValue.slice(0, iSelectionStart);

		// get the portion of existing string after selection (tail).
		var sNewEnd = sOldValue.slice(iSelectionEnd, sOldValue.length);

		// insert new string in betweeen head and tail.
		sNewValue = sNewBegin + sInput + sNewEnd;
		sResult = sNewValue;

	} else {
		// nothing to be changed, string to be added does not
		// represent a valid input, skip it.
		sResult = sOldValue;
	}
	sNewValue = this.format(sNewValue);


	return sNewValue;
}

ElapsedFormatter.prototype.addMissingChars = function (sInput) {
	var sResult = "";
	switch (this.formatType) {
		case TIME_D:
		case TIME_H:
		case TIME_M:
		case TIME_S:
			// do nothing, no extra chars have to be added.
			sResult = sInput;
			break;
		default:
			// limit first field range to avoid overflow.
			var iDigitsLimit = 5;            
			var iDigitCount = 0;
			var iCurrField = 0;
			var iAddedChars = 0;
			var sCurrField = "";

			// add missing fields.
			var iFieldCount = this.getCurrFieldsCount();
			// elapsed time fields.
			var asFields = sInput.split(this.timeSeparator);
			var iCurrFieldsCount = asFields.length;

			var sHead = "";
			while (iCurrFieldsCount < iFieldCount) {
				// missing fields have to be added at the string head.
				sHead += "00" + this.timeSeparator;
				// sResult += this.timeSeparator + "00";
				iCurrFieldsCount++;
			}
			sInput = sHead + sInput;
			var iInputLen = sInput.length;

			if (this.formatType == TIME_HMS || this.formatType == TIME_HM || this.formatType == TIME_MSEC) {
				iDigitsLimit = 6;
			}

			// look for missing digits.
			for (var iInputPos = 0; iInputPos < iInputLen; iInputPos++) {

				if (sInput.charAt(iInputPos) == this.timeSeparator) {
					sCurrField = sResult.slice(iInputPos + iAddedChars - iDigitCount, iInputPos + iAddedChars);
					sResult = sResult.slice(0, iInputPos + iAddedChars - iDigitCount);
					while (iDigitCount < iDigitsLimit) {
						// add missing chars
						sResult += "0";
						iAddedChars++;
						iDigitCount++;
					}
					sResult += sCurrField;
					// add separator.
					sResult += this.timeSeparator;

					// field change
					iDigitsLimit = 2;
					iDigitCount = 0;
					iCurrField++;
					continue;
				}
				sResult += sInput.charAt(iInputPos);
				iDigitCount++;
			}
			// add missing chars to last field
			sCurrField = sInput.slice(iInputLen - iDigitCount, iInputLen);
			var sHead = sResult.slice(0, sResult.length - iDigitCount);
			while (iDigitCount < iDigitsLimit) {
				sHead += "0";
				iDigitCount++;
			}
			sResult = sHead + sCurrField;
			iCurrField++;
            
			//  add missing decimals.
			var bHasDecimals = this.hasDecimals();
			if (bHasDecimals) {
				// TODO: add decimals
				var asDecimalSplit = sResult.split(this.decSeparator);

				if (asDecimalSplit.length > 1) {
					// there already is a decimal separator
					var iDecLength = asDecimalSplit[1].length;
					var sZeros = "";
					while (iDecLength < this.decimals) {
						sZeros += "0";
						iDecLength++;
					}
					sResult += this.decSeparator + sZeros + asDecimalSplit[1];
				} else {
					sResult += this.decSeparator + "00";
				}
			}
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Retrieves how many fields the elapsed time is made of.
////////////////////////////////////////////////////////////////////////
ElapsedFormatter.prototype.getCurrFieldsCount = function () {
	switch (this.formatType) {

		case TIME_D:
		case TIME_H:
		case TIME_M:
		case TIME_S:
		case TIME_DCD:
		case TIME_HCH:
		case TIME_MCM:
			// do nothing, no extra chars have to be added.
			return 1;

		case TIME_HM:
		case TIME_MSEC:
		case TIME_DH:
		case TIME_DHCH:
		case TIME_HMCM:
			return 2;

		case TIME_DHM:
		case TIME_HMS:
		case TIME_DHMCM:
			return 3;

		case TIME_DHMS:
			return 4;
	} 
}

ElapsedFormatter.prototype.hasDecimals = function () {
	if (this.formatType == TIME_DCD || this.formatType == TIME_HCH || this.formatType == TIME_MCM ||
        this.formatType == TIME_DHCH || this.formatType == TIME_HMCM || this.formatType == TIME_DHMCM) {
		return true;
	}
	return false;
}

function DateFieldFormatterExtender(control, formatter, inputValidator) {

	// flag to trace whether the new value has been provided 
	// externally or internally, i.e. from handleInput.
	this.bSetInternally = false;

	if (control.isXType) {
		if (control.isXType('datefield')) {
			this.oFormatter = formatter;
			this.oInputValidator = inputValidator;

			// translate format into ExtJS date format.
			var sFormat = "";

			// day format
			var sDay = "d";
			switch (this.oFormatter.dayMode) {
				case DAY9:
				case DAYB9:
					// 1 digit for the day
					// blank before the digit may not be possible.
					sDay = "j";
					break;
			}
           
			// month format
			var sMonth = "m";
			switch (this.oFormatter.monthMode) {

				case MONTH9:
				case MONTHB9:
					// 1 digit for the month
					// blank before the digit may not be possible.
					sMonth = "n";
					break;
			}
            
			// year format
			var sYear = "Y"
			switch (this.oFormatter.yearMode) {

				case YEAR99:
					// 2 digits for the year
					sYear = "y";
					break;
			}

			switch (this.oFormatter.dateOrder) {
				case DATE_DMY:
					sFormat += sDay;
					sFormat += this.oFormatter.firstSep;
					sFormat += sMonth;
					sFormat += this.oFormatter.secSep;
					sFormat += sYear;
					break;

				case DATE_MDY:
					sFormat += sMonth;
					sFormat += this.oFormatter.firstSep;
					sFormat += sDay;
					sFormat += this.oFormatter.secSep;
					sFormat += sYear;
					break;

				case DATE_YMD:
					sFormat += sYear;
					sFormat += this.oFormatter.firstSep;
					sFormat += sMonth;
					sFormat += this.oFormatter.secSep;
					sFormat += sDay;
					break;
			}

			Ext.apply(control, { format: sFormat });
		}
	}

	this.onKeyPress = function (object, e, eOpts) {
		console.log("onKeyPress");
		if (this.oFormatter) {
			var cTyped = String.fromCharCode(e.charCode);
			var bValid = true;

			if (e.keyCode == 8 || e.keyCode == 46) {
				console.log('pressed char code is ' + e.keyCode);
			}
			if (e.altKey || e.ctrlKey || e.charCode < 28) {
				// alt/ctrl/arrows case
				// do nothing.
				return true;
			}
			if (cTyped.match(/SPECIAL/g)) {
				return true;
			} else {
				var bIsDateConvertible = false;
				if (this.oInputValidator) {
					// validate user input.
					bValid = this.oInputValidator.validate(cTyped);
				}
				if (bValid) {

					// get current textbox content.
					var oOldDate = object.getValue();
					var sOldValue = "";
					// get selection beginning and end.
					var iSelectionStart = object.inputEl.dom.selectionStart;
					var unformattedLength = 0;
					var iSelectionEnd = object.inputEl.dom.selectionEnd;
					var sNewContent = "";
					var dtValue = null;
					var iOldLength = 0;
					if (!(oOldDate instanceof Date) || (iSelectionStart != iSelectionEnd)) {
						// date picker content is not already a valid date or it may become
						// a non date as selection is not empty => there is a char replacement
						// and not an insertion.
						sOldValue = object.getRawValue();



						// handle the input.
						sNewContent = this.oFormatter.handleInput(sOldValue, cTyped, iSelectionStart, iSelectionEnd);

						dtValue = object.parseDate(sNewContent);
						if (dtValue) {
							this.bSetInternally = true;
							object.setValue(sNewContent);
						} else {
							object.setRawValue(sNewContent);
						}
						// new cursor position.                        
						if (sOldValue) {
							iOldLength = sOldValue.length;
						}
						unformattedLength = iSelectionStart + cTyped.length + (iOldLength - iSelectionEnd);
						object.inputEl.dom.selectionEnd = iSelectionStart + 1 + (sNewContent.length - unformattedLength);
					}
					if (oOldDate instanceof Date) {
						sOldValue = object.getRawValue();
						// handle the input.
						sNewContent = this.oFormatter.handleInput(sOldValue, cTyped, iSelectionStart, iSelectionEnd);
						// update the value only if the new one is still a valid date
						dtValue = object.parseDate(sNewContent);
						if (dtValue) {
							this.bSetInternally = true;
							object.setValue(sNewContent);
						} else {
							object.setValue(sOldValue);
						}
						iOldLength = sOldValue.length;
						unformattedLength = iSelectionStart + cTyped.length + (iOldLength - iSelectionEnd);
						object.inputEl.dom.selectionEnd = iSelectionStart + 1 + (sNewContent.length - unformattedLength);
					}
				}
			}
			e.preventDefault();
			return false;
		}
	}

	this.onBlur = function (object, event, opts) {
		console.log("onBlur");
		if (this.oFormatter) {
			// TODO: add isDirty() mechanism to avoid validate
			// a not modified string.

			var sInput = object.getValue();
			if (sInput != "") {
				var bValid = true;
				if (this.oInputValidator) {
					// we have a validator
					if (this.oInputValidator.validate(sInput)) {

					} else {

						bValid = false;
					}
				}
				// if (bValid) {
				if (true) {
					// succesfully validated, format it and set it to 
					// the textbox content. Formatting may be needed as 
					// the textbox content may come from a cut and paste.
					// var sFormatted = this.oFormatter.handleInput(sInput);
					var sFormatted = sInput;
					object.setValue(sFormatted);
				} else {
					// set a default value, if any.
					if (this.oFormatter.sNullValue) {
						object.setRawValue(this.oFormatter.sNullValue);
					} else {
						object.setRawValue("");
					}
				}
			}
			if (event) {
				event.preventDefault();
			}
			return false;
		}
	}

	this.onFocus = function (object, e, eOpts) {
		var sValue = object.getValue();
		if (this.oFormatter.sNullValue && sValue == this.oFormatter.sNullValue) {
			object.setValue("");
			e.preventDefault();
			return false;
		}
		if (this.oFormatter) {
			var sInput = this.oFormatter.removePrologueEpilogue(object.getValue());
			object.setValue(sInput);
			e.preventDefault();
			return false;
		}
	}

	this.onChange = function (obj, newValue, oldValue, eOpts) {
		this.bSetInternally = false;
		console.log("onChangeDate");
		//        alert('onChange');

	}

	// register event handlers for the control.
	control.on(
            {
            	keypress: this.onKeyPress,
            	blur: this.onBlur,
            	change: this.onChange,
            	// focus: this.onFocus,
            	scope: this

            });
}

////////////////////////////////////////////////////////////////////////
/// Date formatter class definition.
////////////////////////////////////////////////////////////////////////
function DateFormatter(oConfig) {
	if (oConfig) {
		// date order
		this.dateOrder = oConfig.formatType;
		// prologue to be applied to the date as string.
		this.prologue = oConfig.prefix;
		// epilogue to be applied to the date as string.
		this.epilogue = oConfig.postFix;
		//Date format year mode.
		this.yearMode = oConfig.yearFormat;
		//Date format month mode.
		this.monthMode = oConfig.monthFormat;
		//Date format day mode.
		this.dayMode = oConfig.dayFormat;
		//Date format first separator.
		this.firstSep = oConfig.firstSep;
		//Date format second separator.
		this.secSep = oConfig.secSep;
		this.sNullValue = "";
	}
}

DateFormatter.prototype = Object.create(StringFormatter.prototype);

DateFormatter.prototype.removePrologueEpilogue = function (sValue) {
	return sValue;
}

////////////////////////////////////////////////////////////////////////
/// Removes char separators from the input string.
/// sValue: The input string
////////////////////////////////////////////////////////////////////////
DateFormatter.prototype.unformat = function (sValue) {

	var sResult = sValue;
	if (sResult) {
		sResult = sResult.replace(this.firstSep, "--");
		sResult = sResult.replace(this.secSep, "--");
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Adds char separators to the input string.
/// sValue: The input string
////////////////////////////////////////////////////////////////////////
DateFormatter.prototype.format = function (sValue) {

	var sResult = sValue;
	sResult.replace("--", this.firstSep);
	sResult.replace("--", this.secSep);

	return sResult;
}

DateFormatter.prototype.handleInput = function (sOldValue, cTyped, iSelectionStart, iSelectionEnd) {

	if (sOldValue instanceof Date && (iSelectionStart == iSelectionEnd)) {
		// it is already a valid date, do not modify it.
		return sOldValue;
	}
	//  sOldValue = this.unformat(sOldValue);
	var sResult = cTyped;

	if (cTyped) {
		if (sOldValue) {
			var sNewBegin = sOldValue.slice(0, iSelectionStart);

			// get the portion of existing string after selection (tail).
			var sNewEnd = sOldValue.slice(iSelectionEnd, sOldValue.length);

			// insert new string in betweeen head and tail.
			var sNewValue = sNewBegin + cTyped + sNewEnd;
			sResult = this.format(sNewValue);
		}
	}
	return sResult;
}

////////////////////////////////////////////////////////////////////////
/// Converts the given string in a way accepted by the client DatePicker,
/// e.g. removes blank spaces from month representation and convert
/// the 3 digits year representation yyy to a 4 digits representation yyyy.
////////////////////////////////////////////////////////////////////////
DateFormatter.prototype.serverToClient = function (sValue) {
	var sResult = sValue;
	if (sValue) {

		if (this.monthMode == MONTHB9) {
			// month sanitization
			// blank space may have to be removed from the given string.
			if (this.firstSep != " " && this.secSep != " ") {
				// withe space can be in the string only because of month digits,
				// remove it.
				sResult = sResult.replace(" ", "");
			} else {
				// if one of the separators is a white space a bug does not allow to use B9 format
				// for month specification => this case is useless.
			}
			if (this.yearMode == YEAR999) {
				var sYear = "";
				var iYear = 0;
				switch (this.dateOrder) {
					case DATE_DMY:
					case DATE_MDY:
						sYear = sResult.slice(sValue.lastindexOf(this.secSep), sValue.length);
						var sHead = sResult.slice(0, sValue.lastindexOf(this.secSep));

						iYear = parseInt(sYear);
						if (iYear < 500) {
							iYear += 2000;
						} else {
							iYear += 1000;
						}
						sResult += sHead + this.secSep + iYear;
						break;

					case DATE_YMD:
						sYear = sResult.slice(0, sValue.indexOf(this.firstSep));
						var sTail = sResult.slice(sValue.indexOf(this.firstSep), sValue.length);
						iYear = parseInt(sYear);
						if (iYear < 500) {
							iYear += 2000;
						} else {
							iYear += 1000;
						}
						sResult = iYear + sTail;
						break;
				}
			}
		}
	}
	return sResult;
}

function TimeFormatter(oConfig) {
	if (oConfig) {
		this.timeFormat = oConfig.timeFormat;
		this.timeSeparator = oConfig.timeSeparator;

		this.timeAM = oConfig.timeAM;
		this.timePM = oConfig.timePM;
	}
}

TimeFormatter.prototype = Object.create(ElapsedFormatter.prototype);

TimeFormatter.prototype.format = function (sValue, bPaddingEnabled) {

	var sResult = "";
	var iInputLen = sValue.length;
	if (sValue) {

		// scan input string and look for
		// positions where to add time separators.
		var iOutputPos = 0;
		//    var sInteger = sValue;
		var sReminder = "";

		// limit first field range to avoid overflow.
		iDigitsLimit = 2;
		iDigitCount = 0;
		iCurrField = 0;
		iMaxValue = 23;
		iMaxFields = 3;

		if (this.timeFormat == HHMM_NODATE || this.timeFormat == BHMM_NODATE || this.timeFormat == HHMM || this.timeFormat == BHMM) {
			iMaxFields = 2;
		}

		var oResult = { integer: "" };
		for (var iInputPos = 0; iInputPos < iInputLen; iInputPos++) {

			if ((iDigitCount >= iDigitsLimit) && (sValue.charAt(iInputPos) != this.timeSeparator)) {
				// field change due to digits limit reached.
				iDigitsLimit = 2;
				iDigitCount = 0;

				// verify the current field is within its validity range.
				// max value for days, it has to do with an arithmetic 
				// overflow on TB.                    

				oResult.integer = this.floorValue(oResult.integer, iMaxValue);
				// need to add a separator.
				oResult.integer += this.timeSeparator;
				iCurrField++;
				if (iCurrField != 0) {
					// minutes or seconds field.
					iMaxValue = 59;
				}
			}

			if (sValue.charAt(iInputPos) == this.timeSeparator) {

				if (iDigitCount != iDigitsLimit && (iCurrField > 0)) {
					// the current field is not the first one, it must be completed 
					// with the missing digits, so ignore the time separator.
					iDigitCount--;
					continue;
				}

				// field change due to a found time separator.
				// This is the transition from the first field 
				// to the next one.
				oResult.integer = this.floorValue(oResult.integer, iMaxValue);

				iDigitsLimit = 2;
				iDigitCount = 0;
				// verify the current field is within its validity range.
				iCurrField++;
				if (iCurrField != 0) {
					// minutes or seconds field.
					iMaxValue = 59;
				}
				oResult.integer += sValue.charAt(iInputPos);
				continue;
			}
			if (iCurrField > iMaxFields - 1) {
				// remove last separator as we are at the 
				// end of the elapsed time.
				oResult.integer = oResult.integer.slice(0, -1);
				// get the reminder and exit from the loop.
				oResult.reminder = sValue.slice(iInputPos, iInputLen);

				if (this.timeFormat == HHMMSSTT_NODATE || this.timeFormat == HHMMSSTT || this.timeFormat == BHMMSSTT_NODATE || this.timeFormat == BHMMSSTT
                    || this.timeFormat == HMMSSTT_NODATE || this.timeFormat == HMMSSTT || this.timeFormat == HHMMTT_NODATE || this.timeFormat == HHMMTT
                    || this.timeFormat == BHMMTT_NODATE || this.timeFormat == BHMMTT || this.timeFormat == HMMTT_NODATE || this.timeFormat == HMMTT) {
					// add AM/PM as needed. The reminder is truncated as
					// it may contain previous AM/PM values which are not relevant anymore.
					var sDayPeriod = oResult.reminder.slice(-3);
					if (this.timeAM == sDayPeriod.trim() || this.timePM == sDayPeriod.trim()) {
						oResult.integer += oResult.reminder.slice(-3);
					}
				}
				break;
			}
			iDigitCount++;
			oResult.integer += sValue.charAt(iInputPos);

			if (iInputPos == iInputLen - 1) {
				// last input char, last field may need to be 
				// floored.
				oResult.integer = this.floorValue(oResult.integer, iMaxValue);
			}
		}
		sResult = oResult.integer;
	}

	return sResult;
}

TimeFormatter.prototype.addMissingChars = function (sInput) {
	var sResult = "";
	// var sResult = sInput;

	var iDigitsLimit = 2;
	var iInputLen = sInput.length;
	var iDigitCount = 0;
	var iCurrField = 0;
	var iAddedChars = 0;

	if (this.timeFormat == HMMSS_NODATE || this.timeFormat == HMM_NODATE || this.timeFormat == HMMSS || this.timeFormat == HMM) {
		// first field can be just one char long.
		iDigitsLimit = 1;
	}
	var sCurrField = ""
	// look for missing digits.
	for (var iInputPos = 0; iInputPos < iInputLen; iInputPos++) {

		if (sInput.charAt(iInputPos) == this.timeSeparator) {
			sCurrField = sResult.slice(iInputPos + iAddedChars - iDigitCount, iInputPos + iAddedChars);
			sResult = sResult.slice(0, iInputPos + iAddedChars - iDigitCount);
			while (iDigitCount < iDigitsLimit) {
				// add missing chars            
				if ((this.timeFormat == BHMMSS_NODATE || this.timeFormat == BHMMSS || this.timeFormat == BHMM_NODATE || this.timeFormat == BHMM ||
                    this.timeFormat == BHMMSSTT_NODATE || this.timeFormat == BHMMSSTT || this.timeFormat == BHMMTT_NODATE || this.timeFormat == BHMMTT)
                    && iCurrField == 0) {

					sResult += " ";
					iDigitCount++;
					iAddedChars++;
				} else {
					sResult += "0";
					iDigitCount++;
					iAddedChars++;
				}
			}
			sResult += sCurrField;
			// add separator.
			sResult += this.timeSeparator;

			// field change
			iDigitsLimit = 2;
			iDigitCount = 0;
			iCurrField++;
			continue;
		}
		sResult += sInput.charAt(iInputPos);
		iDigitCount++;
	}
	// add missing chars to last field
	sCurrField = sInput.slice(iInputLen - iDigitCount, iInputLen);
	var sHead = sResult.slice(0, sResult.length - iDigitCount);
	while (iDigitCount < iDigitsLimit) {
		sHead += "0";
		iDigitCount++;
	}
	sResult = sHead + sCurrField;
	iCurrField++;
	// TODO: add missing fields.
	var iCurrFieldCount = this.getCurrFieldsCount();
	while (iCurrField < iCurrFieldCount) {
		sResult += this.timeSeparator + "00";
		iCurrField++;
	}

	// add AM/PM if needed.
	if (this.timeFormat == HHMMSSTT_NODATE || this.timeFormat == HHMMSSTT || this.timeFormat == BHMMSSTT_NODATE || this.timeFormat == BHMMSSTT
        || this.timeFormat == HMMSSTT_NODATE || this.timeFormat == HMMSSTT || this.timeFormat == HHMMTT_NODATE || this.timeFormat == HHMMTT
        || this.timeFormat == BHMMTT_NODATE || this.timeFormat == BHMMTT || this.timeFormat == HMMTT_NODATE || this.timeFormat == HMMTT) {
		var sHour = sResult.split(this.timeSeparator)[0];
		var iHour = parseInt(sHour);
		if (iHour > 12) {
			sResult += " " + this.timePM;
		} else {
			sResult += " " + this.timeAM;
		}
	}
	return sResult;
}


////////////////////////////////////////////////////////////////////////
/// DateTime widget definition.
////////////////////////////////////////////////////////////////////////
Ext.define('MA.DateTime', {
	extend: 'Ext.Container',
	layout: {
		type: 'hbox'
	},
	dateTimeSep: ' ',
	items: [
    // date portion
                {
                	xtype: 'datefield',
                	name: 'embeddedDate',
                	disableKeyFilter: false,
                	enableKeyEvents: true,
                	disabled: false,
                	fieldLabel: 'Date field',
                	value: '12/5/2012',
                	// invalidText: "",
                	// preventMark: true,
                	format: 'd/m/Y'
                },

    // time portion
               {
               	xtype: 'textfield',
               	name: 'embeddedTime',
               	width: '10px',
               	enableKeyEvents: true
               }
	],

	getValue: function () {
		if (!this.getComponent(0) || !this.getComponent(1) || this.getComponent(0) == undefined || this.getComponent(1) == undefined) {
			// fields have to be set.
			return "";
		}
		return this.getComponent(0).getValue().toDateString() + " " + this.getComponent(1).getValue();

	},

	setValue: function (sValue) {
		if (!this.getComponent(0) || !this.getComponent(1) || this.getComponent(0) == undefined || this.getComponent(1) == undefined) {
			// fields have to be set.
			return "";
		}
		if (sValue) {
			var sFirstDateSep = this.dateFormatter.firstSep;
			var sSecDateSep = this.dateFormatter.secSep;
			// date and time are separated by a blank space,
			// but blank spaces may separate other subfields, 
			// so can't rely only on such a criteria.

			var iFirstSepIndex = sValue.indexOf(sFirstDateSep);
			var iInputLen = sValue.length;
			var sFirstRemainder = sValue;
			if (iFirstSepIndex > -1 && iFirstSepIndex < iInputLen - 1) {
				sFirstRemainder = sValue.slice(iFirstSepIndex + 1, iInputLen);
			}

			var iSecSepIndex = sFirstRemainder.indexOf(sSecDateSep);
			var iFirstRemLen = sFirstRemainder.length;
			var sSecRemainder = sFirstRemainder;
			if (iSecSepIndex > -1 && iSecSepIndex < iFirstRemLen - 1) {
				sSecRemainder = sFirstRemainder.slice(iSecSepIndex + 1, iFirstRemLen);
			}

			var iDateTimeSepIndex = sSecRemainder.indexOf(this.dateTimeSep);
			var iSecRemLen = sSecRemainder.length;
			var sDateTimeRemainder = sSecRemainder;
			if (iDateTimeSepIndex > -1 && iDateTimeSepIndex < iSecRemLen - 1) {
				sDateTimeRemainder = sSecRemainder.slice(iDateTimeSepIndex + 1, iSecRemLen);
			}

			if (sDateTimeRemainder != sSecRemainder && sFirstRemainder != sSecRemainder) {
				// split worked, separators were at their place, 
				// we have been able to identify a date and a time
				// from the input string.
				var sTime = sDateTimeRemainder;
				var sDate = sValue.split(this.dateTimeSep + sDateTimeRemainder)[0];
				this.getComponent(0).setValue(sDate);
				this.getComponent(1).setValue(sTime);
			}
		}
	},
	dateFormatter: null
});


////////////////////////////////////////////////////////////////////////
/// Text with an associated button widget definition.
////////////////////////////////////////////////////////////////////////
Ext.define('MA.TxtBtn', {
	extend: 'Ext.form.FieldContainer',
	mixins: {
		field: 'Ext.form.field.Field'
	},
	alias: 'widget.txtbtn',
	layout: 'hbox',	
	msgTarget: 'side',
	
	initComponent: function () {
	    var me = this;
	    if (!me.txtCfg) me.txtCfg = {};
	    // if (!me.btnCfg) me.btnCfg = {};
	    me.buildField();
	    me.callParent();
	    me.txtField = me.down('textfield');
	    // set the jsonItem for the text field.
	    me.txtField.jsonItem = me.txtCfg;
	    me.btnField = me.down('button');
	    //// set the jsonItem for the button field.
	    if (me.btnField) {
	        me.btnField.jsonItem = me.btnCfg;
	    }
		me.enabled = me.txtCfg.enabled;
		me.value = me.txtCfg.value;
		me.initField();
		me.customSetDisabled(!me.enabled);
		me.width = me.txtCfg.width ;		
		me.x = me.txtCfg.x;
		me.y = me.txtCfg.y;
	    me.fieldLabel = me.label
	},

	//@private
	buildField: function () {
	    var me = this;
	    var oTextField = 'textfield';
	    if (me.txtCfg.multiline) {
	        oTextField = 'textarea';
	    }

		me.items = [
                    Ext.apply({
                        xtype: oTextField,
                    	submitValue: false,
                    	width: 100,
                    	flex: 2
                    }, me.txtCfg)
		];

		if (me.btnCfg ) {
		    me.items[1] =
                Ext.apply({
		            xtype: 'button',
		            submitValue: false,
		            width: 15
		        }, me.btnCfg);

		}
	},

	customSetDisabled: function (bReadOnly) {
	    var me = this;
	    me.enabled = !bReadOnly;
	    me.txtField.setDisabled(bReadOnly);
	    if (me.btnField) {
	        me.btnField.setDisabled(bReadOnly);
	    }
	    me.setDisabled(bReadOnly);
	},	

	setValue: function (sValue) {
		var me = this;
		me.txtField.setValue(sValue);
	},

	setIcon: function (sIcon) {
	    var me = this;
	    if (me.btnField) {
	        me.btnField.setIcon(sIcon);
	    }
	},

	setButtonText: function (sText) {
		var me = this;
		if (me.btnField) {
		    me.btnField.setText(sText);
		}
	},

	setButtonTooltip: function (sText) {
		var me = this;
		if (me.btnField) {
		    me.btnField.setTooltip(sText);
		}
	},

	setButtonEnabled: function (bValue) {
		var me = this;
		if (me.btnField) {
		    me.btnField.setDisabled(!bValue);
		}
	}
});

////////////////////////////////////////////////////////////////////////
/// Text with an associated button widget definition.
////////////////////////////////////////////////////////////////////////
Ext.define('MA.TxtHKL', {
	extend: 'Ext.form.FieldContainer',
	mixins: {
		field: 'Ext.form.field.Field'
	},
	alias: 'widget.txthkl',
	layout: 'hbox',
	items: [Ext.create('Ext.Button', {
		text: 'btn'
	})],

	initComponent: function () {
		var me = this;
		if (!me.txtCfg) me.txtCfg = {};
		if (!me.upBtnCfg) me.upBtnCfg = {};
		if (!me.lowBtnCfg) me.lowBtnCfg = {};
		me.buildField();
		me.callParent();
		me.txtField = me.down('textfield');
		// set the jsonItem for the text field.
		me.txtField.jsonItem = me.txtCfg;
		me.upBtnField = me.down('button:first');
		me.lowBtnField = me.down('button:last');
                
		// set the jsonItem for the button field.
		me.upBtnField.jsonItem = me.upBtnCfg;
		me.lowBtnField.jsonItem = me.lowBtnCfg;
		me.enabled = !me.txtField.disabled;
		me.value = me.txtCfg.value;
		me.x = me.txtCfg.x;
		me.y = me.txtCfg.y;
		me.initField();
	},

	//@private
	buildField: function () {
	    var me = this;
	    var oTextField = 'textfield';
	    if (me.txtCfg.multiline) {
	        oTextField = 'textarea';
	    } 
	    
		me.items = [
                    Ext.apply({
                        xtype: oTextField,
                        submitValue: false
                        //,
                        //flex: 2
                    }, me.txtCfg),

                    Ext.create('Ext.container.Container', {
                    	layout: 'vbox',
                    	items: [
                            Ext.apply({
                            	xtype: 'button',
                            	submitValue: false
                            }, me.upBtnCfg),
                            Ext.apply({
                            	xtype: 'button',
                            	submitValue: false
                            }, me.lowBtnCfg)]
                    })];
	},

	setDisabled: function (bValue) {
		var me = this;
		me.enabled = !bValue;
		me.txtField.setDisabled(bValue);
		me.upBtnField.setDisabled(bValue);
		me.lowBtnField.setDisabled(bValue);
	},

	setValue: function (sValue) {
		var me = this;
		me.txtField.setValue(sValue);
	},

	getValue: function () {
	    var me = this;
	    var oValue = me.txtField.getValue();
	    return oValue;
	},
	setIcon: function (sIcon) {
		var me = this;
		//        me.btnField.setIcon(sIcon);
	}
});

//////////////////////////////////////////////
/// Combo Box in edit mode, Label with 
/// hyperlink when in readonly mode.
//////////////////////////////////////////////
Ext.define('MA.CBLink', {
	extend: 'Ext.form.FieldContainer',
	mixins: {
		field: 'Ext.form.field.Field'
	},
	alias: 'widget.cblink',
	layout: 'hbox',
	width: 200,
	
	initComponent: function () {
		var me = this;
		
		me.roComboCfg || (me.roComboCfg = {});
		me.editableComboCfg || (me.editableComboCfg = {});
		me.buildField();
		me.callParent();
	    me.fieldLabel = me.label,        
		me.readOnlyCB = me.down('label:last');
		me.editableCB = me.down('textfield');

		// updates the label value everytime the selection has changed.
		me.editableCB.on('select', function (combo, records, eOpts) {
			var oValue = records[0];
			me.setLabelValue(oValue.data.val);
		}, this);

		me.initField();
		me.customSetDisabled(me.disabled);
		me.x = me.editableComboCfg.x;
		me.y = me.editableComboCfg.y;
		me.width = me.editableComboCfg.width;
		console.log("initComponent");
	},

	//@private
	buildField: function () {
		var me = this;

		me.items = [

        //Ext.apply({
        //	xtype: 'label',
        //	width: 100,
        //	flex: 2,
        //	text: me.label
        //}, {}),

               Ext.apply({
               	xtype: 'label',
               	width: 100,
               	flex: 2               
               }, me.roComboCfg),

            Ext.apply({
            	xtype: 'combo',
            	submitValue: false,
            	flex: 2,
            	width: 100
            }, me.editableComboCfg)
		]
	},

	customSetDisabled: function (bReadOnly) {
		var me = this;
		me.readOnlyCB.setVisible(bReadOnly);
		me.readOnlyCB.setDisabled(!bReadOnly);
		me.editableCB.setVisible(!bReadOnly);
		me.editableCB.setDisabled(bReadOnly);
		me.editableCB.setEditable(!bReadOnly);
		me.setDisabled(bReadOnly);
	},

	setLabelValue: function (sValue) {
		var me = this;
		if (sValue) {
			var sNewValue = '<a href="javascript:documentEvents.onHyperLink(' + me.editableComboCfg.id + ')"><b>' + sValue + '</b></a>';
			me.readOnlyCB.setText(sNewValue, false);
		} else {
			me.readOnlyCB.setText("", false);
		}
	},

	setValue: function (sValue) {
		var me = this;
		me.editableCB.setValue(sValue);
		me.setLabelValue(sValue);
	}
});

//////////////////////////////////////////////
/// TextBox in edit mode, Label with 
/// hyperlink when in readonly mode.
//////////////////////////////////////////////
Ext.define('MA.EmailEdit', {
    extend: 'Ext.form.FieldContainer',
    mixins: {
        field: 'Ext.form.field.Field'
    },
    alias: 'widget.mailedit',
    layout: 'hbox',
    width: 200,
    height: 22,
    
    initComponent: function () {
        var me = this;       
        me.readOnlyTextCfg || (me.readOnlyTextCfg = {});
        me.editableTextCfg || (me.editableTextCfg = {});
        if (me.style) {
            me.editableTextCfg.fieldStyle = me.style;
            me.readOnlyTextCfg.style = me.style;
        }
        me.disabled = !me.editableTextCfg.enabled;
        me.fieldLabel= me.label,
        me.buildField();
        me.callParent();
        
        me.readOnlyText = me.down('label:last');
        me.editableText = me.down('textfield');

        // updates the label value everytime the selection has changed.
        me.editableText.on('select', function (sender, newValue, oldValue, eOpts) {
            me.setLabelValue(newValue);
        }, this);

        me.initField();
        me.setValue(me.editableTextCfg.value);
        me.customSetDisabled(me.disabled);
        safeAlert("MA.EmailEdit::initComponent");
    },

    //@private
    buildField: function () {
        var me = this;

        me.items = [

            Ext.apply({
                   xtype: 'label',
                   width: 100,
                   flex: 2
            }, me.readOnlyTextCfg),

           Ext.apply({
                xtype: 'textfield',
                submitValue: false,
                flex: 2,                
                width: 100
            }, me.editableTextCfg)
        ]
    },
    
    customSetDisabled: function (bReadOnly) {
        var me = this;
        me.readOnlyText.setVisible(bReadOnly);
        me.readOnlyText.setDisabled(!bReadOnly);
        me.editableText.setVisible(!bReadOnly);
        me.editableText.setDisabled(bReadOnly);
        me.setDisabled(bReadOnly);
    },

    setLabelValue: function (sValue) {
        var me = this;
        if (sValue) {
            var sNewValue = '<a href="mailto:' + sValue + '"><b>' + sValue + '</b></a>';
            me.readOnlyText.setText(sNewValue, false);
        } else {
            me.readOnlyText.setText("", false);
        }
    },

    setValue: function (sValue) {
        var me = this;
        me.editableText.setValue(sValue);
        me.setLabelValue(sValue);
    }
});

////////////////////////////////////////////////////////////////////////
/// Factory function for DateTime widget.
////////////////////////////////////////////////////////////////////////
createDateTimeField = function (sDateOrder, prologue, epilogue, sYearMode, sFirstSep, sMonthMode, sSecSep, sDayMode,
                        timeFormat, sTimeSeparator, sTimeAM, sTimePM) {

	var oDateTime = Ext.create('MA.DateTime', {       

	});
	var oTimeConfig = {timeFormat: timeFormat, timeSeparator: sTimeSeparator,  timeAM: sTimeAM, timePM: sTimePM};
	var timeFormatter = new TimeFormatter(oTimeConfig);
	//        currentExtItem.formatterAdapter = new TextBoxFormatterExtender(currentExtItem, oFormatter);
	// signMode, formatType, sThousandSeparator, cDecimalSeparator
	oAdapter = new TextBoxFormatterExtender(oDateTime.getComponent(1), timeFormatter, 
        new TimeValidator(oTimeConfig));
        
	var dateFormatter = new DateFormatter({formatType: sDateOrder, prefix: prologue, postFix: epilogue, yearFormat: sYearMode, 
		firstSep:sFirstSep, monthFormat: sMonthMode, secSep: sSecSep, dayFormat: sDayMode});
	//    //        currentExtItem.formatterAdapter = new TextBoxFormatterExtender(currentExtItem, oFormatter);
	// signMode, formatType, sThousandSeparator, cDecimalSeparator
	var oEmbeddedAdapter = new DateFieldFormatterExtender(oDateTime.getComponent(0), dateFormatter, new DateValidator({ firstSep: sFirstSep, sSecSep: sSecSep }));

	oDateTime.dateFormatter = dateFormatter;

	return oDateTime;
}