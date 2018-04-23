import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';

@Injectable()
export class CoreHttpService {
    controllerRoute = '/erp-core/';

    constructor(private httpService: HttpService) {
    }

    isVatDuplicate(vat: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatDuplicate', vat);
    }

    checkVatEU(countryCode: string, vatNumber: string) : Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatEU', {'countryCode': countryCode, 'vatNumber': vatNumber})
    }

    checkVatRO(vatNumber: string, date: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckVatRO', {'cui': vatNumber, 'data': date});
    }
}