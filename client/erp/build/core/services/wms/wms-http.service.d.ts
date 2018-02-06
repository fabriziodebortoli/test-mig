import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';
export declare class WmsHttpService {
    private httpService;
    controllerRoute: string;
    constructor(httpService: HttpService);
    checkBinUsesStructure(zone: string, storage: string): Observable<Response>;
}
