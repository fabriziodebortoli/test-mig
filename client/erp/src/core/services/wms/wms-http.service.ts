import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';

@Injectable()
export class WmsHttpService {
    controllerRoute = '/erp-core/';

    constructor(private httpService: HttpService) {
    }

    checkBinUsesStructure(zone: string, storage: string): Observable<Response> {
        return this.httpService.execPost(this.controllerRoute, 'CheckBinUsesStructure', { 'zone': zone, 'storage': storage });
    }
}
