import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';

@Injectable()
export class LogisticsHttpService {
    controllerRoute = '/erp-core/';

    constructor(private httpService: HttpService) {
    }

    checkItemsAutoNumbering(): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckItemsAutoNumbering');
    }

    getItemsSearchList(queryType: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'GetItemsSearchList', queryType);
    }

    checkBinUsesStructure(zone: string, storage: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckBinUsesStructure', { 'zone': zone, 'storage': storage });
    }
}
