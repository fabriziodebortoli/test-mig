import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';

@Injectable()
export class ItemsHttpService {
    controllerRoute = '/erp-core/';

    constructor(private httpService: HttpService) {
    }

    getItemsSearchList(queryType: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'GetItemsSearchList', queryType);
    }

    //TODO to be translated in a call to parameters service  
    checkItemsAutoNumbering(): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckItemsAutoNumbering');
    }
}