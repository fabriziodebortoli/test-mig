/// *** Requires ***
/// FormatterTypes.js
/// Utils.js
function StringValidator() {

}

StringValidator.prototype.validate = function (cChar) {
    var bValid = false;

    return bValid;
}

////////////////////////////////////////////////////////////////////////
/// Integer validator constructor.
/// signMode: the sign mode to be used during validation.
/// formatType: The way integers are represented.
/// sThousandSeparator: the thousand separator used in integer 
///                     representation.
////////////////////////////////////////////////////////////////////////
function IntegerValidator(oConfig) {
    if (oConfig) {
        this.signMode = oConfig.signMode;
        this.formatType = oConfig.formatType;
        this.thousandSeparator = oConfig.thousandSeparator;
    }
}

////////////////////////////////////////////////////////////////////////
/// Validates the input string "sInput" as an integer. Sign symbols 
/// are threated according to "signMode" given to the constructor.
////////////////////////////////////////////////////////////////////////
IntegerValidator.prototype.validate = function (sInput) {
    var bValid = false;

    if (sInput == "-") {
        return true;
    }
    if (sInput) {
        if (sInput.length == 1) {
            // single char, it has to be a digit            
            var oDigitRegex = new RegExp("(\\d)");
            return oDigitRegex.test(sInput);
        }
    }
    // 1 to three digits.
    var sIntegerCore = "\\d{1,3}";
    var oIntegerRegex = null;
    var sRegexText = "";
    var sThousandSeparator = "";
    if (this.formatType == IntegerNumeric || this.formatType == RealFixed) {
        sThousandSeparator = this.thousandSeparator;
    }
    // TODO: refactor to a switch.
    if (this.signMode == SignMinusPrefix) {
        // "^(\\d{1,3}(\\.\\d{3})*)$"
        sRegexText = "^-?(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*)$";
    } else if (this.signMode == SignSignPrefix) {
        sRegexText = "^[-+]+(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*)$";
    } else if (this.signMode == SignMinusPostfix) {
        sRegexText = "^(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*)-?$";
    } else if (this.signMode == SignMinusPostfix) {
        sRegexText = "^(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*)[-+]+$";
    } else if (this.signMode == SignRounds) {
        sRegexText = "^(\\\((" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*)\\\)|(" +
                    sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*))$";
    }
    oIntegerRegex = new RegExp(sRegexText);
    bValid = oIntegerRegex.test(sInput);
    
    return bValid;
}

////////////////////////////////////////////////////////////////////////
/// Real validator constructor.
/// signMode: the sign mode to be used during validation.
/// sThousandSeparator: the thousand separator used in real 
///                     representation.
/// cDecimalSeparator: the decimal separator used in real 
///                     representation.
/// iDecimals: ho many decimal digits are allowed in real numbers.
////////////////////////////////////////////////////////////////////////
function RealValidator(oConfig) {
    if (oConfig) {
        this.signMode = oConfig.signMode;
        this.formatType = oConfig.formatType;
        this.thousandSeparator = oConfig.thousandSeparator;
        this.decimalSeparator = oConfig.decimalSeparator;
        this.decimals = oConfig.decimals;
    }
}

// inherits "IntegerFormatter" methods.
RealValidator.prototype = Object.create(IntegerValidator.prototype);
////////////////////////////////////////////////////////////////////////
/// Validates the input string "sInput" as a real number. Sign symbols 
/// are threated according to "signMode" given to the constructor.
////////////////////////////////////////////////////////////////////////
RealValidator.prototype.validate = function (sInput) {
    var bValid = false;

    if (sInput == "-") {
        return true;
    }
    if (sInput) {
        if (sInput.length == 1) {
            // single char, it has to be a digit or a decimal separator
            var oDigitRegex = new RegExp("(\\d|" + addEscapeForRegex(this.decimalSeparator) + ")");
            return oDigitRegex.test(sInput);
        }
    }
    // esempi col "." come separatore delle migliaia.

    // 1 to three digits.
    var sIntegerCore = "\\d{1,3}";
    var oIntegerRegex = null;
    var sRegexText = "";
    var sThousandSeparator = "";

    if (this.formatType == IntegerNumeric || this.formatType == RealFixed) {
        sThousandSeparator = this.thousandSeparator;
    }
    var sDecimalRegex = "";
    if (this.decimals > 0) {
        // decimal digits and all decimal section is
        // optional in validation because if missing and needed it will be added by the formatter.
        sDecimalRegex = "(" + addEscapeForRegex(this.decimalSeparator) + "\\d{0," + this.decimals + "})*";
    }
    switch (this.signMode) {

        case SignMinusPrefix:
            sRegexText = "^-?(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + ")$";
            break;

        case SignSignPrefix:
            sRegexText = "^[-+]+(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + ")$";
            break;

        case SignMinusPostfix:
            sRegexText = "^(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + ")-?$";
            break;

        case SignMinusPostfix:
            sRegexText = "^(" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + ")[-+]+$";
            break;

        case SignRounds:
            sRegexText = "^(\\\((" + sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + ")\\\)|(" +
                    sIntegerCore + "(" + addEscapeForRegex(sThousandSeparator) + "\\d{3})*" + sDecimalRegex + "))$";
            break;
    }
 
    oIntegerRegex = new RegExp(sRegexText);
    bValid = oIntegerRegex.test(sInput);

    return bValid;
}

