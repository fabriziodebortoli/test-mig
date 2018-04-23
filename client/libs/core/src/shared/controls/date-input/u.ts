import { padStart } from 'lodash';

export const NullDate = new Date("1799-12-31 00:00:00");

export function getDateFormatByFormatter(formatterProps: any, formatter: string = ''): string {
    let ret = '', day = '', month = '', year = '';
    let dateSep1 = formatterProps.refFirstSeparator ? formatterProps.refFirstSeparator : " ";
    let dateSep2 = formatterProps.refSecondSeparator ? formatterProps.refSecondSeparator : " ";
    let timeSep = formatterProps.refTimeSeparator ? formatterProps.refTimeSeparator : ":";

    day = String('d').repeat((<string>formatterProps.DayFormat).length - 3);    // 'DAY99'
    month = String('M').repeat((<string>formatterProps.MonthFormat).length - 5);  // 'MONTH99'
    year = String('y').repeat((<string>formatterProps.YearFormat).length - 4);   // 'YEAR99'


    switch (formatterProps.FormatType) {
      case "DATE_DMY": {
        ret = day + dateSep1 + month + dateSep2 + year;
        break;
      }
      case "DATE_YMD": {
        ret = year + dateSep1 + month + dateSep2 + day;
        break;
      }
      default: {
        ret = month + dateSep1 + day + dateSep2 + year;   // "DATE_MDY"
        break;
      }
    }

    if (formatter === "DateTime") {
      ret = ret + " HH" + timeSep + "mm";
    }

    if (formatter === "DateTimeExtended") {
      ret = ret + " HH" + timeSep + "mm" + timeSep + "ss";
    }

    return ret;
}

export const formatStringDate: (stringDate: string, formatterProps: any, formatter: string) => string = (stringDate, formatterProps, formatter) => {
    let date = new Date(stringDate);
    let dateSep1 = formatterProps.refFirstSeparator ? formatterProps.refFirstSeparator as string : ' ';
    let dateSep2 = formatterProps.refSecondSeparator ? formatterProps.refSecondSeparator as string : ' ';
    let timeSep = formatterProps.refTimeSeparator ? formatterProps.refTimeSeparator as string : ':';
    let day = padStart('' + date.getDay(), formatterProps.DayFormat.length - 3, '0');
    let month = padStart('' + date.getMonth(), formatterProps.MonthFormat.length - 5, '0');
    let year = (formatterProps.YearFormat.length - 4) === 2  ? JSON.stringify(date.getFullYear()).slice(2) : date.getFullYear();
    
    let formattedDates = {
        'DATE_DMY' : day + dateSep1 + month + dateSep2 + year,
        'DATE_YMD' : year + dateSep1 + month + dateSep2 + day,
        'DATE_MDY': month + dateSep1 + day + dateSep2 + year
    };
    let formattedTime = {
        'DateTime' : date.getHours() + timeSep + date.getMinutes(),
        'DateTimeExtended' : date.getHours() + timeSep + date.getMinutes() + timeSep + date.getSeconds()
    };

    let formattedDateStr = formattedDates[formatterProps.FormatType as string];
    let formattedTimeStr = formattedTime[formatter] ? ' ' + formattedTime[formatter] : '';
    
    return formattedDateStr ? formattedDateStr + formattedTimeStr : formattedDates['DATE_MDY']; 
}