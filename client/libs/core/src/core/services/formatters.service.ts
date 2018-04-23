import { Injectable } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';
//import { FormatterType } from '../../shared/models/formatters.enum'

import { HttpService } from './http.service';



@Injectable()
export class FormattersService {

    public formattersTable: any;
    constructor(public httpService: HttpService) { }

    loadFormattersTable() {
        this.httpService.getFormattersTable().subscribe((json) => {
            this.formattersTable = json.formatters;

            // let f = this.getFormatter(FormatterType.DateTime);
        });
    }

    async loadFormattersTableAsync() {
        if (!this.formattersTable) {
            let result = await this.httpService.getFormattersTable().toPromise();
            this.formattersTable = result.formatters;
        }
    }

    getFormatter(key: string): any {
        if (this.formattersTable && this.formattersTable[key])
            return this.formattersTable[key][0];

        return null;
    }
}

// export interface Formatter {
//     StyleName: string;
//     DataType: string;
//     Align: string;
//     Head: string;
//     Tail: string;
//     PaddedLen: string;
//     OutputWidth: string;
//     OutputCharLen: string;
//     InputWidth: string;
//     InputCharLen: string;
//     Owner: string;
//     Source: string;
//     LimitedContextArea: string;
//     CollateCulture: string;
//     ZeroPadded: string;
// }

// export enum FormatterType {
//     DashedCurrency,
//     ElapsedTime,
//     DashedCurrencyWithZero,
//     PositiveRoundedMoney,
//     PositiveMoney,
//     Percent,
//     Bool,
//     DashedPercent,
//     Integer,
//     ProtectedText,
//     DashedQuantity,
//     Double,
//     LongText,
//     NotAccountedMoney,
//     CONAIQty,
//     EuroLiteral,
//     Quantity3Decimals,
//     Quantity,
//     NotAccountableMoney,
//     Uuid,
//     NumberedEntity,
//     MoneyWithoutDecimal,
//     FiscalYear,
//     WeekDayName,
//     Text,
//     DateTime,
//     Long,
//     Fixing,
//     DateTimeExtended,
//     DashedInteger,
//     LongWithZero,
//     Currency,
//     NotFiscalNumber,
//     MoneyLiteralPostfix100,
//     NotAccountedCurrency,
//     MonthName,
//     Date,
//     Money,
//     Enum,
//     Time
// };

