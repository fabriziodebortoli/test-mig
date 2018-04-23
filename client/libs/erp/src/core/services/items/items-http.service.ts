import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService, ParameterService } from '@taskbuilder/core';

@Injectable()
export class ItemsHttpService {
    controllerRoute = '/erp-core/';

    readonly DEFAULT_LEN_ITEM = 21;

    constructor(private httpService: HttpService, private parametersService: ParameterService) {
    }

    /*parametri*/
    async getItemInfo_CodeLength() {
        let len = await this.parametersService.getParameter('Ma_ItemParameters.CodeLength');
        if (len && (+len) > 3) {
            return +len;
        } else {
            // TODO GIANLUCA
            // lettura della lunghezza colonna da db....
            // verificare se compatibile con oracle 
            // int nlen = (pColInfo) ? pColInfo->GetColumnLength() : DEFAULT_LEN_ITEM;

            return this.DEFAULT_LEN_ITEM;
        }
    }

    async queryParam(param: string) {
        let ret = await this.parametersService.getParameter(param);
        return ret;
    }

    /*interrogazioni varie*/
    getItemsSearchList(queryType: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'GetItemsSearchList', queryType);
    }
}