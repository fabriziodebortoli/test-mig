/// Formatter Types
var textFormatter = "Text";
var dateFormatter = "Date";
var moneyFormatter = "Money";
var percentFormatter = "Percent";
var integerFormatter = "Integer";
var elapsedFormatter = "ElapsedTime";
var timeFormatterID = "Time";
var dateTimeFormatter = "DateTime";
var longFormatter = "Long";
var notAccountable = "NotAccountableMoney";
var quantity = "Quantity";

/// String Formatter types.
/// formatType: ASIS|UPPERCASE|LOWERCASE|CAPITALIZED|MASKED|EXPANDED
var StringAsIs = 0x0000;
var StringUpperCase = 0x0001;
var StringLowerCase = 0x0002;
var StringCapitalized = 0x0003;
var StringExpanded = 0x0004;
var StringMasked = 0x0005;

/// Integer formatter types.
/// formatType: NUMERIC|ZERO_AS_DASH|LETTER|ENCODED
var IntegerNumeric = 0x0000;
var IntegerZeroAsDash = 0x0099;
var IntegerLetter = 0x0001;
var IntegerEncoded = 0x0002;
var IntegerNone = "NONE";

/// signMode: ABSOLUTEVAL|MINUSPREFIX|SIGNPREFIX|ROUNDS|SIGNPOSTFIX|MINUSPOSTFIX
var SignAbs = 0x0000;
var SignMinusPrefix = 0x0001;
var SignMinusPostfix = 0x0002;
var SignRounds = 0x0003;
var SignSignPrefix = 0x0004;
var SignSignPostfix = 0x0005;

/// Real formatter types.
/// formatType: FIXED|ZERO_AS_DASH|LETTER|ENCODED|EXPONENTIAL|ENGINEER
var RealFixed = 0x0000;
var RealExponential = 0x0001;
var RealENGINEER = 0x0002;
var RealEncoded = 0x0003;
var RealLetter = 0x0004;
var RealZeroAsDash = 0x0099;
var RealNone = "NONE";

var RoundNone = 0x0000;
var RoundAbs = 0x0001;
var RoundSigned = 0x0002;
var RoundZero = 0x0003;
var RoundInf = 0x0004;


// Date Formatter
// Date Order.
var DATE_DMY = 0x0000;
var DATE_MDY = 0x0001;
var DATE_YMD = 0x0002;


// Day format
var DAY99 = 0x0000;
var DAYB9 = 0x0001;
var DAY9 = 0x0002;


// Month format
var MONTH99 = 0x0000;
var MONTHB9 = 0x0001;
var MONTH9 = 0x0002;

// Year format
var YEAR99 = 0x0000;
var YEAR999 = 0x0001;
var YEAR9999 = 0x0002;

// Elapsed time format
var TIME_D = 0X0001;
var TIME_H = 0X0002;
var TIME_M = 0x0004;
var TIME_S = 0x0008;
var TIME_CH = 0x0010;
var TIME_C = 0x1000;
var TIME_F = 0x2000;
var TIME_DEC = TIME_C | TIME_F | TIME_CH;
var TIME_DHMS = TIME_D | TIME_H | TIME_M | TIME_S;
var TIME_DHMSF = TIME_DHMS | TIME_F;
var TIME_DHMCM = TIME_DHMS | TIME_C;
var TIME_DHM = TIME_D | TIME_H | TIME_M;
var TIME_DHCH = TIME_DHM | TIME_C;
var TIME_DH = TIME_D | TIME_H;
var TIME_DCD = TIME_DH | TIME_C;
var TIME_HMS = TIME_H | TIME_M | TIME_S;
var TIME_HMSF = TIME_HMS | TIME_F;
var TIME_HMCM = TIME_HMS | TIME_C;
var TIME_HM = TIME_H | TIME_M;
var TIME_HCH = TIME_HM | TIME_C;
var TIME_MSEC = TIME_M | TIME_S;
var TIME_MSF = TIME_MSEC | TIME_F;
var TIME_MCM = TIME_MSEC | TIME_C;
var TIME_SF = TIME_S | TIME_F;

// time formats
var TIME_NONE = 0x0000;
var TIME_HF99 = 0x0001;
var TIME_HFB9 = 0x0002;
var TIME_HF9 = 0x0003;
var TIME_AMPM = 0x0010;
var TIME_ONLY = 0x0020;
var TIME_NOSEC = 0x0040;
var HHMMTT = TIME_HF99 | TIME_NOSEC | TIME_AMPM;
var BHMMTT = TIME_HFB9 | TIME_NOSEC | TIME_AMPM;
var HMMTT = TIME_HF9 | TIME_NOSEC | TIME_AMPM;
var HHMMSSTT = TIME_HF99 | TIME_AMPM;
var BHMMSSTT = TIME_HFB9 | TIME_AMPM;
var HMMSSTT = TIME_HF9 | TIME_AMPM;
var HHMM = TIME_HF99 | TIME_NOSEC;
var BHMM = TIME_HFB9 | TIME_NOSEC;
var HMM = TIME_HF9 | TIME_NOSEC;
var HHMMSS = TIME_HF99;
var BHMMSS = TIME_HFB9;
var HMMSS = TIME_HF9;
var HHMMTT_NODATE = HHMMTT | TIME_ONLY;
var BHMMTT_NODATE = BHMMTT | TIME_ONLY;
var HMMTT_NODATE = HMMTT | TIME_ONLY;
var HHMMSSTT_NODATE = HHMMSSTT | TIME_ONLY;
var BHMMSSTT_NODATE = BHMMSSTT | TIME_ONLY;
var HMMSSTT_NODATE = HMMSSTT | TIME_ONLY;
var HHMM_NODATE = HHMM | TIME_ONLY;
var BHMM_NODATE = BHMM | TIME_ONLY;
var HMM_NODATE = HMM | TIME_ONLY;
var HHMMSS_NODATE = HHMMSS | TIME_ONLY;
var BHMMSS_NODATE = BHMMSS | TIME_ONLY;
var HMMSS_NODATE = HMMSS | TIME_ONLY;