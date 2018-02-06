import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';
export declare class CoreHttpService {
    private httpService;
    controllerRoute: string;
    constructor(httpService: HttpService);
    isVatDuplicate(vat: string): Observable<Response>;
    checkVatEU(countryCode: string, vatNumber: string): Observable<Response>;
    checkVatRO(vatNumber: string, date: string): Observable<Response>;
}
