import { HttpService } from '@taskbuilder/core';
import { Injectable } from '@angular/core';

@Injectable()
export class InfoService {
    desktop: boolean;
    constructor(httpService: HttpService) {
    }



}