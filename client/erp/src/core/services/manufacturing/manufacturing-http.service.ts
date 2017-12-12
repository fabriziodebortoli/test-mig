import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Response } from '@angular/http';
import { HttpService } from '@taskbuilder/core';

@Injectable()
export class ManufacturingHttpService {
    controllerRoute = '/erp-core/';

    constructor(private httpService: HttpService) {
    }
}