function DateValidator(oConfig) {
    if (oConfig) {
        //Date format first separator.
        this.firstSep = oConfig.firstSep;
        //Date format second separator.
        this.secSep = oConfig.secSep;
    }
}

// input may a date too, not just a string.
DateValidator.prototype.validate = function (sInput) {
    var bValid = false;
    if (sInput) {

        if (sInput instanceof Date) {
            // if sInput is a date it is valid by definition.
            // this is the case when a DatePicker loses focus
            // and its content is a valid date.
            bValid = true;
        }
        if (sInput.length == 1) {
            // single char, it has to be a digit or a decimal separator
            var oDateChartRegex = new RegExp("(\\d|" + addEscapeForRegex(this.firstSep) + "|" + addEscapeForRegex(this.secSep) + ")");
            bValid = oDateChartRegex.test(sInput);
        }
        // if sInput does not fall into any of the ifs above 
        // the content is not a valid date.  => Validation failed.
    }
    return bValid;
}

function ElapsedValidator(oConfig) {
    if (oConfig) {
        this.formatType = oConfig.formatType;
        this.timeSeparator = oConfig.timeSeparator;
        this.decSeparator = oConfig.decSeparator;
        this.decNumber = parseInt(oConfig.decNumber);
    }
}

ElapsedValidator.prototype.validate = function (sInput) {
    var bValid = true;
    var oElapsedRegex;
    var sRegex = "";
    switch (this.formatType) {

        case TIME_D:
        case TIME_H:
        case TIME_M:
        case TIME_S:
            sRegex = "(\\d)+|" + addEscapeForRegex(this.timeSeparator);
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_DHMS:
            sRegex = "^(\\d){1,5}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,3}$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_DHM:
            sRegex = "^(\\d){1,5}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,2}$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_DH:
            sRegex = "^(\\d){1,5}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,1}$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_HMS:
            sRegex = "^(\\d){1,6}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,2}$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_HM:
        case TIME_MSEC:
            sRegex = "^(\\d){1,6}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,1}$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_DCD:
        case TIME_HCH:
        case TIME_MCM:
            // days and day fraction (decimals)
            // input is validated if it is a numeric optionally followed
            // by decimal separator and decimal digits. Alternatively it validates 
            // a single digit or the decimal separator.
            sRegex = "((^(\\d){1,5}(" + addEscapeForRegex(this.timeSeparator) + "(\\d)+){0,1}$)|(\\d|" + addEscapeForRegex(this.timeSeparator) + "|" +
                addEscapeForRegex(this.decSeparator) + "))";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case TIME_DHCH:
        case TIME_DHMCM:
        case TIME_HMCM:
            // days and day fraction (decimals)
            // input is validated if it is a numeric optionally followed
            // by decimal separator and decimal digits. Alternatively it validates 
            // a single digit or the decimal separator.
            sRegex = "^((\\d){1,5}(" + addEscapeForRegex(this.timeSeparator) + "(\\d){1,2}){0,1}$|(\\d|" + addEscapeForRegex(this.timeSeparator) + "|" +
                addEscapeForRegex(this.decSeparator) + "))";
            oElapsedRegex = new RegExp(sRegex);
            break;
    }
 
    return oElapsedRegex.test(sInput);
}

function TimeValidator(oConfig) {
    if (oConfig) {
        this.timeFormat = oConfig.timeFormat;
        this.timeSeparator = oConfig.timeSeparator;

        this.timeAM = oConfig.timeAM;
        this.timePM = oConfig.timePM;
    }
}

TimeValidator.prototype.validate = function (sInput) {
    var bValid = true;
    var oElapsedRegex;
    var sRegex = "";

    switch (this.timeFormat) {

        case HHMMSS_NODATE:
        case HHMMSS:
            sRegex = "^((\\d){2}" + addEscapeForRegex(this.timeSeparator) + "){2}" + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case BHMMSS_NODATE:
        case BHMMSS:
            sRegex = "^((\\d){2}" + addEscapeForRegex(this.timeSeparator) + "){2}" + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HHMMSSTT_NODATE:
        case HHMMSSTT:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
                addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case BHMMSSTT_NODATE:
        case BHMMSSTT:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
                addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HMMSSTT_NODATE:
        case HMMSSTT:
            sRegex = "^(\\d){1,2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
                addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HMMSS_NODATE:
        case HMMSS:
            sRegex = "^((\\d){1,2}" + addEscapeForRegex(this.timeSeparator) + "){2}" + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HHMM_NODATE:
        case HHMM:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case BHMM_NODATE:
        case BHMM:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HMM_NODATE:
        case HMM:
            sRegex = "^(\\d){1,2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HHMMTT_NODATE:
        case HHMMTT:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
            addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case BHMMTT_NODATE:
        case BHMMTT:
            sRegex = "^(\\d){2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
                addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

        case HMMTT_NODATE:
        case HMMTT:
            sRegex = "^(\\d){1,2}" + addEscapeForRegex(this.timeSeparator) + "(\\d){2}(" +
                addEscapeForRegex(this.timeAM) + "|" + addEscapeForRegex(this.timePM) + ")$|(^\\d$)|" + "^" + addEscapeForRegex(this.timeSeparator) + "$";
            oElapsedRegex = new RegExp(sRegex);
            break;

    }
   
    return oElapsedRegex.test(sInput);
}